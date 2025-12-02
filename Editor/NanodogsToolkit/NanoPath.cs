// NanoPath.cs (Editor)
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

#if UNITY_2019_1_OR_NEWER
using UnityEditor.PackageManager;
#endif

public static class NanoPath
{
    private static string _rootAssetPath; // e.g. "Assets/Nanodogs Toolkit" or "Packages/com.your.package/Nanodogs Toolkit"
    private static string _rootFullPath;

    // Exact folder name in the Project window
    private const string ToolkitFolderName = "Nanodogs Toolkit";

    public static string RootAssetPath
    {
        get
        {
            if (string.IsNullOrEmpty(_rootAssetPath))
                _rootAssetPath = FindToolkitRootAssetPath();

            return _rootAssetPath;
        }
    }

    public static string RootFullPath
    {
        get
        {
            if (!string.IsNullOrEmpty(_rootFullPath))
                return _rootFullPath;

            if (string.IsNullOrEmpty(RootAssetPath))
                return null;

            // Assets/...
            if (RootAssetPath.StartsWith("Assets"))
            {
                // Application.dataPath is the full path to "Assets"
                string relative = RootAssetPath.Substring("Assets".Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, '/');

                _rootFullPath = string.IsNullOrEmpty(relative)
                    ? Application.dataPath
                    : Path.GetFullPath(Path.Combine(Application.dataPath, relative));
            }
            // Packages/...
            else if (RootAssetPath.StartsWith("Packages/"))
            {
#if UNITY_2019_1_OR_NEWER
                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(RootAssetPath);
                if (packageInfo != null)
                {
                    // packageInfo.assetPath: "Packages/com.your.package"
                    string rel = RootAssetPath.Substring(packageInfo.assetPath.Length)
                        .TrimStart('/', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    _rootFullPath = string.IsNullOrEmpty(rel)
                        ? packageInfo.resolvedPath
                        : Path.GetFullPath(Path.Combine(
                            packageInfo.resolvedPath,
                            rel.Replace('/', Path.DirectorySeparatorChar)));
                }
                else
                {
                    Debug.LogError($"NanoPath: Could not resolve package path for '{RootAssetPath}'.");
                    return null;
                }
#else
                Debug.LogError("NanoPath: Package paths require Unity 2019.1 or newer.");
                return null;
#endif
            }
            else
            {
                // Fallback: treat as relative to project folder
                _rootFullPath = Path.GetFullPath(RootAssetPath);
            }

            return _rootFullPath;
        }
    }

    public static string PrefabsAssetPath =>
        string.IsNullOrEmpty(RootAssetPath)
            ? null
            : Path.Combine(RootAssetPath, "Prefabs").Replace('\\', '/');

    public static string ImportFolderFullPath =>
        string.IsNullOrEmpty(RootFullPath)
            ? null
            : Path.Combine(RootFullPath, "Editor", "NanodogsToolkit", "Import");

    // ----------------------------------------------------------------------
    // Root detection: EXPLICIT folder named "Nanodogs Toolkit"
    // ----------------------------------------------------------------------
    private static string FindToolkitRootAssetPath()
    {
        var candidates = new List<string>();

        CollectToolkitFolders("Assets", candidates);
        CollectToolkitFolders("Packages", candidates);

        if (candidates.Count == 0)
        {
            Debug.LogError(
                $"NanoPath: Could not locate '{ToolkitFolderName}' folder. " +
                $"Expected a folder literally named '{ToolkitFolderName}' under 'Assets/' or 'Packages/'."
            );
            return null;
        }

        // Deduplicate
        candidates = candidates.Distinct().ToList();

        // If multiple, prefer Assets/ over Packages/
        string chosen = candidates
            .OrderBy(p => p.StartsWith("Assets/") ? 0 : 1)
            .First();

        if (candidates.Count > 1)
        {
            Debug.LogWarning(
                $"NanoPath: Multiple '{ToolkitFolderName}' folders found. Using: {chosen}" +
                "\nAll candidates:\n" + string.Join("\n", candidates)
            );
        }

        return chosen;
    }

    private static void CollectToolkitFolders(string root, List<string> results)
    {
        if (!AssetDatabase.IsValidFolder(root))
            return;

        foreach (var folder in AssetDatabase.GetSubFolders(root))
        {
            string normalized = folder.Replace('\\', '/');
            string name = Path.GetFileName(normalized);

            if (string.Equals(name, ToolkitFolderName, System.StringComparison.OrdinalIgnoreCase))
            {
                results.Add(normalized);
            }

            // Recurse into subfolders
            CollectToolkitFolders(normalized, results);
        }
    }
}
