using UnityEngine;

namespace Nanodogs.Skyrooms
{
    [CreateAssetMenu(fileName = "New3DSkyboxSettings", menuName = "Skyrooms/3D Skybox Settings")]
    public class Skybox3DSettings : ScriptableObject
    {
        public SceneReference skyboxScene;   // Custom type, see below
        public float scale = 0.0625f;
        public Vector3 offset = Vector3.zero;
    }
}