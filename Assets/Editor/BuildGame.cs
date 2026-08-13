using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

/// <summary>
/// Headless Windows x64 build entry point for the game. Invoked from the batch-mode
/// build script (tools\build-game.ps1) via -executeMethod BuildGame.Build and also
/// exposed as a manual editor menu item. The version is synchronized automatically
/// before the build by the GameInfoVersionSync preprocess callback.
/// </summary>
public static class BuildGame
{
    private const string _defaultBuildFolder = "Build/Darkest Dungeon";
    private const string _executableName = "Darkest Dungeon.exe";
    private const string _outputDirEnvironmentVariable = "DD_BUILD_DIR";

    /// <summary>
    /// Builds the Windows x64 standalone player into the configured output folder.
    /// The output directory can be overridden through the DD_BUILD_DIR environment
    /// variable; otherwise the default Build\Darkest Dungeon folder inside the
    /// project is used. Fails the batch-mode run with a non-zero exit code when the
    /// build does not succeed.
    /// </summary>
    public static void Build()
    {
        string outputFolder = Environment.GetEnvironmentVariable(_outputDirEnvironmentVariable);
        if (string.IsNullOrEmpty(outputFolder))
            outputFolder = _defaultBuildFolder;

        string outputPath = outputFolder + "/" + _executableName;
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[BUILD] No enabled scenes in EditorBuildSettings.");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log("[BUILD] Building " + scenes.Length + " scenes to " + outputPath);

        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.StandaloneWindows64, BuildOptions.None);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.LogError("[BUILD] Build failed: " + report.summary);
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log("[BUILD] Build succeeded: " + outputPath);
    }

    /// <summary>Manual build entry point available from the editor menu.</summary>
    [MenuItem("Tools/Game/Build Windows x64")]
    public static void BuildFromMenu()
    {
        Build();
    }
}
