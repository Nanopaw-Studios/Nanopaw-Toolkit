using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Nanodogs.API.NanoMusic
{
    /// <summary>
    /// Manages music Toasts in the NanoMusic system.
    /// </summary>
    public class ToastManager : MonoBehaviourRequireNanoMusic
    {
        public GameObject toastPrefab;

        private GameObject currentToast;

        private void Awake()
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                Debug.LogError("ToastManager: No Canvas found!");
                return;
            }

            currentToast = Instantiate(toastPrefab, canvas.transform);
        }

        public void Change(NanoMusicAsset music)
        {
            ShowToast(music);
            StartCoroutine(ChangeIe(music));
        }
        public IEnumerator ChangeIe(NanoMusicAsset music)
        {
            yield return new WaitForSecondsRealtime(3);
            HideToast(music);
        }

        public void HideToast(NanoMusicAsset music)
        {
            Debug.Log("Hiding");
            currentToast.gameObject.SetActive(true);
            currentToast.GetComponent<Animator>().Play("MusicToastSlideOut");

            UpdateDetails(music);
        }

        public void ShowToast(NanoMusicAsset music)
        {
            Debug.Log("Showing");
            currentToast.gameObject.SetActive(true);
            currentToast.GetComponent<Animator>().Play("MusicToastSlideIn");

            UpdateDetails(music);
        }

        public void UpdateDetails(NanoMusicAsset music)
        {
            Debug.Log("Updating");
            TMP_Text title = currentToast.transform.Find("Title").GetComponent<TMP_Text>();
            TMP_Text artist = currentToast.transform.Find("Artist").GetComponent<TMP_Text>();

            title.text = music.songData.songName;
            artist.text = music.songData.artistName;
        }
    }
}
