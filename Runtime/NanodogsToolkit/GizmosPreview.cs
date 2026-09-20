using UnityEngine;

namespace Nanodogs.UniversalScripts
{
    /// <summary>
    /// A simple script to visualize gizmos in the Unity Editor.
    /// </summary>
    public class GizmosPreview : MonoBehaviour
    {
        public enum GizmoShape { Sphere, Cube, WireSphere, WireCube }
        public GizmoShape shape = GizmoShape.Sphere;

        public Color gizmoColor = Color.yellow;
        public float size = 1f;
        public Vector3 offset = Vector3.zero;

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Vector3 position = transform.position + offset;
            switch (shape)
            {
                case GizmoShape.Sphere:
                    Gizmos.DrawSphere(position, size);
                    break;
                case GizmoShape.Cube:
                    Gizmos.DrawCube(position, Vector3.one * size);
                    break;
                case GizmoShape.WireSphere:
                    Gizmos.DrawWireSphere(position, size);
                    break;
                case GizmoShape.WireCube:
                    Gizmos.DrawWireCube(position, Vector3.one * size);
                    break;
            }
        }
    }
}