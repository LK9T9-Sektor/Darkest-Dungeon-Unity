using System;
using System.IO;

using Sektor.DarkestDungeon.Clients.EventLog;
using Sektor.DarkestDungeon.Core.GameEvents;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game-event recorder host (docs\SPEC_GAME_EVENTS.md): creates a <see cref="GameEventBus"/>
/// plus <see cref="JsonEventLogger"/> run directory under
/// <c>persistentDataPath\Replays\run_&lt;timestamp&gt;</c>. Bootstrapped after the first scene load
/// (same pattern as SceneSnapshotDumper) and records every raid automatically: a run starts when a
/// scene with a <see cref="RaidSceneManager"/> appears (including a direct Play in the raid scene)
/// and stops when that scene unloads, so leaving to the menu finalizes the manifest.
/// </summary>
public class GameEventRecorder : MonoBehaviour
{
    private const string LogPrefix = "[GameEventLog]";
    private const string ReplaysFolderName = "Replays";
    private const string RunFolderPrefix = "run_";
    private const string RunTimestampFormat = "yyyy-MM-dd_HH-mm-ss";
    private const string RunNotes = "auto-recorded raid run";

    private GameEventBus bus;
    private JsonEventLogger logger;
    private Scene recordingScene;

    /// <summary>Gets the live recorder instance (null before bootstrap / after destroy).</summary>
    public static GameEventRecorder Instance { get; private set; }

    /// <summary>Gets a value indicating whether a recording run is currently active.</summary>
    public bool IsRecording
    {
        get { return logger != null; }
    }

    /// <summary>Creates the non-destroyable recorder host right after the first scene load.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameObject host = new GameObject("GameEventRecorder");
        DontDestroyOnLoad(host);
        host.AddComponent<GameEventRecorder>();
    }

    private void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // Play can start directly in a raid scene: no sceneLoaded fires for it, and this host is
        // created after the scene Awake (RaidSceneManager.Instanse is set) but before its Start,
        // so the raid-start snapshot published from Start lands in the run.
        if (RaidSceneManager.Instanse != null)
            StartRecording(SceneManager.GetActiveScene());

        Debug.Log(LogPrefix + " ready. Raid runs are recorded automatically.");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            StopRecording();
            Instance = null;
        }
    }

    /// <summary>Publishes one event onto the run's bus; no-op when not recording.</summary>
    /// <param name="gameEvent">The stamped event record.</param>
    public void Publish(GameEvent gameEvent)
    {
        if (bus != null && gameEvent != null)
            bus.Publish(gameEvent);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // sceneLoaded fires after the scene Awake, so the manager already points at the new raid
        // and the recorder is in place before RaidSceneManager.Start publishes. The scene check
        // keeps an unrelated (e.g. additive) scene from restarting the run.
        RaidSceneManager raid = RaidSceneManager.Instanse;
        if (raid == null || raid.gameObject.scene != scene)
            return;

        // A stale run is finalized here too, in case a delayed sceneUnloaded arrives afterwards.
        if (IsRecording)
            StopRecording();

        StartRecording(scene);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (IsRecording && scene == recordingScene)
            StopRecording();
    }

    private void StartRecording(Scene scene)
    {
        if (IsRecording)
            return;

        string runName = RunFolderPrefix + DateTime.Now.ToString(RunTimestampFormat);
        string runDirectory = Path.Combine(Application.persistentDataPath, ReplaysFolderName, runName);

        try
        {
            bus = new GameEventBus();
            logger = new JsonEventLogger(bus, runDirectory, notes: RunNotes);
        }
        catch (Exception exception)
        {
            bus = null;
            logger = null;
            Debug.LogError(LogPrefix + " failed to start recording in '" + runDirectory + "': " + exception);
            return;
        }

        recordingScene = scene;
        Debug.Log(LogPrefix + " recording started: " + logger.ManifestPath);
    }

    private void StopRecording()
    {
        if (logger == null)
            return;

        Debug.Log(LogPrefix + " recording stopped: " + logger.ManifestPath
            + " (files: " + string.Join(", ", logger.Files) + ")");
        logger.Dispose();
        logger = null;
        bus = null;
    }
}
