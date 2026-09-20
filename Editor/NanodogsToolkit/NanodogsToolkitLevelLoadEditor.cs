// © 2025 Nanodogs Studios. All rights reserved.

using Nanodogs.UniversalScripts;
using System;
using UnityEditor;
using UnityEngine;

namespace Nanodogs.Toolkit
{
    public class NanodogsToolkitLevelLoadEditor : NDSEditorWindow
    {
        new Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        Vector3 scale = Vector3.one;

        string levelName = "Replace me with your level name";

        [MenuItem("Nanodogs/Tools/Utilites/Level Load Trigger Creation", false, -50)]
        public static void ShowWindow()
        {
            GetWindow<NanodogsToolkitLevelLoadEditor>("Level Load Trigger Creation");
        }

        new public void OnGUI()
        {
            EditorGUILayout.Vector3Field("Position", position);
            EditorGUILayout.Space();

            EditorGUILayout.Vector3Field("Rotation", rotation.eulerAngles);
            EditorGUILayout.Space();

            EditorGUILayout.Vector3Field("Scale", scale);
            EditorGUILayout.Space();

            levelName = EditorGUILayout.TextField("Level Name", levelName);
            EditorGUILayout.Space();

            if (GUILayout.Button("Create Level Load Trigger"))
            {
                CreateLevelLoadTrigger();
            }

            base.OnGUI();
        }

        private void CreateLevelLoadTrigger()
        {
            GameObject trigger = new GameObject("Level Load Trigger");
            trigger.transform.position = position;
            trigger.transform.rotation = rotation;
            trigger.transform.localScale = scale;
            BoxCollider boxCollider = trigger.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            LevelLoadTrigger levelLoadTrigger = trigger.AddComponent<LevelLoadTrigger>();
            levelLoadTrigger.levelName = levelName;
            Undo.RegisterCreatedObjectUndo(trigger, "Create Level Load Trigger");
            Selection.activeObject = trigger;
        }
    }
}
