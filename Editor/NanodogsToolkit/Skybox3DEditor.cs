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

            settings.skyboxSceneName = EditorGUILayout.TextField("Scene Name", settings.skyboxSceneName);

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
                        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByName(settings.skyboxSceneName);
                        var scenePath = scene.path;
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

