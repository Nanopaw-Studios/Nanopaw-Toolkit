using UnityEditor;
using UnityEngine;

namespace Nanodogs.Skyrooms
{
    [CreateAssetMenu(fileName = "Skybox3DSettings", menuName = "Nanodogs/Skyrooms/Skybox3DSettings")]
    public class Skybox3DSettings : ScriptableObject
    {
        public SkyboxSceneReference skyboxScene;
        public float scale = 1f;
        public Vector3 offset = Vector3.zero;
    }

    #if UNITY_EDITOR
    [System.Serializable]
    public class SkyboxSceneReference
    {
        public SceneAsset sceneAsset;
        public string sceneName;

        public void UpdateSceneName()
        {
            if (sceneAsset != null)
            {
                sceneName = sceneAsset.name;
            }
        }
    }

    #endif
}