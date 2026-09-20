// NanodogsToolkitImporter.cs (Editor)
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Nanodogs.Toolkit
{
    public class NanodogsToolkitImporter
    {
        // Build full path to the import folder using NanoPath helper
        private static string BaseImportFolderFullPath => NanoPath.ImportFolderFullPath; // e.g. C:\Project\Assets\Plugins\Nanodogs-Toolkit\Editor\NanodogsToolkit\Import

        [MenuItem("Nanodogs/Importer/Import Volumetric Light Beam")]
        private static void ImportVolumetricLightBeam()
        {
            if (string.IsNullOrEmpty(BaseImportFolderFullPath))
            {
                Debug.LogError("Import folder path not found. NanoPath failed to resolve toolkit root.");
                return;
            }

            string packageFileName = "VolumetricLightBeam.unitypackage";
            string packageFullPath = Path.Combine(BaseImportFolderFullPath, packageFileName);

            if (!File.Exists(packageFullPath))
            {
                Debug.LogError($"Could not find package at: {packageFullPath}");
                // For debugging, print the dir contents:
                if (Directory.Exists(BaseImportFolderFullPath))
                {
                    Debug.Log("Files in import folder:");
                    foreach (var f in Directory.GetFiles(BaseImportFolderFullPath))
                        Debug.Log(" - " + f);
                }
                return;
            }

            AssetDatabase.ImportPackage(packageFullPath, false);
            Debug.Log("Imported package: " + packageFullPath);
        }

        // Example other menu item using the same pattern:
        [MenuItem("Nanodogs/Importer/Import Modular FPS Package")]
        private static void ImportModularFPS()
        {
            if (string.IsNullOrEmpty(BaseImportFolderFullPath)) { Debug.LogError("Import folder path not found."); return; }
            string packageFullPath = Path.Combine(BaseImportFolderFullPath, "Modular First Person Controller.unitypackage");
            if (!File.Exists(packageFullPath)) { Debug.LogError("Missing: " + packageFullPath); return; }
            AssetDatabase.ImportPackage(packageFullPath, false);
        }
    }
}
