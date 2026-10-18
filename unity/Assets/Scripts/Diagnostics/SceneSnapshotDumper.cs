using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Diagnostic tool: writes full scene hierarchy snapshots (all scene roots, inactive objects
/// included, with per-component diagnostic properties) to JSON files (+ a compact CSV layout
/// list next to each) under &lt;persistentDataPath&gt;/SceneDumps/. Captures every loaded scene
/// automatically when it has settled (first rendered frame with the forced final canvas layout),
/// again at 5/15 seconds after load, and on game view resolution changes; the DontDestroyOnLoad
/// pseudo-scene is captured alongside every snapshot. F9 takes a manual snapshot of the active
/// scene, <see cref="Capture"/> is the code-triggered entry point. The DTO tree is built on the
/// main thread in chunks (the frame is yielded every <see cref="NodesPerYield"/> nodes); JSON/CSV
/// writing happens on a background thread. See README.md next to this file.
/// </summary>
public class SceneSnapshotDumper : MonoBehaviour
{
    private const string DumpFolderName = "SceneDumps";
    private const string LogPrefix = "[SceneDump]";
    private const int NodesPerYield = 300;
    private const string DestroyedNodeName = "<destroyed>";
    private const string DontDestroyOnLoadSceneName = "DontDestroyOnLoad";
    private const string SettledTrigger = "auto-settled";

    private static readonly float[] AutoCaptureDelaysSeconds = { 5f, 15f };

    private static SceneSnapshotDumper _instance;

    private SceneSnapshotPropertyReader _reader;
    private int _nodeCounter;
    private int _lastScreenWidth;
    private int _lastScreenHeight;
    private Coroutine _resizeRoutine;

    /// <summary>Creates the non-destroyable dumper host right after the first scene load.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameObject host = new GameObject("SceneSnapshotDumper");
        DontDestroyOnLoad(host);
        host.AddComponent<SceneSnapshotDumper>();
    }

    /// <summary>
    /// Requests an immediate snapshot of the active scene (plus the DontDestroyOnLoad pseudo-scene)
    /// under the given trigger tag; meant for semantic moments in scene initialization code (for
    /// example once the raid HUD has been built).
    /// </summary>
    /// <param name="tag">The trigger tag recorded in the snapshot and in the file name.</param>
    public static void Capture(string tag)
    {
        if (_instance == null || string.IsNullOrEmpty(tag))
        {
            return;
        }

        Debug.Log(LogPrefix + " capture requested ('" + tag + "').");
        _instance.StartCoroutine(_instance.CaptureAllRoutine(SceneManager.GetActiveScene(), tag));
    }

    private void Awake()
    {
        _instance = this;
        _reader = new SceneSnapshotPropertyReader();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
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
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
        Scene scene = SceneManager.GetActiveScene();
        Debug.Log(LogPrefix + " auto-capture scheduled for scene '" + scene.name + "' (settled + 5/15 s).");
        StartCoroutine(CaptureRoutine(scene));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log(LogPrefix + " manual capture requested (F9).");
            StartCoroutine(CaptureAllRoutine(SceneManager.GetActiveScene(), "manual"));
        }

        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
        {
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            if (_resizeRoutine != null)
            {
                StopCoroutine(_resizeRoutine);
            }

            _resizeRoutine = StartCoroutine(ResizeCaptureRoutine(_lastScreenWidth, _lastScreenHeight));
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(LogPrefix + " scene '" + scene.name + "' loaded (" + mode + ") — auto-capture scheduled.");
        StartCoroutine(CaptureRoutine(scene));
    }

    private IEnumerator CaptureRoutine(Scene scene)
    {
        yield return WaitForSettledLayout();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.Log(LogPrefix + " scene '" + scene.name + "' is gone — auto-capture stopped.");
            yield break;
        }

        yield return CaptureAllRoutine(scene, SettledTrigger);

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
            yield return CaptureAllRoutine(scene, trigger);
        }
    }

    private IEnumerator ResizeCaptureRoutine(int width, int height)
    {
        Debug.Log(LogPrefix + " game view resized to " + width + "x" + height + " — capture scheduled.");
        yield return WaitForSettledLayout();
        yield return CaptureAllRoutine(SceneManager.GetActiveScene(), "resize-" + width + "x" + height);
        _resizeRoutine = null;
    }

    /// <summary>Waits for a rendered frame with the forced final canvas layout applied.</summary>
    private static IEnumerator WaitForSettledLayout()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        yield return null;
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator CaptureAllRoutine(Scene scene, string trigger)
    {
        yield return DumpRoutine(scene, trigger);
        yield return DumpDontDestroyOnLoadRoutine(trigger);
    }

    private IEnumerator DumpRoutine(Scene scene, string trigger)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.Log(LogPrefix + " scene '" + scene.name + "' is not loaded — snapshot skipped.");
            yield break;
        }

        yield return BuildAndWriteRoutine(scene.name, trigger, scene.GetRootGameObjects(), false, true);
    }

    private IEnumerator DumpDontDestroyOnLoadRoutine(string trigger)
    {
        List<GameObject> roots = new List<GameObject>();
        Scene scene = SceneManager.GetSceneByName(DontDestroyOnLoadSceneName);
        if (scene.IsValid() && scene.isLoaded)
        {
            GameObject[] sceneRoots = scene.GetRootGameObjects();
            for (int i = 0; i < sceneRoots.Length; i++)
            {
                roots.Add(sceneRoots[i]);
            }
        }
        else
        {
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                GameObject candidate = all[i];
                if (candidate != null && candidate.scene.name == DontDestroyOnLoadSceneName
                    && candidate.transform.parent == null)
                {
                    roots.Add(candidate);
                }
            }
        }

        if (roots.Count == 0)
        {
            Debug.Log(LogPrefix + " no DontDestroyOnLoad objects to capture.");
            yield break;
        }

        yield return BuildAndWriteRoutine(DontDestroyOnLoadSceneName, trigger, roots, true, false);
    }

    private IEnumerator BuildAndWriteRoutine(string sceneName, string trigger, IList<GameObject> roots,
        bool isDontDestroyOnLoad, bool captureScreenshot)
    {
        SceneSnapshot snapshot = new SceneSnapshot();
        snapshot.SceneName = sceneName;
        snapshot.Trigger = trigger;
        snapshot.TakenAtUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        snapshot.FrameCount = Time.frameCount;
        snapshot.TimeSinceLevelLoad = Time.timeSinceLevelLoad;
        snapshot.ScreenWidth = Screen.width;
        snapshot.ScreenHeight = Screen.height;
        snapshot.ScreenAspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 1f;
        snapshot.IsDontDestroyOnLoad = isDontDestroyOnLoad;

        _nodeCounter = 0;
        for (int i = 0; i < roots.Count; i++)
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
        WriteSnapshotAsync(snapshot, json, captureScreenshot);
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

    private void WriteSnapshotAsync(SceneSnapshot snapshot, string json, bool captureScreenshot)
    {
        string folder = Path.Combine(Application.persistentDataPath, DumpFolderName);
        string fileName = SanitizeFileName(snapshot.SceneName) + "-" + SanitizeFileName(snapshot.Trigger) + "-"
            + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture) + ".json";
        string path = Path.Combine(folder, fileName);
        if (captureScreenshot)
        {
            ScreenCapture.CaptureScreenshot(Path.ChangeExtension(path, ".png"));
        }

        Task.Run(() =>
        {
            try
            {
                Directory.CreateDirectory(folder);
                File.WriteAllText(path, json);
                File.WriteAllText(Path.ChangeExtension(path, ".csv"), SceneSnapshotLayoutWriter.Build(snapshot));
            }
            catch (Exception exception)
            {
                Debug.LogError(LogPrefix + " failed to write " + path + ": " + exception);
            }
        });

        Debug.Log(LogPrefix + " snapshot '" + snapshot.SceneName + "' (" + snapshot.Trigger + ") queued: " + path);
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
