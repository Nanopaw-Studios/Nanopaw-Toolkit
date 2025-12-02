// NanoPath.cs (Editor)
using UnityEditor;
using UnityEngine;
using System.IO;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;


#if UNITY_2019_1_OR_NEWER
using UnityEditor.PackageManager;
#endif

public static class NanoPath
{
    private static string _rootAssetPath; // e.g. "Assets/Plugins/Nanodogs-Toolkit" or "Packages/com.yourcompany.toolkit/Nanodogs-Toolkit"
    private static string _rootFullPath;  // e.g. "C:\Project\Assets\Plugins\Nanodogs-Toolkit" or actual package cache path

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
                // Application.dataPath is full path to "Assets".
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
                var packageInfo = PackageInfo.FindForAssetPath(RootAssetPath);
                if (packageInfo != null)
                {
                    // packageInfo.assetPath is like "Packages/com.yourcompany.toolkit"
                    string rel = RootAssetPath.Substring(packageInfo.assetPath.Length)
                        .TrimStart('/', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    // packageInfo.resolvedPath is the real filesystem path
                    _rootFullPath = string.IsNullOrEmpty(rel)
                        ? packageInfo.resolvedPath
                        : Path.GetFullPath(Path.Combine(packageInfo.resolvedPath, rel.Replace('/', Path.DirectorySeparatorChar)));
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

    private static string FindToolkitRootAssetPath()
    {
        // Search for package.json or README.md in both Assets and Packages
        string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets", "Packages" });

        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(assetPath);

            if (fileName == "package.json" || fileName == "README.md")
            {
                int idx = assetPath.IndexOf(ToolkitFolderName, System.StringComparison.OrdinalIgnoreCase);
                if (idx != -1)
                {
                    string root = assetPath
                        .Substring(0, idx + ToolkitFolderName.Length)
                        .Replace('\\', '/');

                    return root;
                }
            }
        }

        Debug.LogError(
            $"NanoPath: Could not locate '{ToolkitFolderName}' root. " +
            "Make sure package.json or README.md exists in the toolkit root (under Assets/ or Packages/)."
        );
        return null;
    }
}
