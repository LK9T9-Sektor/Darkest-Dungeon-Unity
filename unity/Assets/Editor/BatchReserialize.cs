using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEngine;

/// <summary>
/// Headless rewrite of owned prefabs and materials into the current Unity serialization
/// format. Invoked from the batch-mode script (tools\unity-reserialize.ps1) via
/// -executeMethod BatchReserialize.Run and also exposed as a manual editor menu item.
/// Vendor folders are skipped so upstream packages keep their original files.
/// </summary>
public static class BatchReserialize
{
    private const string _assetsPrefix = "Assets/";
    private const string _prefabExtension = ".prefab";
    private const string _materialExtension = ".mat";
    private const string _filterArgument = "-reserializePath";
    private const int _progressStep = 50;

    private static readonly string[] _excludedFolders =
    {
        "Assets/Plugins/",
        "Assets/Photon Unity Networking/",
        "Assets/PhotonChatApi/",
        "Assets/Spine/",
        "Assets/MobileDependencyResolver/",
    };

    /// <summary>
    /// Reserializes every owned prefab and material. Prefabs are saved back through
    /// <see cref="PrefabUtility"/> because <see cref="AssetDatabase.ForceReserializeAssets"/>
    /// leaves their text representation untouched; materials are rewritten in place. The
    /// batch run fails with exit code 1 when any asset could not be rewritten, so the
    /// wrapper script never reports a silent no-op.
    /// </summary>
    public static void Run()
    {
        string filter = GetArgument(_filterArgument);
        try
        {
            List<string> prefabPaths = CollectAssetPaths(_prefabExtension, filter);
            List<string> materialPaths = CollectAssetPaths(_materialExtension, filter);
            Debug.Log("[RESERIALIZE] Prefabs: " + prefabPaths.Count + ", materials: " + materialPaths.Count + ".");

            int failures = ReserializePrefabs(prefabPaths);
            if (materialPaths.Count > 0)
                AssetDatabase.ForceReserializeAssets(materialPaths, ForceReserializeAssetsOptions.ReserializeAssets);

            AssetDatabase.SaveAssets();

            if (failures > 0)
            {
                Debug.LogError("[RESERIALIZE] Failed: " + failures + " prefabs could not be rewritten.");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[RESERIALIZE] Done: " + (prefabPaths.Count + materialPaths.Count) + " assets.");
        }
        catch (Exception exception)
        {
            Debug.LogError("[RESERIALIZE] Failed: " + exception);
            EditorApplication.Exit(1);
        }
    }

    /// <summary>Manual reserialize entry point available from the editor menu.</summary>
    [MenuItem("Tools/Assets/Reserialize Prefabs and Materials")]
    public static void RunFromMenu()
    {
        Run();
    }

    private static int ReserializePrefabs(List<string> paths)
    {
        int failures = 0;
        for (int index = 0; index < paths.Count; index++)
        {
            string path = paths[index];
            try
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
            catch (Exception exception)
            {
                failures++;
                Debug.LogError("[RESERIALIZE] " + path + ": " + exception.Message);
            }

            if ((index + 1) % _progressStep == 0)
                Debug.Log("[RESERIALIZE] Progress: " + (index + 1) + " / " + paths.Count);
        }

        return failures;
    }

    private static List<string> CollectAssetPaths(string extension, string filter)
    {
        List<string> paths = new List<string>();
        foreach (string path in AssetDatabase.GetAllAssetPaths())
        {
            if (!path.StartsWith(_assetsPrefix, StringComparison.Ordinal))
                continue;
            if (!string.Equals(Path.GetExtension(path), extension, StringComparison.OrdinalIgnoreCase))
                continue;
            if (IsExcluded(path))
                continue;
            if (!string.IsNullOrEmpty(filter) && !path.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
                continue;

            paths.Add(path);
        }

        paths.Sort(StringComparer.Ordinal);
        return paths;
    }

    private static bool IsExcluded(string path)
    {
        foreach (string folder in _excludedFolders)
        {
            if (path.StartsWith(folder, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static string GetArgument(string name)
    {
        string[] arguments = Environment.GetCommandLineArgs();
        for (int index = 0; index < arguments.Length - 1; index++)
        {
            if (string.Equals(arguments[index], name, StringComparison.Ordinal))
                return arguments[index + 1];
        }

        return string.Empty;
    }
}
