// © 2025 Nanodogs Studios. All rights reserved.

using Nanodogs.Toolkit;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Nanodogs.Aurora
{
    public class AuroraWindow : NDSEditorWindow
    {
        private List<string> scenesInProject = new List<string>(); // List of all scenes in the project
        private List<bool> scenesToInclude = new List<bool>(); // List of bools to track which scenes to include in the box
        private BuildTarget buildTarget = BuildTarget.StandaloneWindows; // Default build target
        private string boxPath; // Path to the built box

        [MenuItem("Nanodogs/Aurora/Mod Creation", false, 30)]
        public static void ShowWindow()
        {
            GetWindow<AuroraWindow>("Mod Creation");
        }

        private void OnEnable()
        {
            // Populate the scenesInProject list with all scenes in the project
            foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes)
            {
                if (S.enabled)
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(S.path);
                    scenesInProject.Add(name);
                    scenesToInclude.Add(false);
                }
            }
        }

        new public void OnGUI()
        {
            GUILayout.Label("Mod Creation System", EditorStyles.boldLabel);

            // Display a toggle for each scene in the project
            for (int i = 0; i < scenesInProject.Count; i++)
            {
                scenesToInclude[i] = EditorGUILayout.Toggle(scenesInProject[i], scenesToInclude[i]);
            }

            // Allow the user to select the build target
            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Select Build Platform", buildTarget);

            if (GUILayout.Button("Build Asset Boxes"))
            {
                BuildAssetBoxes();
            }

            // If the box has been built, display a button to view the box
            if (!string.IsNullOrEmpty(boxPath) && GUILayout.Button("View Box"))
            {
                // Open the folder containing the box
                System.Diagnostics.Process.Start("explorer.exe", "/select," + boxPath.Replace('/', '\\'));
            }

            base.OnGUI();
        }

        private void BuildAssetBoxes()
        {
            // Only include the scenes that the user has selected
            List<string> scenesToBuild = new List<string>();
            for (int i = 0; i < scenesInProject.Count; i++)
            {
                if (scenesToInclude[i])
                {
                    scenesToBuild.Add("Assets/Scenes/" + scenesInProject[i] + ".unity");
                }
            }

            // First, we are gonna make the folders to put the packed box in.
            Directory.CreateDirectory("Nanodogs/Aurora/Packed/");
            // Build the asset boxes
            boxPath = "Nanodogs/Aurora/Packed/packed.box";
            BuildPipeline.BuildPlayer(scenesToBuild.ToArray(), boxPath, buildTarget, BuildOptions.BuildAdditionalStreamedScenes);
        }
    }
}
