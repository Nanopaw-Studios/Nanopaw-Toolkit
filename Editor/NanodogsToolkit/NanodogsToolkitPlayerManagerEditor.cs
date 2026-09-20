// © 2025 Nanodogs Studios. All rights reserved.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Nanodogs.UniversalScripts;
using UnityEditor.SceneManagement;

namespace Nanodogs.Toolkit
{
    public class NanodogsToolkitPlayerManagerEditor : NDSEditorWindow
    {
        public NanoPlayerTemplate playerTemplate;

        [MenuItem("Nanodogs/Tools/Utilites/Player Manager", false, -50)]
        public static void ShowWindow()
        {
            GetWindow<NanodogsToolkitPlayerManagerEditor>("NanoPlayer Manager");
        }

        new public void OnGUI()
        {
            EditorGUILayout.Space(15);
            GUILayout.Label("Choose your player template", EditorStyles.boldLabel);
            GUILayout.Label("Tip: go to Runtime/NanodogsToolkit/Templates to choose your type!", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);
            playerTemplate = (NanoPlayerTemplate)EditorGUILayout.ObjectField(playerTemplate, typeof(NanoPlayerTemplate), true);
            EditorGUILayout.Space(15);

            if(playerTemplate == null)
            {
                EditorGUILayout.HelpBox("Please assign a player template to enable spawnpoint creation.", MessageType.Warning);
            }
            else
            {
                if (GUILayout.Button("Create Spawnpoint"))
                {
                    CreateSpawnpoint();
                }
            }

            base.OnGUI();
        }

        private void CreateSpawnpoint()
        {
            if (playerTemplate == null)
            {
                Debug.LogError("Please assign a player template before creating a spawnpoint.");
                return;
            }
            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.position = Vector3.zero;
            spawnPoint.transform.rotation = Quaternion.identity;
            NanoPlayerSpawnpoint spawnpointComponent = spawnPoint.AddComponent<NanoPlayerSpawnpoint>();
            spawnpointComponent.playerTemplate = playerTemplate;
            // Select the newly created spawn point in the editor
            Selection.activeGameObject = spawnPoint;
            // Mark the scene as dirty to ensure changes are saved
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Spawnpoint created with assigned player template.");
        }
    }
}
