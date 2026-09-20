using Nanodogs.Toolkit;
using UnityEditor;
using UnityEngine;

namespace Nanodogs.Console
{
    public class NanoConsoleEditorWindow : NDSEditorWindow
    {
        public static NanoConsoleSettings settings;

        [MenuItem("Nanodogs/NanoConsole/Settings", false, 30)]
        public static void ShowWindow()
        {
            GetWindow<NanoConsoleEditorWindow>("Manage NanoConsole");
        }

        new private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Configuration", EditorStyles.boldLabel);

            // Load or create settings asset
            if (settings == null)
            {
                settings = AssetDatabase.LoadAssetAtPath<NanoConsoleSettings>("Assets/Resources/NanoConsoleSettings.asset");
                if (settings == null)
                {
                    settings = CreateInstance<NanoConsoleSettings>();
                    AssetDatabase.CreateAsset(settings, "Assets/Resources/NanoConsoleSettings.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                // Display settings fields
                EditorGUI.BeginChangeCheck();
                settings.enableNanoConsole = EditorGUILayout.Toggle("Enable NanoConsole", settings.enableNanoConsole);
                settings.toggleKey = (KeyCode)EditorGUILayout.EnumPopup("Toggle Key", settings.toggleKey);
                settings.enableInEditor = EditorGUILayout.Toggle("Enable in Editor", settings.enableInEditor);
                settings.enableInBuild = EditorGUILayout.Toggle("Enable in Build", settings.enableInBuild);
                settings.enableLogs = EditorGUILayout.Toggle("Enable Logs", settings.enableLogs);
                settings.enableWarnings = EditorGUILayout.Toggle("Enable Warnings", settings.enableWarnings);
                settings.enableErrors = EditorGUILayout.Toggle("Enable Errors", settings.enableErrors);
                settings.maxLogs = EditorGUILayout.IntField("Max Logs", settings.maxLogs);
                settings.collapseLogs = EditorGUILayout.Toggle("Collapse Logs", settings.collapseLogs);
                settings.showTimestamps = EditorGUILayout.Toggle("Show Timestamps", settings.showTimestamps);
                settings.showStackTrace = EditorGUILayout.Toggle("Show Stack Trace", settings.showStackTrace);
                settings.backgroundColor = EditorGUILayout.ColorField("Background Color", settings.backgroundColor);
                settings.logColor = EditorGUILayout.ColorField("Log Color", settings.logColor);
                settings.warningColor = EditorGUILayout.ColorField("Warning Color", settings.warningColor);
                settings.errorColor = EditorGUILayout.ColorField("Error Color", settings.errorColor);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssets();
                }

                base.OnGUI();
            }

            GUILayout.Space(10);
            GUILayout.Label("Creation & Other", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if(GUILayout.Button("Create NanoConsole in Scene"))
            {
                if (FindFirstObjectByType<NanoConsole>() == null)
                {
                    GameObject consoleGO = new GameObject("NanoConsole");
                    consoleGO.AddComponent<NanoConsole>();
                    Selection.activeGameObject = consoleGO;
                }
                else
                {
                    EditorUtility.DisplayDialog("NanoConsole", "A NanoConsole already exists in the scene.", "OK");
                }
            }
        }
    }
}
