using System;
using System.IO;

using Sektor.DarkestDungeon.Clients.EventLog;
using Sektor.DarkestDungeon.Core.GameEvents;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game-event recorder host (docs\SPEC_GAME_EVENTS.md): creates a <see cref="GameEventBus"/>
/// plus <see cref="JsonEventLogger"/> run directory under the shared
/// <c>&lt;repository&gt;\captures\run_&lt;timestamp&gt;</c> folder, or
/// <c>persistentDataPath\Replays\run_&lt;timestamp&gt;</c> when the repository is not found.
/// Bootstrapped after the first scene load (same pattern as SceneSnapshotDumper) and records every
/// raid automatically: a run starts when a scene with a <see cref="RaidSceneManager"/> appears
/// (including a direct Play in the raid scene) and stops when that scene unloads, so leaving to the
/// menu finalizes the manifest.
/// </summary>
public class GameEventRecorder : MonoBehaviour
{
    private const string LogPrefix = "[GameEventLog]";
    private const string FallbackReplaysFolderName = "Replays";
    private const string RunTimestampFormat = "yyyy-MM-dd_HH-mm-ss";
    private const string RaidRunNotes = "auto-recorded raid run";
    private const string EstateRunNotes = "auto-recorded estate run";

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

    /// <summary>Gets the active run kind (<c>raid</c>/<c>estate</c>), or <c>null</c> when idle.</summary>
    public string Kind { get; private set; }

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

        // Play can start directly in a raid/estate scene: no sceneLoaded fires for it, and this host
        // is created after the scene Awake (the manager instance is set) but before its Start, so
        // the entry snapshot published from Start lands in the run.
        if (RaidSceneManager.Instanse != null)
            StartRecording(SceneManager.GetActiveScene(), GameEventRunKinds.Raid);
        else if (EstateSceneManager.Instanse != null)
            StartRecording(SceneManager.GetActiveScene(), GameEventRunKinds.Estate);

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
        if (bus == null || gameEvent == null)
            return;

        try
        {
            bus.Publish(gameEvent);
        }
        catch (Exception exception)
        {
            Debug.LogError(LogPrefix + " failed to publish " + gameEvent.GetType().Name
                + ": " + exception);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // sceneLoaded fires after the scene Awake, so the manager already points at the new raid
        // and the recorder is in place before RaidSceneManager.Start publishes. The scene check
        // keeps an unrelated (e.g. additive) scene from restarting the run.
        RaidSceneManager raid = RaidSceneManager.Instanse;
        if (raid != null && raid.gameObject.scene == scene)
        {
            RestartRecording(scene, GameEventRunKinds.Raid);
            return;
        }

        EstateSceneManager estate = EstateSceneManager.Instanse;
        if (estate != null && estate.gameObject.scene == scene)
            RestartRecording(scene, GameEventRunKinds.Estate);
    }

    private void RestartRecording(Scene scene, string kind)
    {
        // A stale run is finalized here too, in case a delayed sceneUnloaded arrives afterwards.
        if (IsRecording)
            StopRecording();

        StartRecording(scene, kind);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (IsRecording && scene == recordingScene)
            StopRecording();
    }

    private void StartRecording(Scene scene, string kind)
    {
        if (IsRecording)
            return;

        string runDirectory = string.Empty;
        try
        {
            string fallbackRoot = Path.Combine(Application.persistentDataPath, FallbackReplaysFolderName);
            string captureRoot = GameEventCaptureLocator.ResolveCaptureRoot(Application.dataPath, fallbackRoot);
            string runName = GameEventCaptureLocator.RunFolderPrefix + DateTime.Now.ToString(RunTimestampFormat);
            runDirectory = Path.Combine(captureRoot, runName);

            bus = new GameEventBus();
            string notes = string.Equals(kind, GameEventRunKinds.Estate, StringComparison.Ordinal)
                ? EstateRunNotes
                : RaidRunNotes;
            logger = new JsonEventLogger(bus, runDirectory, notes: notes, kind: kind);
        }
        catch (Exception exception)
        {
            bus = null;
            logger = null;
            Kind = null;
            Debug.LogError(LogPrefix + " failed to start recording in '" + runDirectory + "': " + exception);
            return;
        }

        recordingScene = scene;
        Kind = kind;
        Debug.Log(LogPrefix + " recording started (" + kind + "): " + logger.ManifestPath);
    }

    private void StopRecording()
    {
        if (logger == null)
            return;

        Debug.Log(LogPrefix + " recording stopped: " + logger.ManifestPath
            + " (files: " + string.Join(", ", logger.Files) + ")");

        try
        {
            logger.Dispose();
        }
        catch (Exception exception)
        {
            Debug.LogError(LogPrefix + " failed to finalize recording: " + exception);
        }
        finally
        {
            logger = null;
            bus = null;
            Kind = null;
        }
    }
}
