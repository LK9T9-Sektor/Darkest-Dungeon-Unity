using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

/// <summary>
/// Keeps PlayerSettings in sync with the single version source (GameInfo).
/// Runs before every build and is also exposed as a manual editor menu item.
/// </summary>
public class GameInfoVersionSync : IPreprocessBuild
{
    /// <summary>
    /// Invoked by Unity before a build to push the version into PlayerSettings.
    /// </summary>
    public int callbackOrder { get { return 0; } }

    /// <summary>
    /// Synchronizes bundleVersion and Android bundle version code from GameInfo.
    /// </summary>
    [MenuItem("Tools/Game/Sync Version")]
    public static void SyncPlayerSettings()
    {
        PlayerSettings.bundleVersion = GameInfo.Version;
        PlayerSettings.Android.bundleVersionCode = GameInfo.AndroidBundleVersionCode;
        Debug.Log("Game version synced: " + PlayerSettings.bundleVersion +
            " (Android bundle version code " + PlayerSettings.Android.bundleVersionCode + ")");
    }

    /// <summary>
    /// Unity build callback entry point.
    /// </summary>
    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        SyncPlayerSettings();
    }
}
