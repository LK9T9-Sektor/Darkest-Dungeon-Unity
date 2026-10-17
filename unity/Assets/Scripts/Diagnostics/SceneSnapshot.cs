using System;
using System.Collections.Generic;

/// <summary>
/// Root DTO of one scene hierarchy snapshot: scene identity, capture trigger and the full node
/// tree. Serialized to JSON with <see cref="UnityEngine.JsonUtility"/> by
/// <see cref="SceneSnapshotDumper"/>.
/// </summary>
[Serializable]
public class SceneSnapshot
{
    /// <summary>The name of the captured scene.</summary>
    public string SceneName;

    /// <summary>The capture trigger ("auto-t0s", "auto-t5s", "auto-t15s", "manual").</summary>
    public string Trigger;

    /// <summary>UTC capture time in ISO 8601 round-trip format.</summary>
    public string TakenAtUtc;

    /// <summary>The frame number at capture time.</summary>
    public int FrameCount;

    /// <summary>Seconds since level load at capture time.</summary>
    public float TimeSinceLevelLoad;

    /// <summary>Total number of captured nodes.</summary>
    public int NodeCount;

    /// <summary>The root nodes of the scene hierarchy.</summary>
    public List<SceneNodeSnapshot> Roots = new List<SceneNodeSnapshot>();
}
