using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Nanodogs.Toolkit.MapBuilding
{
    public class InvisWaller : EditorWindow
    {
        [MenuItem("Nanodogs/Tools/Utilites/Map Building/Invis Waller")]
        public static void ShowWindow()
        {
            GetWindow<InvisWaller>("Invis Waller");
        }

        private List<Vector3> points = new List<Vector3>();
        private bool isEditing = false;
        private float wallHeight = 3f;
        private float wallThickness = 0.2f;
        private Material wallMaterial;
        private bool useBoxColliders = true;
        private bool loopPath = false;

        private void OnGUI()
        {
            GUILayout.Label("Invisible Wall Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            wallHeight = EditorGUILayout.FloatField("Wall Height", wallHeight);
            wallThickness = EditorGUILayout.FloatField("Wall Thickness", wallThickness);
            wallMaterial = (Material)EditorGUILayout.ObjectField("Wall Material", wallMaterial, typeof(Material), false);
            useBoxColliders = EditorGUILayout.Toggle("Use BoxColliders", useBoxColliders);
            loopPath = EditorGUILayout.Toggle("Loop Path", loopPath);

            EditorGUILayout.Space();
            if (GUILayout.Button(isEditing ? "Stop Editing Path" : "Start Editing Path"))
            {
                isEditing = !isEditing;
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Clear Points"))
            {
                Undo.RecordObject(this, "Clear InvisWall Points");
                points.Clear();
            }

            if (GUILayout.Button("Generate Walls"))
                GenerateWalls();

            EditorGUILayout.Space();
            GUILayout.Label($"Current Points: {points.Count}");
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!isEditing) return;

            Event e = Event.current;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Handles.color = Color.cyan;

            // Draw path
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 newPos = Handles.PositionHandle(points[i], Quaternion.identity);
                if (newPos != points[i])
                {
                    Undo.RecordObject(this, "Move InvisWall Point");
                    points[i] = newPos;
                }

                Handles.SphereHandleCap(0, points[i], Quaternion.identity, 0.2f, EventType.Repaint);

                if (i > 0)
                    Handles.DrawLine(points[i - 1], points[i]);
            }

            // Draw loop connection if enabled
            if (loopPath && points.Count > 2)
                Handles.DrawLine(points[points.Count - 1], points[0]);

            // Add points with Shift + Click
            if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                Vector3 newPoint = Vector3.zero;
                bool pointFound = false;

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    newPoint = hit.point;
                    pointFound = true;
                }
                else
                {
                    Plane plane = new Plane(Vector3.up, Vector3.zero);
                    if (plane.Raycast(ray, out float enter))
                    {
                        newPoint = ray.GetPoint(enter);
                        pointFound = true;
                    }
                }

                if (pointFound)
                {
                    Undo.RecordObject(this, "Add InvisWall Point");
                    points.Add(newPoint);
                    e.Use();
                }
            }

            SceneView.RepaintAll();
        }

        private void GenerateWalls()
        {
            if (points.Count < 2)
            {
                EditorUtility.DisplayDialog("Invis Waller", "You need at least 2 points to generate walls.", "OK");
                return;
            }

            GameObject parent = new GameObject("InvisibleWalls");
            Undo.RegisterCreatedObjectUndo(parent, "Create Invisible Walls");

            int segmentCount = loopPath ? points.Count : points.Count - 1;

            for (int i = 0; i < segmentCount; i++)
            {
                Vector3 start = points[i];
                Vector3 end = points[(i + 1) % points.Count];
                Vector3 mid = (start + end) / 2f;
                float distance = Vector3.Distance(start, end);

                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Undo.RegisterCreatedObjectUndo(wall, "Create InvisWall Segment");

                wall.name = $"Wall_{i}";
                wall.transform.position = mid + Vector3.up * (wallHeight / 2f);
                wall.transform.rotation = Quaternion.LookRotation(end - start);
                wall.transform.localScale = new Vector3(wallThickness, wallHeight, distance);
                wall.transform.SetParent(parent.transform);

                if (wallMaterial != null)
                {
                    wall.GetComponent<MeshRenderer>().sharedMaterial = wallMaterial;
                }
                else
                {
                    wall.GetComponent<MeshRenderer>().enabled = false; // invisible if no material
                }

                if (useBoxColliders)
                {
                    // keep default
                }
                else
                {
                    DestroyImmediate(wall.GetComponent<BoxCollider>());
                    wall.AddComponent<MeshCollider>().convex = true;
                }
            }

            EditorUtility.DisplayDialog("Invis Waller", "Invisible walls generated successfully.", "OK");
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }
}
