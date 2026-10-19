using System;
using System.IO;

using Sektor.DarkestDungeon.Clients.EventLog;
using Sektor.DarkestDungeon.Core.GameEvents;

using UnityEngine;

/// <summary>
/// F10 game-event recorder host (docs\SPEC_GAME_EVENTS.md): creates a <see cref="GameEventBus"/>
/// plus <see cref="JsonEventLogger"/> run directory under
/// <c>persistentDataPath\Replays\run_&lt;timestamp&gt;</c> while recording. Bootstrapped after the
/// first scene load (same pattern as SceneSnapshotDumper); publishes nothing until started.
/// </summary>
public class GameEventRecorder : MonoBehaviour
{
    private const string LogPrefix = "[GameEventLog]";
    private const string ReplaysFolderName = "Replays";
    private const string RunFolderPrefix = "run_";
    private const string RunTimestampFormat = "yyyy-MM-dd_HH-mm-ss";
    private const string RunNotes = "first mission scenario: start -> corridor -> room -> battle -> torch";

    private GameEventBus bus;
    private JsonEventLogger logger;

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
        Debug.Log(LogPrefix + " ready. Press F10 to start/stop event recording.");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            StopRecording();
            Instance = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            if (IsRecording)
                StopRecording();
            else
                StartRecording();
        }
    }

    /// <summary>Publishes one event onto the run's bus; no-op when not recording.</summary>
    /// <param name="gameEvent">The stamped event record.</param>
    public void Publish(GameEvent gameEvent)
    {
        if (bus != null && gameEvent != null)
            bus.Publish(gameEvent);
    }

    private void StartRecording()
    {
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
