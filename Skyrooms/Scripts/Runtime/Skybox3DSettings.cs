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

    [System.Serializable]
    public class SkyboxSceneReference
    {
        public UnityEditor.SceneAsset sceneAsset;
        public string sceneName;

        public void UpdateSceneName()
        {
            if (sceneAsset != null)
            {
                sceneName = sceneAsset.name;
            }
        }
    }
}