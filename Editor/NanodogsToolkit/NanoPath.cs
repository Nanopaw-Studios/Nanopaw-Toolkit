// NanoPath.cs (Editor)
using UnityEditor;
using UnityEngine;
using System.IO;

#if UNITY_2019_1_OR_NEWER
using UnityEditor.PackageManager;
#endif

public static class NanoPath
{
    // e.g. "Assets/Plugins/Nanodogs-Toolkit" or "Packages/com.nanodogs.toolkit"
    private static string _rootAssetPath;

    // e.g. "C:\Project\Assets\Plugins\Nanodogs-Toolkit" or real package cache path
    private static string _rootFullPath;

    // Only used for logging now, not for path detection
    private const string ToolkitFolderName = "Nanodogs-Toolkit";

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
                    // packageInfo.assetPath: "Packages/com.nanodogs.toolkit"
                    string rel = RootAssetPath.Substring(packageInfo.assetPath.Length)
                        .TrimStart('/', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    // packageInfo.resolvedPath is the real filesystem path
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

    // --- Derived paths (null-safe) ---

    public static string PrefabsAssetPath =>
        string.IsNullOrEmpty(RootAssetPath)
            ? null
            : Path.Combine(RootAssetPath, "Prefabs").Replace('\\', '/');

    public static string ImportFolderFullPath =>
        string.IsNullOrEmpty(RootFullPath)
            ? null
            : Path.Combine(RootFullPath, "Editor", "NanodogsToolkit", "Import");

    // --- Root detection ---

    private static string FindToolkitRootAssetPath()
    {
        // Search for package.json or README.md in both Assets and Packages
        string[] searchFolders = { "Assets", "Packages" };
        string[] guids = AssetDatabase.FindAssets("t:TextAsset", searchFolders);

        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(assetPath);

            if (fileName != "package.json" && fileName != "README.md")
                continue;

            // Folder that contains package.json / README.md is considered the root
            string root = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(root))
                continue;

            // OPTIONAL: If you still want to enforce the folder name when under Assets:
            // if (root.StartsWith("Assets") && Path.GetFileName(root) != ToolkitFolderName)
            //     continue;

            return root;
        }

        Debug.LogError(
            $"NanoPath: Could not locate toolkit root. " +
            $"Make sure a package.json or README.md exists in the Nanodogs toolkit root (under Assets/ or Packages/)."
        );
        return null;
    }
}
