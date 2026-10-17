using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Diagnostic tool: writes full scene hierarchy snapshots (all scene roots, inactive objects
/// included, with per-component diagnostic properties) to JSON files under
/// &lt;persistentDataPath&gt;/SceneDumps/. Captures every loaded scene automatically at 0/5/15
/// seconds after load and takes a manual snapshot on F9. The DTO tree is built on the main thread
/// in chunks (the frame is yielded every <see cref="NodesPerYield"/> nodes) and the file write
/// happens on a background thread. See README.md next to this file.
/// </summary>
public class SceneSnapshotDumper : MonoBehaviour
{
    private const string DumpFolderName = "SceneDumps";
    private const string LogPrefix = "[SceneDump]";
    private const int NodesPerYield = 300;
    private const string DestroyedNodeName = "<destroyed>";

    private static readonly float[] AutoCaptureDelaysSeconds = { 0f, 5f, 15f };

    private SceneSnapshotPropertyReader _reader;
    private int _nodeCounter;

    /// <summary>Creates the non-destroyable dumper host right after the first scene load.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameObject host = new GameObject("SceneSnapshotDumper");
        DontDestroyOnLoad(host);
        host.AddComponent<SceneSnapshotDumper>();
    }

    private void Awake()
    {
        _reader = new SceneSnapshotPropertyReader();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Debug.Log(LogPrefix + " dumps folder: " + Path.Combine(Application.persistentDataPath, DumpFolderName));
        Scene scene = SceneManager.GetActiveScene();
        Debug.Log(LogPrefix + " auto-capture scheduled for scene '" + scene.name + "' (delays: 0/5/15 s).");
        StartCoroutine(CaptureRoutine(scene));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log(LogPrefix + " manual capture requested (F9).");
            StartCoroutine(DumpRoutine(SceneManager.GetActiveScene(), "manual"));
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(LogPrefix + " scene '" + scene.name + "' loaded (" + mode + ") — auto-capture scheduled.");
        StartCoroutine(CaptureRoutine(scene));
    }

    private IEnumerator CaptureRoutine(Scene scene)
    {
        float elapsed = 0f;
        for (int i = 0; i < AutoCaptureDelaysSeconds.Length; i++)
        {
            float delay = AutoCaptureDelaysSeconds[i] - elapsed;
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
                elapsed = AutoCaptureDelaysSeconds[i];
            }

            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.Log(LogPrefix + " scene '" + scene.name + "' is gone — auto-capture stopped.");
                yield break;
            }

            string trigger = "auto-t" + AutoCaptureDelaysSeconds[i].ToString(CultureInfo.InvariantCulture) + "s";
            yield return DumpRoutine(scene, trigger);
        }
    }

    private IEnumerator DumpRoutine(Scene scene, string trigger)
    {
        SceneSnapshot snapshot = new SceneSnapshot();
        snapshot.SceneName = scene.name;
        snapshot.Trigger = trigger;
        snapshot.TakenAtUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        snapshot.FrameCount = Time.frameCount;
        snapshot.TimeSinceLevelLoad = Time.timeSinceLevelLoad;

        _nodeCounter = 0;
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] == null)
            {
                continue;
            }

            SceneNodeSnapshot node = new SceneNodeSnapshot();
            yield return BuildNodeRoutine(roots[i], string.Empty, node);
            snapshot.Roots.Add(node);
        }

        snapshot.NodeCount = _nodeCounter;

        string json = JsonUtility.ToJson(snapshot, true);
        WriteSnapshotAsync(scene.name, trigger, json);
    }

    private IEnumerator BuildNodeRoutine(GameObject gameObject, string parentPath, SceneNodeSnapshot target)
    {
        if (gameObject == null)
        {
            target.Name = DestroyedNodeName;
            yield break;
        }

        Transform transform = gameObject.transform;
        target.Name = gameObject.name;
        target.Path = parentPath + "/" + gameObject.name;
        target.SiblingIndex = transform.GetSiblingIndex();
        target.ActiveSelf = gameObject.activeSelf;
        target.ActiveInHierarchy = gameObject.activeInHierarchy;
        target.Tag = gameObject.tag;
        target.Layer = gameObject.layer;
        target.LayerName = LayerMask.LayerToName(gameObject.layer);

        Component[] components = gameObject.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            target.Components.Add(BuildComponent(components[i]));
        }

        _nodeCounter++;
        if (_nodeCounter % NodesPerYield == 0)
        {
            yield return null;
        }

        int childIndex = 0;
        while (transform != null && childIndex < transform.childCount)
        {
            Transform childTransform = transform.GetChild(childIndex);
            childIndex++;
            if (childTransform == null)
            {
                continue;
            }

            SceneNodeSnapshot child = new SceneNodeSnapshot();
            yield return BuildNodeRoutine(childTransform.gameObject, target.Path, child);
            target.Children.Add(child);
        }
    }

    private ComponentSnapshot BuildComponent(Component component)
    {
        ComponentSnapshot snapshot = new ComponentSnapshot();
        if (component == null)
        {
            snapshot.Type = "<MissingScript>";
            snapshot.Enabled = false;
            return snapshot;
        }

        snapshot.Type = component.GetType().FullName;
        if (component is Behaviour behaviour)
        {
            snapshot.Enabled = behaviour.enabled;
        }

        try
        {
            _reader.Read(component, snapshot);
        }
        catch (Exception exception)
        {
            ComponentPropertySnapshot property = new ComponentPropertySnapshot();
            property.Name = "readerError";
            property.Value = exception.GetType().Name + ": " + exception.Message;
            snapshot.Properties.Add(property);
        }

        return snapshot;
    }

    private void WriteSnapshotAsync(string sceneName, string trigger, string json)
    {
        string folder = Path.Combine(Application.persistentDataPath, DumpFolderName);
        string fileName = SanitizeFileName(sceneName) + "-" + SanitizeFileName(trigger) + "-"
            + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture) + ".json";
        string path = Path.Combine(folder, fileName);
        string screenshotPath = Path.ChangeExtension(path, ".png");
        ScreenCapture.CaptureScreenshot(screenshotPath);

        Task.Run(() =>
        {
            try
            {
                Directory.CreateDirectory(folder);
                File.WriteAllText(path, json);
            }
            catch (Exception exception)
            {
                Debug.LogError(LogPrefix + " failed to write " + path + ": " + exception);
            }
        });

        Debug.Log(LogPrefix + " snapshot '" + sceneName + "' (" + trigger + ") queued: " + path);
    }

    private static string SanitizeFileName(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        for (int i = 0; i < invalid.Length; i++)
        {
            value = value.Replace(invalid[i], '_');
        }

        return value;
    }
}
