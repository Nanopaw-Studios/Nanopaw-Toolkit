using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nanodogs.Skyrooms
{

    [System.Serializable]
    public class SceneReference
    {
#if UNITY_EDITOR
        public SceneAsset sceneAsset; // Only visible in Editor
#endif
        public string sceneName;      // Used at runtime

#if UNITY_EDITOR
        // When changed in editor, update the string
        public void UpdateSceneName()
        {
            if (sceneAsset != null)
                sceneName = sceneAsset.name;
        }
#endif
    }
}
