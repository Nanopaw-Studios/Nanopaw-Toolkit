using Nanodogs.UniversalScripts;
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nanodogs.Toolkit
{
    [CustomEditor(typeof(LevelLoadTrigger))]
    public class LevelLoadTriggerEditor : Editor
    {
        private BoxBoundsHandle boundsHandle = new BoxBoundsHandle();

        // tweak these to taste
        private static Color visibleEdgeColor = new Color(0f, 1f, 1f, 0.95f);
        private static Color occludedEdgeColor = new Color(0f, 1f, 1f, 0.20f);
        private static Color visibleFaceColor = new Color(0f, 1f, 1f, 0.06f);
        private static Color occludedFaceColor = new Color(0f, 1f, 1f, 0.03f);

        // UI toggles (simple, not serialized)
        private bool drawFaceFill = true;
        private bool drawGridOnFaces = true;
        private bool showDimensionLabels = false;

        private void OnSceneGUI()
        {
            LevelLoadTrigger trigger = (LevelLoadTrigger)target;
            if (trigger == null) return;

            Transform t = trigger.transform;
            BoxCollider box = trigger.GetComponent<BoxCollider>();

            // --- Axis sliders (makes moving on X/Y/Z easy) ---
            Handles.color = Color.white;
            float sliderSize = HandleUtility.GetHandleSize(t.position) * 0.8f;

            EditorGUI.BeginChangeCheck();
            Vector3 newPos = t.position;
            //newPos = Handles.Slider(newPos, t.right, sliderSize, Handles.ArrowHandleCap, 0f);
            //newPos = Handles.Slider(newPos, t.up, sliderSize, Handles.ArrowHandleCap, 0f);
            //newPos = Handles.Slider(newPos, t.forward, sliderSize, Handles.ArrowHandleCap, 0f);

            // Also allow free Move (optional) — keep it, but sliders are primary for precise axis movement
            //Vector3 freePos = Handles.PositionHandle(t.position, t.rotation);
            // If user used the free handle, prefer that new position
            //if (freePos != t.position) newPos = freePos;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Move Level Load Trigger");
                t.position = newPos;
                SceneView.RepaintAll();
            }

            // --- Box bounds handle for resizing (local space) ---
            if (box != null)
            {
                Matrix4x4 prevMatrix = Handles.matrix;
                Handles.matrix = t.localToWorldMatrix; // draw the box handles in local space

                boundsHandle.center = box.center;
                boundsHandle.size = box.size;

                EditorGUI.BeginChangeCheck();
                boundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(box, "Resize Trigger Collider");
                    box.center = boundsHandle.center;
                    box.size = boundsHandle.size;
                    EditorUtility.SetDirty(box);
                }

                Handles.matrix = prevMatrix;
            }

            // --- Depth cues and improved wireframe rendering ---
            if (box != null)
            {
                // Build the 8 local-space corners
                Vector3 half = box.size * 0.5f;
                Vector3 c = box.center;

                Vector3[] localVerts = new Vector3[8];
                localVerts[0] = c + new Vector3(-half.x, -half.y, -half.z);
                localVerts[1] = c + new Vector3(+half.x, -half.y, -half.z);
                localVerts[2] = c + new Vector3(+half.x, +half.y, -half.z);
                localVerts[3] = c + new Vector3(-half.x, +half.y, -half.z);
                localVerts[4] = c + new Vector3(-half.x, -half.y, +half.z);
                localVerts[5] = c + new Vector3(+half.x, -half.y, +half.z);
                localVerts[6] = c + new Vector3(+half.x, +half.y, +half.z);
                localVerts[7] = c + new Vector3(-half.x, +half.y, +half.z);

                // transform to world
                Vector3[] worldVerts = new Vector3[8];
                for (int i = 0; i < 8; i++) worldVerts[i] = t.TransformPoint(localVerts[i]);

                // faces (each face uses 4 indices; winding chosen so normals point outward)
                int[][] faces = new int[][] {
                    new int[]{4,5,6,7}, // +Z front
                    new int[]{1,0,3,2}, // -Z back
                    new int[]{0,4,7,3}, // -X left
                    new int[]{5,1,2,6}, // +X right
                    new int[]{3,7,6,2}, // +Y top
                    new int[]{0,1,5,4}  // -Y bottom
                };

                Camera sceneCam = SceneView.currentDrawingSceneView?.camera ?? Camera.current ?? Camera.main;
                if (sceneCam == null) sceneCam = Camera.current;

                // Draw face fills (depth-tested normally) so they are occluded by world geometry — helps volume perception
                if (drawFaceFill)
                {
                    Handles.zTest = CompareFunction.LessEqual;
                    for (int f = 0; f < faces.Length; f++)
                    {
                        Vector3 a = worldVerts[faces[f][0]];
                        Vector3 b = worldVerts[faces[f][1]];
                        Vector3 d = worldVerts[faces[f][3]];
                        Vector3 faceCenter = (a + b + worldVerts[faces[f][2]] + d) * 0.25f;

                        // compute face normal
                        Vector3 normal = Vector3.Cross(b - a, d - a).normalized;

                        bool facingCamera = Vector3.Dot((sceneCam.transform.position - faceCenter).normalized, normal) > 0f;
                        Handles.color = facingCamera ? visibleFaceColor : occludedFaceColor;

                        // draw filled quad (AA convex polygon)
                        Handles.DrawAAConvexPolygon(new Vector3[] { a, b, worldVerts[faces[f][2]], d });

                        // optional grid/diagonals to show planar orientation
                        if (drawGridOnFaces)
                        {
                            Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, Handles.color.a * 1.4f);
                            Handles.DrawLine(a, worldVerts[faces[f][2]]);
                            Handles.DrawLine(b, d);
                        }
                    }
                }

                // Draw occluded (behind) edges faintly, then visible edges strongly — this gives hidden-line cues
                Handles.zTest = CompareFunction.Greater;
                Handles.color = occludedEdgeColor;
                DrawAllBoxEdges(worldVerts, 6f); // thicker faint lines for behind parts

                Handles.zTest = CompareFunction.LessEqual;
                Handles.color = visibleEdgeColor;
                DrawAllBoxEdges(worldVerts, 2.5f); // visible edges

                // Draw small corner caps so the box reads as a volume — also use two passes for occlusion correctness
                float capSize = HandleUtility.GetHandleSize(t.position) * 0.03f;
                Handles.zTest = CompareFunction.Greater;
                Handles.color = new Color(occludedEdgeColor.r, occludedEdgeColor.g, occludedEdgeColor.b, occludedEdgeColor.a * 0.8f);
                foreach (var v in worldVerts) Handles.SphereHandleCap(0, v, Quaternion.identity, capSize, EventType.Repaint);

                Handles.zTest = CompareFunction.LessEqual;
                Handles.color = visibleEdgeColor;
                foreach (var v in worldVerts) Handles.SphereHandleCap(0, v, Quaternion.identity, capSize * 0.6f, EventType.Repaint);

                // Dimension labels
                if (showDimensionLabels)
                {
                    // X width (right face center)
                    Vector3 rightCenter = (worldVerts[5] + worldVerts[1] + worldVerts[2] + worldVerts[6]) * 0.25f;
                    Vector3 upCenter = (worldVerts[3] + worldVerts[7] + worldVerts[6] + worldVerts[2]) * 0.25f;
                    Vector3 frontCenter = (worldVerts[4] + worldVerts[5] + worldVerts[6] + worldVerts[7]) * 0.25f;

                    GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
                    Handles.Label(rightCenter, $"W: {box.size.x:0.###}", labelStyle);
                    Handles.Label(upCenter, $"H: {box.size.y:0.###}", labelStyle);
                    Handles.Label(frontCenter, $"D: {box.size.z:0.###}", labelStyle);
                }

                // restore default zTest
                Handles.zTest = CompareFunction.LessEqual;
            }
        }

        private void DrawAllBoxEdges(Vector3[] verts, float width)
        {
            // edges: back square, front square, connecting edges
            // back: 0-1,1-2,2-3,3-0
            Handles.DrawAAPolyLine(width, new Vector3[] { verts[0], verts[1] });
            Handles.DrawAAPolyLine(width, new Vector3[] { verts[1], verts[2] });
            Handles.DrawAAPolyLine(width, new Vector3[] { verts[2], verts[3] });
            Handles.DrawAAPolyLine(width, new Vector3[] { verts[3], verts[0] });

            // front: 4-5,5-6,6-7,7-4
            Handles.DrawAAPolyLine(width, new Vector3[] { verts[4], verts[5] });
            Handles.DrawAAPolyLine(width, new Vector3[] { verts[5], verts[6] });
            Handles.DrawAAPolyLine(width, new Vector3[] { verts[6], verts[7] });
            Handles.DrawAAPolyLine(width, new Vector3[] { verts[7], verts[4] });

            // connections
            Handles.DrawAAPolyLine(width, new Vector3[] { verts[0], verts[4] });
            Handles.DrawAAPolyLine(width, new Vector3[] { verts[1], verts[5] });
            Handles.DrawAAPolyLine(width, new Vector3[] { verts[2], verts[6] });
            Handles.DrawAAPolyLine(width, new Vector3[] { verts[3], verts[7] });
        }

       
    }
}
