using Nanodogs.API.Nanosaves;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Nanodogs.Toolkit.NanoMenu
{
    public class MainMenu : MonoBehaviour
    {
        public Color ThemeColor = Color.red;

        [Header("Menu Animators")]
        public Animator PlayOptions;
        public Animator MainOptions;
        public Animator QuitOptions;
        public Animator SettingsOptions;
        public Animator CreditsOptions;

        [Header("Settings UI")]
        public Slider volumeSlider;
        public TMP_Dropdown qualityDropdown;
        public Toggle vsyncToggle;
        public Toggle fullscreenToggle;

        [Header("Other UI")]
        public Button continueButton;

        private const float transitionDelay = 1f;

        // Store transition state instead of using local function invoke (fixes Invoke error)
        private Animator transitionFrom;
        private Animator transitionTo;
        private int transitionChildIndex;

        private int levelIndex;

        private void Start()
        {
            LoadSettings();

            // do sum color stuff
            // loop through all buttons and set their colors to ThemeColor
            //ApplyTheme(
            //    ThemeColor,
            //    MainOptions.gameObject,
            //    PlayOptions.gameObject,
            //    SettingsOptions.gameObject,
            //    CreditsOptions.gameObject,
            //    QuitOptions.gameObject
            //);
        }

        void ApplyThemeTo(GameObject root, Color themeColor)
        {
            foreach (Image img in root.GetComponentsInChildren<Image>(true))
            {
                Color c = img.color;
                c.r = themeColor.r;
                c.g = themeColor.g;
                c.b = themeColor.b;
                img.color = c;
            }
        }
        void ApplyTheme(Color themeColor, params GameObject[] groups)
        {
            foreach (var g in groups)
            {
                foreach (var img in g.GetComponentsInChildren<Image>(true))
                {
                    Color c = img.color;
                    c.r = themeColor.r;
                    c.g = themeColor.g;
                    c.b = themeColor.b;
                    img.color = c;
                }
            }
        }

        #region Button Click Handlers

        public void PlayButtonClicked() => TransitionTo(MainOptions, PlayOptions, 1);

        public void BackButtonClicked() => TransitionTo(PlayOptions, MainOptions, 0);

        public void QuitButtonClicked() => TransitionTo(MainOptions, QuitOptions, 1);

        public void QuitBackClicked() => TransitionTo(QuitOptions, MainOptions, 0);

        public void SettingsButtonClicked() => TransitionTo(MainOptions, SettingsOptions, 1);

        public void SettingsBackClicked() => TransitionTo(SettingsOptions, MainOptions, 0);

        public void CreditsButtonClicked() => TransitionTo(MainOptions, CreditsOptions, 10);

        public void CreditsBackClicked() => TransitionTo(CreditsOptions, MainOptions, 0);

        #endregion

        #region Transition Logic

        private void TransitionTo(Animator fromAnimator, Animator toAnimator, int selectChildIndex)
        {
            if (fromAnimator == null || toAnimator == null) return;

            transitionFrom = fromAnimator;
            transitionTo = toAnimator;
            transitionChildIndex = selectChildIndex;

            fromAnimator.Play("btnsSlideout");
            Invoke(nameof(DoTransition), transitionDelay);
        }

        private void DoTransition()
        {
            if (transitionFrom == null || transitionTo == null) return;

            transitionFrom.gameObject.SetActive(false);
            transitionTo.gameObject.SetActive(true);
            transitionTo.Play("btnsSlidein");

            if (EventSystem.current != null && transitionTo.transform.childCount > transitionChildIndex)
            {
                EventSystem.current.SetSelectedGameObject(transitionTo.transform.GetChild(transitionChildIndex).gameObject);
            }

            transitionFrom = null;
            transitionTo = null;
        }

        #endregion

        #region Settings

        public void QuitGame()
        {
            Application.Quit(0);
            Debug.Log("Quit with code: 0");
        }

        public void SetVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
            NanoSaves.SaveData("volume", volume.ToString());
        }

        public void SetQuality(int quality)
        {
            QualitySettings.SetQualityLevel(quality);
            NanoSaves.SaveData("quality", quality.ToString());
        }

        public void SetVsync(bool vsync)
        {
            QualitySettings.vSyncCount = vsync ? 1 : 0;
            NanoSaves.SaveData("vsync", vsync.ToString());
        }

        public void SetFullscreen(bool fullscreen)
        {
            Screen.fullScreen = fullscreen;
            NanoSaves.SaveData("fullscreen", fullscreen.ToString());
        }

        private void LoadSettings()
        {
            if (float.TryParse(NanoSaves.LoadData("volume"), out float volume))
            {
                AudioListener.volume = Mathf.Clamp01(volume);
                if (volumeSlider != null) volumeSlider.value = volume;
            }

            if (int.TryParse(NanoSaves.LoadData("quality"), out int quality))
            {
                QualitySettings.SetQualityLevel(quality);
                if (qualityDropdown != null) qualityDropdown.value = quality;
            }

            if (bool.TryParse(NanoSaves.LoadData("vsync"), out bool vsync))
            {
                QualitySettings.vSyncCount = vsync ? 1 : 0;
                if (vsyncToggle != null) vsyncToggle.isOn = vsync;
            }

            if (bool.TryParse(NanoSaves.LoadData("fullscreen"), out bool fullscreen))
            {
                Screen.fullScreen = fullscreen;
                if (fullscreenToggle != null) fullscreenToggle.isOn = fullscreen;
            }
        }

        #endregion

        #region Play Saving/Loading

        public void NewGameClicked()
        {
            // create a new save game
            NanoSaves.SaveData("CurrentSaveGame", "lvl:01|x:0|y:0|z:0|hp:100");
            Nanoloader.LoadLevel("01");
        }

        public void ContinueClicked()
        {
            string data = NanoSaves.LoadData("CurrentSaveGame");
            if (string.IsNullOrEmpty(data))
            {
                continueButton.interactable = false;
                return;
            }

            string[] datas = data.Split('|');
            Dictionary<string, string> parsedData = new Dictionary<string, string>();

            foreach (string s in datas)
            {
                if (string.IsNullOrEmpty(s)) continue;

                string[] parts = s.Split(':');
                if (parts.Length == 2)
                {
                    parsedData[parts[0]] = parts[1];
                }
            }

            string level = parsedData["lvl"];
            float x = float.Parse(parsedData["x"]);
            float y = float.Parse(parsedData["y"]);
            float z = float.Parse(parsedData["z"]);
            float hp = float.Parse(parsedData["hp"]);

            // Store for next scene
            PlayerLoadData.Position = new Vector3(x, y, z);
            PlayerLoadData.HP = hp;
            PlayerLoadData.HasPendingData = true;

            Debug.Log($"Continuing game at Level {level} - Pos({x}, {y}, {z}) HP:{hp}");

            Nanoloader.LoadLevel(level);
            levelIndex = SceneManager.GetSceneByName(level).buildIndex;
        }

        private void OnLevelWasLoaded(int level)
        {
            if (level == levelIndex)
            {
                Debug.Log("yup");
            }
        }

        #endregion
    }
}