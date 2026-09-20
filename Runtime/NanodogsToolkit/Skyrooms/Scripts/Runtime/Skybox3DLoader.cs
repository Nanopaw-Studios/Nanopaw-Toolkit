using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Nanodogs.Skyrooms
{
    public class Skybox3DLoader : MonoBehaviour
    {
        public Skybox3DSettings settings;

        private static Camera skyboxCamera;  // keep it

        void Start()
        {
            if (settings != null)
            {
                LoadSkybox(settings);
            }
        }

        void LateUpdate()
        {
            // after main camera moved
            if (skyboxCamera != null && Camera.main != null)
            {
                // Position skybox camera at main camera but scaled/offset if you want
                skyboxCamera.transform.position = Camera.main.transform.position * settings.scale + settings.offset;
                skyboxCamera.transform.rotation = Camera.main.transform.rotation;
            }
            else
            {
                skyboxCamera = Camera.main;
            }
        }

        public static void LoadSkybox(Skybox3DSettings s)
        {
            if (string.IsNullOrEmpty(s.skyboxSceneName))
            {
                Debug.LogError("No scene name.");
                return;
            }

            if (skyboxCamera == null)
            {
                var go = new GameObject("Skybox3DCamera");
                skyboxCamera = go.AddComponent<Camera>();
                skyboxCamera.cullingMask = 1 << LayerMask.NameToLayer("Skybox3D");
                skyboxCamera.clearFlags = CameraClearFlags.Skybox;
                skyboxCamera.depth = -1; // draw first

                // make it the Base camera
                var skyboxCamData = go.AddComponent<UniversalAdditionalCameraData>();
                skyboxCamData.renderPostProcessing = false;
                skyboxCamData.renderShadows = false;
                skyboxCamData.renderType = CameraRenderType.Base;

                // configure main camera as overlay and add to stack
                var mainCamData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
                mainCamData.renderType = CameraRenderType.Overlay;
                skyboxCamData.cameraStack.Add(Camera.main);

                // main camera shouldn’t render skybox layer
                Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("Skybox3D"));

                // ensure we dont have weird fov issues
                skyboxCamera.fieldOfView = Camera.main.fieldOfView;
            }

            // load the skybox scene additively
            var async = SceneManager.LoadSceneAsync(s.skyboxSceneName, LoadSceneMode.Additive);
            async.completed += _ =>
            {
                Scene loadedScene = SceneManager.GetSceneByName(s.skyboxSceneName);
                if (loadedScene.isLoaded)
                {
                    foreach (GameObject go in loadedScene.GetRootGameObjects())
                    {
                        go.transform.localScale *= s.scale;
                        go.transform.position += s.offset;
                        go.layer = LayerMask.NameToLayer("Skybox3D");
                    }
                }
            };
        }
    }
}
