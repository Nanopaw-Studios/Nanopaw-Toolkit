// © 2025-2026 Nanodogs Studios. All rights reserved.

using System.Collections.Generic;
using System.IO;
using Nanodogs.Toolkit.Missions;
using Nanodogs.Toolkit.Missions.UI;
using UnityEditor;
using UnityEngine;

namespace Nanodogs.Toolkit
{
    /// <summary>
    /// Dedicated Editor Window for creating and managing NanoMissions and objectives
    /// without writing any code. Includes one-click scene setup tools.
    /// </summary>
    public class NanoMissionsEditorWindow : NDSEditorWindow
    {
        private List<NanoMissionData> _foundMissions = new List<NanoMissionData>();
        private NanoMissionData _selectedMission;
        private SerializedObject _serializedMission;

        private Vector2 _sidebarScroll;
        private Vector2 _contentScroll;
        private string _searchFilter = "";
        private string _newMissionName = "New Mission";
        private bool _showSceneTools = true;
        private string _statusMessage = "";
        private double _statusMessageTime = 0;

        [MenuItem("Nanodogs/Tools/Utilites/NanoMissions Creator", false, 2)]
        [MenuItem("Nanodogs/NanoMissions Editor", false, 20)]
        public static void ShowWindow()
        {
            var window = GetWindow<NanoMissionsEditorWindow>("NanoMissions");
            window.minSize = new Vector2(720, 500);
            window.RefreshMissionList();
        }

        private void OnEnable()
        {
            RefreshMissionList();
        }

        private void RefreshMissionList()
        {
            _foundMissions.Clear();
            string[] guids = AssetDatabase.FindAssets("t:NanoMissionData");

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<NanoMissionData>(path);
                if (asset != null)
                {
                    _foundMissions.Add(asset);
                }
            }

            if (_selectedMission != null && !_foundMissions.Contains(_selectedMission))
            {
                _selectedMission = _foundMissions.Count > 0 ? _foundMissions[0] : null;
            }

            UpdateSerializedObject();
        }

        private void UpdateSerializedObject()
        {
            if (_selectedMission != null)
            {
                _serializedMission = new SerializedObject(_selectedMission);
            }
            else
            {
                _serializedMission = null;
            }
        }

        new private void OnGUI()
        {
            DrawHeader();
            DrawSceneToolsBar();

            EditorGUILayout.Space(4);

            // Two-column layout
            EditorGUILayout.BeginHorizontal();

            // Left Sidebar: Mission List & Creation
            DrawSidebar();

            // Right Pane: Mission Details & Objectives Editor
            DrawContentPane();

            EditorGUILayout.EndHorizontal();

            DrawStatusMessage();

            base.OnGUI();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("NanoMissions Creator & Editor", EditorStyles.boldLabel);
            GUILayout.Label("Create, customize, and configure missions and objectives without writing any code.", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawSceneToolsBar()
        {
            _showSceneTools = EditorGUILayout.BeginFoldoutHeaderGroup(_showSceneTools, "Scene Setup & Spawning Tools");
            if (_showSceneTools)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                if (GUILayout.Button(new GUIContent("Setup MissionManager", "Adds or configures NanoMissionManager in current scene"), EditorStyles.toolbarButton))
                {
                    SetupMissionManagerInScene();
                }

                if (GUILayout.Button(new GUIContent("Setup Mission HUD (UGUI+TMP)", "Creates the complete HUD Canvas and quest tracker in current scene"), EditorStyles.toolbarButton))
                {
                    SetupHUDInScene();
                }

                if (GUILayout.Button(new GUIContent("Spawn TriggerZone", "Spawns a TriggerZone GameObject in the scene"), EditorStyles.toolbarButton))
                {
                    SpawnTriggerZoneInScene("Zone_A");
                }

                if (GUILayout.Button(new GUIContent("Spawn Collectable", "Spawns a Collectable item in the scene"), EditorStyles.toolbarButton))
                {
                    SpawnCollectableInScene("Coin");
                }

                if (GUILayout.Button(new GUIContent("Spawn Target", "Spawns a Target enemy/object in the scene"), EditorStyles.toolbarButton))
                {
                    SpawnTargetInScene("Enemy");
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(230));

            GUILayout.Label("Missions in Project", EditorStyles.boldLabel);

            // Creation section
            EditorGUILayout.BeginHorizontal();
            _newMissionName = EditorGUILayout.TextField(_newMissionName);
            if (GUILayout.Button("+", GUILayout.Width(28)))
            {
                CreateNewMissionAsset(_newMissionName);
            }
            EditorGUILayout.EndHorizontal();

            // Search bar
            _searchFilter = EditorGUILayout.TextField("Search:", _searchFilter);

            EditorGUILayout.Space(5);

            _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll);

            for (int i = 0; i < _foundMissions.Count; i++)
            {
                var mission = _foundMissions[i];
                if (mission == null) continue;

                if (!string.IsNullOrEmpty(_searchFilter) &&
                    !mission.missionTitle.ToLowerInvariant().Contains(_searchFilter.ToLowerInvariant()) &&
                    !mission.missionId.ToLowerInvariant().Contains(_searchFilter.ToLowerInvariant()))
                {
                    continue;
                }

                bool isSelected = _selectedMission == mission;

                GUI.backgroundColor = isSelected ? new Color(0.35f, 0.7f, 1f) : Color.white;

                string label = string.IsNullOrEmpty(mission.missionTitle) ? mission.name : mission.missionTitle;
                if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.Height(26)))
                {
                    _selectedMission = mission;
                    UpdateSerializedObject();
                    GUI.FocusControl(null);
                }

                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Refresh List", EditorStyles.miniButton))
            {
                RefreshMissionList();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawContentPane()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (_selectedMission == null || _serializedMission == null)
            {
                EditorGUILayout.Space(20);
                GUILayout.Label("Select a mission from the list or click '+' to create a new one.", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
                return;
            }

            _serializedMission.Update();

            _contentScroll = EditorGUILayout.BeginScrollView(_contentScroll);

            // Top action buttons for selected mission
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Editing: {_selectedMission.name}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Ping Asset", EditorStyles.miniButton))
            {
                EditorGUIUtility.PingObject(_selectedMission);
            }

            if (GUILayout.Button("Add to Scene Manager", EditorStyles.miniButton))
            {
                AddMissionToSceneManager(_selectedMission);
            }

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                if (EditorUtility.DisplayDialog("Delete Mission", $"Delete mission '{_selectedMission.missionTitle}' permanently?", "Delete", "Cancel"))
                {
                    string path = AssetDatabase.GetAssetPath(_selectedMission);
                    AssetDatabase.DeleteAsset(path);
                    RefreshMissionList();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                    return;
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Mission Details Section
            EditorGUILayout.LabelField("Mission Identity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serializedMission.FindProperty("missionId"));
            EditorGUILayout.PropertyField(_serializedMission.FindProperty("missionTitle"));
            EditorGUILayout.PropertyField(_serializedMission.FindProperty("category"));
            EditorGUILayout.PropertyField(_serializedMission.FindProperty("description"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Progression & Flow", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serializedMission.FindProperty("autoStart"));
            EditorGUILayout.PropertyField(_serializedMission.FindProperty("sequentialObjectives"));
            EditorGUILayout.PropertyField(_serializedMission.FindProperty("nextMission"));
            EditorGUILayout.PropertyField(_serializedMission.FindProperty("rewardText"));

            EditorGUILayout.Space(10);

            // Objectives Section
            DrawObjectivesEditor();

            EditorGUILayout.Space(15);

            EditorGUILayout.EndScrollView();

            // Bottom Save Bar
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (GUILayout.Button("Save Mission Changes", GUILayout.Height(30)))
            {
                _serializedMission.ApplyModifiedProperties();
                EditorUtility.SetDirty(_selectedMission);
                AssetDatabase.SaveAssets();
                ShowStatus("Mission saved successfully!");
            }
            EditorGUILayout.EndHorizontal();

            _serializedMission.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
        }

        private void DrawObjectivesEditor()
        {
            var objectivesProp = _serializedMission.FindProperty("objectives");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Objectives ({objectivesProp.arraySize})", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+ Add Objective", EditorStyles.miniButton))
            {
                objectivesProp.arraySize++;
                var newObjProp = objectivesProp.GetArrayElementAtIndex(objectivesProp.arraySize - 1);
                newObjProp.FindPropertyRelative("id").stringValue = System.Guid.NewGuid().ToString().Substring(0, 8);
                newObjProp.FindPropertyRelative("title").stringValue = "New Objective";
                newObjProp.FindPropertyRelative("type").enumValueIndex = (int)ObjectiveType.ReachLocation;
                newObjProp.FindPropertyRelative("radius").floatValue = 3.5f;
                newObjProp.FindPropertyRelative("requiredAmount").intValue = 1;
                newObjProp.FindPropertyRelative("timerDuration").floatValue = 30f;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            for (int i = 0; i < objectivesProp.arraySize; i++)
            {
                var objProp = objectivesProp.GetArrayElementAtIndex(i);
                var typeProp = objProp.FindPropertyRelative("type");
                var titleProp = objProp.FindPropertyRelative("title");
                var idProp = objProp.FindPropertyRelative("id");

                EditorGUILayout.BeginVertical(GUI.skin.box);

                // Title row
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"#{i + 1}", EditorStyles.boldLabel, GUILayout.Width(24));
                EditorGUILayout.PropertyField(titleProp, GUIContent.none);
                EditorGUILayout.PropertyField(typeProp, GUIContent.none, GUILayout.Width(130));

                // Up/Down/Delete
                GUI.enabled = i > 0;
                if (GUILayout.Button("^", EditorStyles.miniButton, GUILayout.Width(22)))
                {
                    objectivesProp.MoveArrayElement(i, i - 1);
                    break;
                }
                GUI.enabled = i < objectivesProp.arraySize - 1;
                if (GUILayout.Button("v", EditorStyles.miniButton, GUILayout.Width(22)))
                {
                    objectivesProp.MoveArrayElement(i, i + 1);
                    break;
                }
                GUI.enabled = true;

                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(22)))
                {
                    objectivesProp.DeleteArrayElementAtIndex(i);
                    GUI.backgroundColor = Color.white;
                    break;
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();

                // Details per type
                ObjectiveType currentType = (ObjectiveType)typeProp.enumValueIndex;

                switch (currentType)
                {
                    case ObjectiveType.ReachLocation:
                        var posProp = objProp.FindPropertyRelative("targetPosition");
                        var radProp = objProp.FindPropertyRelative("radius");
                        EditorGUILayout.PropertyField(posProp);
                        EditorGUILayout.PropertyField(radProp);

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Set from Selected Object", EditorStyles.miniButton))
                        {
                            if (Selection.activeTransform != null)
                            {
                                posProp.vector3Value = Selection.activeTransform.position;
                                ShowStatus($"Target position set from {Selection.activeTransform.name}");
                            }
                            else
                            {
                                ShowStatus("Please select a GameObject in the scene first.");
                            }
                        }
                        if (GUILayout.Button("Set from Scene Camera", EditorStyles.miniButton))
                        {
                            if (SceneView.lastActiveSceneView != null)
                            {
                                posProp.vector3Value = SceneView.lastActiveSceneView.camera.transform.position;
                                ShowStatus("Target position set from Scene View Camera.");
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        break;

                    case ObjectiveType.TriggerZone:
                        var zoneIdProp = objProp.FindPropertyRelative("targetZoneId");
                        EditorGUILayout.PropertyField(zoneIdProp);

                        if (GUILayout.Button($"Spawn TriggerZone ('{zoneIdProp.stringValue}') in Scene", EditorStyles.miniButton))
                        {
                            SpawnTriggerZoneInScene(zoneIdProp.stringValue);
                        }
                        break;

                    case ObjectiveType.CollectItems:
                        var tagProp = objProp.FindPropertyRelative("targetTag");
                        var amountProp = objProp.FindPropertyRelative("requiredAmount");
                        EditorGUILayout.PropertyField(tagProp, new GUIContent("Item Tag"));
                        EditorGUILayout.PropertyField(amountProp, new GUIContent("Required Count"));

                        if (GUILayout.Button($"Spawn Collectable ('{tagProp.stringValue}') in Scene", EditorStyles.miniButton))
                        {
                            SpawnCollectableInScene(tagProp.stringValue);
                        }
                        break;

                    case ObjectiveType.DefeatTargets:
                        var enemyTagProp = objProp.FindPropertyRelative("targetTag");
                        var enemyAmountProp = objProp.FindPropertyRelative("requiredAmount");
                        EditorGUILayout.PropertyField(enemyTagProp, new GUIContent("Target Tag"));
                        EditorGUILayout.PropertyField(enemyAmountProp, new GUIContent("Required Defeats"));

                        if (GUILayout.Button($"Spawn Target ('{enemyTagProp.stringValue}') in Scene", EditorStyles.miniButton))
                        {
                            SpawnTargetInScene(enemyTagProp.stringValue);
                        }
                        break;

                    case ObjectiveType.Timer:
                        var durProp = objProp.FindPropertyRelative("timerDuration");
                        EditorGUILayout.PropertyField(durProp, new GUIContent("Duration (Seconds)"));
                        break;

                    case ObjectiveType.Custom:
                        EditorGUILayout.PropertyField(idProp, new GUIContent("Custom Objective ID"));
                        EditorGUILayout.HelpBox("Complete this objective from scripts or UnityEvents by calling NanoMissionManager.Instance.CompleteObjective(...)", MessageType.Info);
                        break;
                }

                // Optional toggle
                var optProp = objProp.FindPropertyRelative("isOptional");
                EditorGUILayout.PropertyField(optProp);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
        }

        private void DrawStatusMessage()
        {
            if (!string.IsNullOrEmpty(_statusMessage) && EditorApplication.timeSinceStartup - _statusMessageTime < 4.0)
            {
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }
        }

        private void ShowStatus(string message)
        {
            _statusMessage = message;
            _statusMessageTime = EditorApplication.timeSinceStartup;
            Repaint();
        }

        #region Asset Creation & Scene Helpers

        private void CreateNewMissionAsset(string title)
        {
            string folder = GetMissionsSaveDirectory();
            string safeTitle = string.IsNullOrEmpty(title) ? "NewMission" : title.Replace(" ", "_");
            string assetPath = Path.Combine(folder, $"{safeTitle}.asset").Replace("\\", "/");
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            var newMission = ScriptableObject.CreateInstance<NanoMissionData>();
            newMission.missionTitle = title;
            newMission.missionId = safeTitle.ToLowerInvariant();

            // Default starter objective
            newMission.objectives.Add(new NanoObjectiveData
            {
                id = "obj_01",
                title = "Reach Destination",
                type = ObjectiveType.ReachLocation,
                radius = 3.5f
            });

            AssetDatabase.CreateAsset(newMission, assetPath);
            AssetDatabase.SaveAssets();

            RefreshMissionList();
            _selectedMission = newMission;
            UpdateSerializedObject();
            ShowStatus($"Created new mission: {assetPath}");
        }

        private string GetMissionsSaveDirectory()
        {
            // Preferred directory: Assets/Plugins/Nanopaw-Toolkit/Runtime/NanodogsToolkit/NanoMissions/Missions
            string preferred = "Assets/Plugins/Nanopaw-Toolkit/Runtime/NanodogsToolkit/NanoMissions/Missions";
            if (!AssetDatabase.IsValidFolder(preferred))
            {
                string parent = "Assets/Plugins/Nanopaw-Toolkit/Runtime/NanodogsToolkit/NanoMissions";
                if (AssetDatabase.IsValidFolder(parent))
                {
                    AssetDatabase.CreateFolder(parent, "Missions");
                    return preferred;
                }

                // Fallback to Assets/Missions
                if (!AssetDatabase.IsValidFolder("Assets/Missions"))
                {
                    AssetDatabase.CreateFolder("Assets", "Missions");
                }
                return "Assets/Missions";
            }
            return preferred;
        }

        private void SetupMissionManagerInScene()
        {
            var manager = FindFirstObjectByType<NanoMissionManager>();
            if (manager == null)
            {
                var go = new GameObject("NanoMissionManager");
                manager = go.AddComponent<NanoMissionManager>();
                Undo.RegisterCreatedObjectUndo(go, "Created NanoMissionManager");
            }

            if (_selectedMission != null && !manager.missionsToLoad.Contains(_selectedMission))
            {
                manager.missionsToLoad.Add(_selectedMission);
                EditorUtility.SetDirty(manager);
            }

            Selection.activeGameObject = manager.gameObject;
            ShowStatus("NanoMissionManager configured in scene.");
        }

        private void SetupHUDInScene()
        {
            var hud = FindFirstObjectByType<NanoMissionHUD>();
            if (hud == null)
            {
                var hudGo = NanoMissionHUD.CreateDefaultHUDInScene();
                Undo.RegisterCreatedObjectUndo(hudGo, "Created NanoMissionHUD");
                Selection.activeGameObject = hudGo;
                ShowStatus("NanoMissionHUD created in scene!");
            }
            else
            {
                Selection.activeGameObject = hud.gameObject;
                ShowStatus("NanoMissionHUD already exists in scene.");
            }
        }

        private void AddMissionToSceneManager(NanoMissionData mission)
        {
            var manager = FindFirstObjectByType<NanoMissionManager>();
            if (manager == null)
            {
                SetupMissionManagerInScene();
                manager = FindFirstObjectByType<NanoMissionManager>();
            }

            if (manager != null && !manager.missionsToLoad.Contains(mission))
            {
                Undo.RecordObject(manager, "Add Mission to Manager");
                manager.missionsToLoad.Add(mission);
                EditorUtility.SetDirty(manager);
                ShowStatus($"Added '{mission.missionTitle}' to Scene Manager.");
            }
        }

        private void SpawnTriggerZoneInScene(string zoneId)
        {
            var go = new GameObject($"TriggerZone_{zoneId}", typeof(BoxCollider), typeof(NanoMissionTriggerZone));
            var box = go.GetComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(4, 3, 4);

            var tz = go.GetComponent<NanoMissionTriggerZone>();
            tz.zoneId = zoneId;

            PositionInFrontOfCamera(go);
            Undo.RegisterCreatedObjectUndo(go, "Spawned TriggerZone");
            Selection.activeGameObject = go;
            ShowStatus($"Spawned TriggerZone '{zoneId}' in scene.");
        }

        private void SpawnCollectableInScene(string itemTag)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"Collectable_{itemTag}";
            go.transform.localScale = Vector3.one * 0.6f;

            var col = go.GetComponent<SphereCollider>();
            if (col != null) col.isTrigger = true;

            var comp = go.AddComponent<NanoMissionCollectable>();
            comp.itemTag = itemTag;

            PositionInFrontOfCamera(go);
            Undo.RegisterCreatedObjectUndo(go, "Spawned Collectable");
            Selection.activeGameObject = go;
            ShowStatus($"Spawned Collectable '{itemTag}' in scene.");
        }

        private void SpawnTargetInScene(string targetTag)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Target_{targetTag}";

            var comp = go.AddComponent<NanoMissionTarget>();
            comp.targetTag = targetTag;

            PositionInFrontOfCamera(go);
            Undo.RegisterCreatedObjectUndo(go, "Spawned Target");
            Selection.activeGameObject = go;
            ShowStatus($"Spawned Target '{targetTag}' in scene.");
        }

        private void PositionInFrontOfCamera(GameObject go)
        {
            if (SceneView.lastActiveSceneView != null)
            {
                var cam = SceneView.lastActiveSceneView.camera;
                go.transform.position = cam.transform.position + cam.transform.forward * 4f;
            }
            else
            {
                go.transform.position = Vector3.zero;
            }
        }

        #endregion
    }
}
