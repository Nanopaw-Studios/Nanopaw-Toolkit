// © 2025 Nanodogs Studios. All rights reserved.

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Net.Security;
using UnityEditor.SearchService;
using UnityEditor.SceneManagement;

namespace Nanodogs.Toolkit
{
    public class NanodogsToolkitOthersEditor : MonoBehaviour
    {
        #region Other

        [MenuItem("Nanodogs/Help")]
        static void Help()
        {
            Application.OpenURL("https://discord.gg/hrm4nTyAmA");
        }

        [MenuItem("Nanodogs/File/Save Open Scenes")]
        static void SaveScene()
        {
            EditorSceneManager.SaveOpenScenes();
        }

        [MenuItem("Nanodogs/File/Create Scene/Default")]
        static void CreateSceneDefault()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        }
        [MenuItem("Nanodogs/File/Create Scene/Empty")]
        static void CreateSceneEmpty()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        }

        #endregion
    }
}
