using Nanodogs.API.NanoHealth;
using Nanodogs.API.Nanosaves;
using Nanodogs.UniversalScripts;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nanodogs.Toolkit.NanoMenu
{

    public class PlayerLoadHandler : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            if (PlayerLoadData.HasPendingData)
            {
                var player = NanoPlayerSpawnpoint.GetPlayer();
                if (player != null)
                {
                    // Apply position
                    player.transform.position = PlayerLoadData.Position;

                    // Apply HP — assuming player has a health component -- ignore for now.
                    //var health = player.GetComponent<NanoHealth>();
                    //if (health != null)
                    //{
                    //    health.currentHealth = PlayerLoadData.HP;
                    //}

                    Debug.Log($"Loaded Player at {PlayerLoadData.Position} with HP {PlayerLoadData.HP}");
                }

                PlayerLoadData.HasPendingData = false; // clear once used
            }

            StartCoroutine(Autosave());
        }

        private IEnumerator Autosave()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(10);
                Quicksave();
            }
        }


        public static void Quicksave()
        {
            var player = NanoPlayerSpawnpoint.GetPlayer();
            if (player != null)
            {
                NanoSaves.SaveData("CurrentSaveGame",
                    "lvl:" + SceneManager.GetActiveScene().name +
                    "|x:" + player.transform.position.x +
                    "|y:0" + player.transform.position.y +
                    "|z:0" + player.transform.position.z +
                    "|hp:100" // for now
                );
            }
        }
    }
}
