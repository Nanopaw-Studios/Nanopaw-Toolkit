using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nanodogs.Skyrooms
{
    public class Skybox3DEditor : EditorWindow
    {
        private Skybox3DSettings settings;
        private Scene loadedScene;

        [MenuItem("Nanodogs/Tools/Utilites/Map Building/Skyrooms Editor")]
        public static void ShowWindow()
        {
            GetWindow<Skybox3DEditor>("Skyrooms Editor");
        }

        private void OnGUI()
        {
            settings = (Skybox3DSettings)EditorGUILayout.ObjectField(
                "Settings Asset",
                settings,
                typeof(Skybox3DSettings),
                false);

            if (settings == null)
            {
                EditorGUILayout.HelpBox("Assign a Skybox3DSettings asset to edit.", MessageType.Info);
                return;
            }

            EditorGUI.BeginChangeCheck();

#if UNITY_EDITOR
            settings.skyboxScene.sceneAsset =
                (SceneAsset)EditorGUILayout.ObjectField("Skybox Scene",
                    settings.skyboxScene.sceneAsset,
                    typeof(SceneAsset),
                    false);

            if (settings.skyboxScene.sceneAsset != null)
                settings.skyboxScene.UpdateSceneName();
#endif

            settings.scale = EditorGUILayout.FloatField("Scale", settings.scale);
            settings.offset = EditorGUILayout.Vector3Field("Offset", settings.offset);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(settings);
            }

            if (GUILayout.Button("Test Load Additively / Close if open"))
            {
                if (Application.isPlaying)
                {
                    // Use runtime loader
                    Skybox3DLoader.LoadSkybox(settings);
                }
                else
                {
                    if (loadedScene.isLoaded)
                    {
                        Debug.Log("Closing previously loaded skybox scene.");
                        EditorSceneManager.CloseScene(loadedScene, true);
                        return;
                    }
                    else
                    {
                        // Editor mode preview
                        var scenePath = AssetDatabase.GetAssetPath(settings.skyboxScene.sceneAsset);
                        loadedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                        foreach (GameObject go in loadedScene.GetRootGameObjects())
                        {
                            go.transform.localScale *= settings.scale;
                            go.transform.position += settings.offset;
                        }

                        Debug.Log("Skybox scene loaded additively in Editor.");
                    }
                }
            }

        }
    }
}

