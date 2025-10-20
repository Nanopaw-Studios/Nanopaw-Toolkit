// © 2025 Nanodogs Studios. All rights reserved.

using Nanodogs.UniversalScripts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Nanoloader : MonoBehaviour
{
    public static void LoadLevel(string levelName)
    {
        GameObject player = NanoPlayerSpawnpoint.GetPlayer();

        Debug.Log("Loading level: " + levelName);

        SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
        GameObject loadingScreen = LoadingScreen();

        player.GetComponent<Rigidbody>().isKinematic = true;

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            Destroy(loadingScreen);

            player.GetComponent<Rigidbody>().isKinematic = false;
            GameObject[] pl = GameObject.FindGameObjectsWithTag("Player");
            if (pl.Length > 1)
            {
                Debug.Log("Multiple players found, removing duplicates.");
                if (pl[0].GetInstanceID() != player.GetInstanceID())
                {
                    Destroy(pl[0]);
                }
                else
                {
                    Destroy(pl[1]);
                }
            }
        };
    }

    static GameObject LoadingScreen()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject loadingScreenObj = new GameObject("Loading Screen");
        loadingScreenObj.transform.SetParent(canvasObj.transform);

        GameObject text = new GameObject("Loading Text");
        text.transform.SetParent(loadingScreenObj.transform);
        var textComponent = text.AddComponent<UnityEngine.UI.Text>();
        textComponent.text = "Loading...";
        textComponent.alignment = TextAnchor.LowerLeft;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = 32;
        textComponent.color = Color.white;
        textComponent.rectTransform.anchorMin = new Vector2(0, 0);
        textComponent.rectTransform.anchorMax = new Vector2(0, 0);
        textComponent.rectTransform.pivot = new Vector2(0, 0);
        textComponent.rectTransform.sizeDelta = new Vector2(300, 200);
        textComponent.rectTransform.anchoredPosition = new Vector2(10, 10);
        return canvasObj;
    }
}
