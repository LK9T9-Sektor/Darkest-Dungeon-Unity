/// <summary>
/// Single source of truth for the game version.
/// Bump the numbers here; the pre-build editor hook (GameInfoVersionSync) pushes the
/// derived values to PlayerSettings.bundleVersion and the Android bundle version code.
/// </summary>
public static class GameInfo
{
    public const int Major = 1;
    public const int Minor = 0;
    public const int Patch = 4;

    /// <summary>
    /// "Major.Minor.Patch" string used by Photon to separate clients by game version.
    /// </summary>
    public static string Version { get { return Major + "." + Minor + "." + Patch; } }

    /// <summary>
    /// Strictly increasing integer Android bundle version code derived from Major/Minor/Patch
    /// (e.g. 1.0.4 -> 10004).
    /// </summary>
    public static int AndroidBundleVersionCode { get { return Major * 10000 + Minor * 100 + Patch; } }
}
