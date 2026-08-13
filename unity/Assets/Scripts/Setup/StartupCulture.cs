using System.Globalization;
using UnityEngine;

/// <summary>
/// Ensures all number parsing and formatting in the game uses the invariant culture.
/// Required after switching the scripting runtime from .NET 3.5 to .NET 4.6: Mono 4.x
/// honours the OS locale (e.g. ',' decimal separator on Russian systems), which breaks
/// parsing of dot-formatted content. See docs\RUNTIME_MIGRATION.md.
/// </summary>
public static class StartupCulture
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyInvariantCulture()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        Debug.Log("Game version: " + GameInfo.Version);
    }
}
