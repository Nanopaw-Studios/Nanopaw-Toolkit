using UnityEditor;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SkyboxSceneReference
{
    public Scene sceneAsset;
    public string sceneName;

    public void UpdateSceneName()
    {
        if (sceneAsset != null)
        {
            sceneName = sceneAsset.name;
        }
    }
}