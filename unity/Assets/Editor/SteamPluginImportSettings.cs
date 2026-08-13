using UnityEngine;
using UnityEditor;

/// <summary>
/// Configures the imported steam_api64.dll native plugin for the Steam transport
/// (Standalone Windows x64 + Editor). Runs on import so the plugin never stays
/// with the default "Any platform" settings.
/// </summary>
public class SteamPluginImportSettings : AssetPostprocessor
{
    private const string SteamPluginPath = "Assets/Plugins/x86_64/steam_api64.dll";

    /// <summary>
    /// Applies the platform settings to the Steam native plugin after import.
    /// </summary>
    private void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string importedPath in importedAssets)
        {
            if (importedPath != SteamPluginPath)
                continue;

            PluginImporter importer = (PluginImporter)AssetImporter.GetAtPath(importedPath);
            if (importer == null)
                continue;

            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(true);
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, true);
            importer.SetPlatformData(BuildTarget.StandaloneWindows64, "CPU", "x86_64");
            importer.SaveAndReimport();

            Debug.Log("[STEAM] Plugin imported for Standalone Windows x64 (Editor + Player).");
        }
    }
}
