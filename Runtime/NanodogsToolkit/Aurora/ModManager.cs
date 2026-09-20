using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nanodogs.Aurora
{

    public class ModManager : MonoBehaviour
    {
        public string assetBoxDirectory = "Nanodogs/Aurora/Packed";
        public string assetBoxName = "packed.box";

        void Start()
        {
            LoadAssetBox(assetBoxDirectory, assetBoxName);
        }

        public void LoadAssetBox(string directory, string boxName)
        {
            var myLoadedAssetBox = AssetBundle.LoadFromFile(Path.Combine(directory, boxName));
            if (myLoadedAssetBox == null)
            {
                Debug.Log("Failed to load AssetBox!");
                return;
            }
            var scenePath = myLoadedAssetBox.GetAllScenePaths()[0];
            SceneManager.LoadScene(scenePath);
        }
    }
}
