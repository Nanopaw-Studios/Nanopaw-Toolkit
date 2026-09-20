// © 2025 Nanodogs Studios. All rights reserved.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Nanodogs.Toolkit
{
    /// <summary>
    /// Nanodogs Toolkit NPC Creator Editor Script
    /// </summary>
    public class NanodogsToolkitNPCCreatorEditor : NDSEditorWindow
    {
        private string npcName = "New NPC";
        private int health = 100;
        private float speed = 5.0f;
        private bool backgroundCharacter = false;
        private bool spawnInScene = true;

        [MenuItem("Nanodogs/Tools/Utilites/NPC Creation", false, 1)]
        public static void ShowWindow()
        {
            GetWindow<NanodogsToolkitNPCCreatorEditor>("NPC Creator");
        }

        new private void OnGUI()
        {
            GUILayout.Label("NPC Creator", EditorStyles.boldLabel);
            GUILayout.Label("Create and customize NPCs for your game.", EditorStyles.label);
            GUILayout.Space(10);

            // GUI fields
            npcName = EditorGUILayout.TextField("NPC Name", npcName);
            health = EditorGUILayout.IntField("Health", health);
            speed = EditorGUILayout.FloatField("Speed", speed);
            backgroundCharacter = EditorGUILayout.Toggle("Background Character", backgroundCharacter);
            spawnInScene = EditorGUILayout.Toggle("Spawn in Scene", spawnInScene);

            GUILayout.Space(10);
            GUILayout.Label("(Your NPC will be saved to Prefabs/NPCs inside Nanodogs-Toolkit)", EditorStyles.miniLabel);

            if (GUILayout.Button("Create New NPC"))
            {
                CreateNewNPC();
            }

            GUILayout.Space(10);
            GUILayout.Label("NPC Settings", EditorStyles.boldLabel);
            // Additional settings here...

            base.OnGUI();
        }

        private void CreateNewNPC()
        {
            // Load base prefab
            string basePrefabPath = Path.Combine(NanoPath.PrefabsAssetPath, "NPCBase.prefab").Replace("\\", "/");
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);

            if (basePrefab == null)
            {
                Debug.LogError($"Base NPC prefab not found at path: {basePrefabPath}");
                return;
            }

            // Ensure NPCs folder exists
            string npcFolderPath = Path.Combine(NanoPath.PrefabsAssetPath, "NPCs").Replace("\\", "/");
            if (!AssetDatabase.IsValidFolder(npcFolderPath))
            {
                // Create the NPCs folder inside Prefabs
                AssetDatabase.CreateFolder(NanoPath.PrefabsAssetPath, "NPCs");
            }

            // Build prefab save path (asset path, not full OS path)
            string npcPrefabPath = Path.Combine(npcFolderPath, $"{npcName}.prefab").Replace("\\", "/");

            // Instantiate base prefab temporarily
            GameObject tempInstance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
            tempInstance.name = npcName;

            // Save as a new prefab asset
            GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(tempInstance, npcPrefabPath);
            Undo.RegisterCreatedObjectUndo(newPrefab, "Created NPC Prefab");

            // Destroy the temporary object after saving
            GameObject.DestroyImmediate(tempInstance);

            // Spawn in scene as prefab instance if enabled
            if (spawnInScene && newPrefab != null)
            {
                GameObject sceneInstance = PrefabUtility.InstantiatePrefab(newPrefab) as GameObject;
                sceneInstance.transform.position = Vector3.zero;
                sceneInstance.name = npcName;
                Undo.RegisterCreatedObjectUndo(sceneInstance, "Spawned NPC in Scene");
            }

            Debug.Log($"New NPC prefab created at: {npcPrefabPath}");
        }
    }

    public class NanoNPCData
    {
        public string npcName;
        public int health;
        public float speed;
        public bool backgroundCharacter;

        public NanoNPCData(string name, int hp, float spd, bool isBackground)
        {
            npcName = name;
            health = hp;
            speed = spd;
            backgroundCharacter = isBackground;
        }
    }
}
