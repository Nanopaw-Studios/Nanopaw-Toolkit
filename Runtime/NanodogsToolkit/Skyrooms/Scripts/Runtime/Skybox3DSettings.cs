using UnityEditor;
using UnityEngine;

namespace Nanodogs.Skyrooms
{
    [CreateAssetMenu(fileName = "Skybox3DSettings", menuName = "Nanodogs/Skyrooms/Skybox3DSettings")]
    public class Skybox3DSettings : ScriptableObject
    {
        public string skyboxSceneName;
        public float scale = 1f;
        public Vector3 offset = Vector3.zero;
    }
}