using Nanodogs.Aurora;
using Nanodogs.Toolkit;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Nanodogs.Concrete
{
    /// <summary>
    /// Concrete is an editor window that makes building a lot easier.
    /// It can be accessed from the Nanodogs menu in the Unity Editor,
    /// and supplies various tools to streamline the building process.
    /// </summary>

    public class NanodogsToolkitConcreteEditor : NDSEditorWindow
    {
        bool Windows = true;    
        bool Mac = false;
        bool Linux = false;
        bool Android = false;
        bool iOS = false;
        bool WebGL = false;
        bool Switch = false;
        bool PS5 = false;
        bool Xbox = false;

        BuildOptions buildOptions = BuildOptions.None;
        BuildTarget buildTarget = BuildTarget.StandaloneWindows;

        [MenuItem("Nanodogs/Concrete/Building", false, 30)]
        public static void ShowWindow()
        {
            GetWindow<NanodogsToolkitConcreteEditor>("Concrete");
        }

        new private void OnGUI()
        {
            GUILayout.Label("Concrete - Build Helper", EditorStyles.boldLabel);
            GUILayout.Space(10);
            GUILayout.Label("Platform Selections (this will build for multiple platforms at once!)");
            Windows = EditorGUILayout.Toggle("Windows", Windows);
            Mac = EditorGUILayout.Toggle("Mac", Mac);
            Linux = EditorGUILayout.Toggle("Linux", Linux);
            Android = EditorGUILayout.Toggle("Android", Android);
            iOS = EditorGUILayout.Toggle("iOS", iOS);
            WebGL = EditorGUILayout.Toggle("WebGL", WebGL);
            Switch = EditorGUILayout.Toggle("Switch", Switch);
            PS5 = EditorGUILayout.Toggle("PS5", PS5);
            Xbox = EditorGUILayout.Toggle("Xbox", Xbox);

            GUILayout.Space(10);
            GUILayout.Label("Build Options");

            buildOptions = (BuildOptions)EditorGUILayout.EnumPopup("Build Options", buildOptions);

            // we need to specify the build target based on the selected platforms
            // this is a bit tricky since BuildPipeline.BuildPlayer only takes one target at a time
            // so what we will do is go through the selected platforms and build for each one
            if (Windows) buildTarget = BuildTarget.StandaloneWindows;
            else if (Mac) buildTarget = BuildTarget.StandaloneOSX;
            else if (Linux) buildTarget = BuildTarget.StandaloneLinux64;
            else if (Android) buildTarget = BuildTarget.Android;
            else if (iOS) buildTarget = BuildTarget.iOS;
            else if (WebGL) buildTarget = BuildTarget.WebGL;
            else if (Switch) buildTarget = BuildTarget.Switch;
            else if (PS5) buildTarget = BuildTarget.PS5;
            else if (Xbox) buildTarget = BuildTarget.XboxOne;
            else
            {
                EditorGUILayout.HelpBox("Please select at least one platform to build for.", MessageType.Warning);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Build for selected platforms"))
            {
                string[] scenes = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray();

                // Create a list of selected platforms
                var selectedPlatforms = new System.Collections.Generic.List<BuildTarget>();

                if (Windows) selectedPlatforms.Add(BuildTarget.StandaloneWindows);
                if (Mac) selectedPlatforms.Add(BuildTarget.StandaloneOSX);
                if (Linux) selectedPlatforms.Add(BuildTarget.StandaloneLinux64);
                if (Android) selectedPlatforms.Add(BuildTarget.Android);
                if (iOS) selectedPlatforms.Add(BuildTarget.iOS);
                if (WebGL) selectedPlatforms.Add(BuildTarget.WebGL);
                if (Switch) selectedPlatforms.Add(BuildTarget.Switch);
                if (PS5) selectedPlatforms.Add(BuildTarget.PS5);
                if (Xbox) selectedPlatforms.Add(BuildTarget.XboxOne);

                if (selectedPlatforms.Count == 0)
                {
                    EditorUtility.DisplayDialog("No Platform Selected", "Please select at least one platform.", "OK");
                    return;
                }

                // Loop through each selected platform and build
                foreach (var target in selectedPlatforms)
                {
                    string buildPath = "Builds/" + target.ToString()+ "/" + Application.productName + ".exe";

                    Debug.Log("Building for " + target);

                    var buildReport = BuildPipeline.BuildPlayer(
                        scenes,
                        buildPath,
                        target,
                        buildOptions
                    );

                    if (buildReport.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                    {
                        Debug.Log($"Build for {target} succeeded ({buildReport.summary.totalSize} bytes)");
                    }
                    else
                    {
                        Debug.LogError($"Build for {target} failed: {buildReport.SummarizeErrors()}");
                    }
                }
            }


            base.OnGUI();
        }
    }
}
