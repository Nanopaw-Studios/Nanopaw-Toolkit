// © 2025-2026 Nanodogs Studios. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nanodogs.Toolkit.Missions.UI
{
    /// <summary>
    /// Clean, lightweight UGUI + TextMeshPro HUD for displaying active missions,
    /// objectives, progress counters, and completion toasts.
    /// </summary>
    public class NanoMissionHUD : MonoBehaviour
    {
        [Header("Tracker Panel")]
        [Tooltip("The main container panel for the mission tracker.")]
        public RectTransform trackerPanel;

        [Tooltip("Text displaying the mission category (e.g. 'MAIN QUEST').")]
        public TMP_Text categoryText;

        [Tooltip("Text displaying the mission title.")]
        public TMP_Text missionTitleText;

        [Tooltip("Text displaying the mission description.")]
        public TMP_Text descriptionText;

        [Tooltip("Transform container where objective items are instantiated.")]
        public RectTransform objectivesContainer;

        [Tooltip("Optional prefab for an objective item. If null, entries are generated dynamically.")]
        public GameObject objectiveItemPrefab;

        [Header("Toast Notification")]
        [Tooltip("CanvasGroup for the start/complete notification banner.")]
        public CanvasGroup toastCanvasGroup;

        [Tooltip("Text for the toast notification.")]
        public TMP_Text toastText;

        [Tooltip("Duration in seconds the toast remains visible.")]
        public float toastDuration = 3f;

        [Header("Aesthetic Settings")]
        public Color activeObjectiveColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        public Color completedObjectiveColor = new Color(0.4f, 0.9f, 0.4f, 0.85f);
        public Color inactiveObjectiveColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);

        private readonly List<TMP_Text> _objectiveTextPool = new List<TMP_Text>();
        private Coroutine _toastCoroutine;

        private bool _isHooked = false;

        private void OnEnable()
        {
            if (NanoMissionManager.Instance != null)
            {
                HookEvents();
            }
        }

        private void Start()
        {
            if (NanoMissionManager.Instance != null)
            {
                HookEvents();
                RefreshTrackedMission();
            }

            if (toastCanvasGroup != null)
            {
                toastCanvasGroup.alpha = 0f;
            }
        }

        private void Update()
        {
            if (!_isHooked && NanoMissionManager.Instance != null)
            {
                HookEvents();
                RefreshTrackedMission();
            }
        }

        private void OnDisable()
        {
            UnhookEvents();
        }

        private void HookEvents()
        {
            UnhookEvents();
            if (NanoMissionManager.Instance == null) return;

            NanoMissionManager.Instance.OnMissionStarted += HandleMissionStarted;
            NanoMissionManager.Instance.OnObjectiveUpdated += HandleObjectiveUpdated;
            NanoMissionManager.Instance.OnMissionCompleted += HandleMissionCompleted;
            NanoMissionManager.Instance.OnMissionFailed += HandleMissionFailed;
            NanoMissionManager.Instance.OnTrackedMissionChanged += HandleTrackedMissionChanged;
            _isHooked = true;
        }

        private void UnhookEvents()
        {
            if (NanoMissionManager.Instance != null && _isHooked)
            {
                NanoMissionManager.Instance.OnMissionStarted -= HandleMissionStarted;
                NanoMissionManager.Instance.OnObjectiveUpdated -= HandleObjectiveUpdated;
                NanoMissionManager.Instance.OnMissionCompleted -= HandleMissionCompleted;
                NanoMissionManager.Instance.OnMissionFailed -= HandleMissionFailed;
                NanoMissionManager.Instance.OnTrackedMissionChanged -= HandleTrackedMissionChanged;
            }
            _isHooked = false;
        }

        private void HandleMissionStarted(NanoMission mission)
        {
            ShowToast($"MISSION: {mission.Data.missionTitle.ToUpperInvariant()}");
            RefreshTrackedMission();
        }

        private void HandleObjectiveUpdated(NanoMission mission, NanoObjective objective)
        {
            if (NanoMissionManager.Instance != null && NanoMissionManager.Instance.TrackedMission == mission)
            {
                RefreshObjectives(mission);
            }
        }

        private void HandleMissionCompleted(NanoMission mission)
        {
            ShowToast($"COMPLETED: {mission.Data.missionTitle}!");
            RefreshTrackedMission();
        }

        private void HandleMissionFailed(NanoMission mission)
        {
            ShowToast($"FAILED: {mission.Data.missionTitle}");
            RefreshTrackedMission();
        }

        private void HandleTrackedMissionChanged(NanoMission mission)
        {
            RefreshTrackedMission();
        }

        public void RefreshTrackedMission()
        {
            var tracked = NanoMissionManager.Instance != null ? NanoMissionManager.Instance.TrackedMission : null;

            if (tracked == null || !tracked.IsInProgress)
            {
                if (trackerPanel != null)
                {
                    trackerPanel.gameObject.SetActive(false);
                }
                return;
            }

            if (trackerPanel != null)
            {
                trackerPanel.gameObject.SetActive(true);
            }

            if (categoryText != null)
            {
                categoryText.text = string.IsNullOrEmpty(tracked.Data.category)
                    ? "MISSION"
                    : tracked.Data.category.ToUpperInvariant();
            }

            if (missionTitleText != null)
            {
                missionTitleText.text = tracked.Data.missionTitle;
            }

            if (descriptionText != null)
            {
                bool hasDesc = !string.IsNullOrEmpty(tracked.Data.description);
                descriptionText.gameObject.SetActive(hasDesc);
                if (hasDesc) descriptionText.text = tracked.Data.description;
            }

            RefreshObjectives(tracked);
        }

        private void RefreshObjectives(NanoMission mission)
        {
            if (objectivesContainer == null) return;

            // Clear or hide pooled texts
            for (int i = 0; i < _objectiveTextPool.Count; i++)
            {
                _objectiveTextPool[i].gameObject.SetActive(false);
            }

            if (mission == null || mission.Objectives == null) return;

            for (int i = 0; i < mission.Objectives.Count; i++)
            {
                var obj = mission.Objectives[i];
                var textComp = GetOrCreateObjectiveText(i);

                string prefix = obj.Status switch
                {
                    ObjectiveStatus.Completed => "<color=#55FF77>[X]</color> ",
                    ObjectiveStatus.Active => "<color=#FFFFFF>[ ]</color> ",
                    ObjectiveStatus.Failed => "<color=#FF5555>[-]</color> ",
                    _ => "<color=#888888>[ ]</color> "
                };

                textComp.text = prefix + obj.GetFormattedText();
                textComp.color = obj.Status switch
                {
                    ObjectiveStatus.Completed => completedObjectiveColor,
                    ObjectiveStatus.Active => activeObjectiveColor,
                    _ => inactiveObjectiveColor
                };

                textComp.gameObject.SetActive(true);
            }
        }

        private TMP_Text GetOrCreateObjectiveText(int index)
        {
            while (index >= _objectiveTextPool.Count)
            {
                TMP_Text newText;
                if (objectiveItemPrefab != null)
                {
                    var go = Instantiate(objectiveItemPrefab, objectivesContainer);
                    newText = go.GetComponent<TMP_Text>() ?? go.GetComponentInChildren<TMP_Text>();
                }
                else
                {
                    var go = new GameObject($"Objective_{_objectiveTextPool.Count}", typeof(RectTransform));
                    go.transform.SetParent(objectivesContainer, false);
                    newText = go.AddComponent<TextMeshProUGUI>();
                    newText.fontSize = 14;
                    newText.textWrappingMode = TextWrappingModes.Normal;
                    newText.alignment = TextAlignmentOptions.TopLeft;

                    var rt = go.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(240, 24);
                }

                _objectiveTextPool.Add(newText);
            }

            return _objectiveTextPool[index];
        }

        public void ShowToast(string message)
        {
            if (toastCanvasGroup == null || toastText == null) return;

            if (_toastCoroutine != null)
            {
                StopCoroutine(_toastCoroutine);
            }

            _toastCoroutine = StartCoroutine(ToastRoutine(message));
        }

        private IEnumerator ToastRoutine(string message)
        {
            toastText.text = message;

            // Fade in
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.unscaledDeltaTime;
                toastCanvasGroup.alpha = Mathf.Clamp01(t / 0.25f);
                yield return null;
            }
            toastCanvasGroup.alpha = 1f;

            // Hold
            yield return new WaitForSecondsRealtime(toastDuration);

            // Fade out
            t = 0f;
            while (t < 0.35f)
            {
                t += Time.unscaledDeltaTime;
                toastCanvasGroup.alpha = 1f - Mathf.Clamp01(t / 0.35f);
                yield return null;
            }
            toastCanvasGroup.alpha = 0f;
            _toastCoroutine = null;
        }

        /// <summary>
        /// Procedurally generates a complete, stylish UGUI + TextMeshPro Mission HUD in the current scene.
        /// </summary>
        public static GameObject CreateDefaultHUDInScene()
        {
            // Find or create Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                // Ensure EventSystem
                if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    var esGo = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
#if ENABLE_INPUT_SYSTEM
                    esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                    esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
                }
            }

            // HUD Root Object
            var hudGo = new GameObject("NanoMissionHUD", typeof(RectTransform));
            hudGo.transform.SetParent(canvas.transform, false);
            var hudRt = hudGo.GetComponent<RectTransform>();
            hudRt.anchorMin = Vector2.zero;
            hudRt.anchorMax = Vector2.one;
            hudRt.offsetMin = Vector2.zero;
            hudRt.offsetMax = Vector2.zero;

            var hudComp = hudGo.AddComponent<NanoMissionHUD>();

            // --- TRACKER PANEL (Top-Right) ---
            var panelGo = new GameObject("TrackerPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            panelGo.transform.SetParent(hudGo.transform, false);

            var panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(1f, 1f);
            panelRt.anchorMax = new Vector2(1f, 1f);
            panelRt.pivot = new Vector2(1f, 1f);
            panelRt.anchoredPosition = new Vector2(-24f, -24f);
            panelRt.sizeDelta = new Vector2(320f, 200f);

            var panelImg = panelGo.GetComponent<Image>();
            panelImg.color = new Color(0.08f, 0.09f, 0.12f, 0.88f);

            var vlg = panelGo.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 14, 16);
            vlg.spacing = 6f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = panelGo.GetComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            hudComp.trackerPanel = panelRt;

            // Category Subtitle
            var catGo = new GameObject("CategoryText", typeof(RectTransform), typeof(TextMeshProUGUI));
            catGo.transform.SetParent(panelGo.transform, false);
            var catText = catGo.GetComponent<TextMeshProUGUI>();
            catText.text = "MAIN QUEST";
            catText.fontSize = 11;
            catText.fontStyle = FontStyles.Bold;
            catText.color = new Color(1f, 0.78f, 0.25f, 1f);
            hudComp.categoryText = catText;

            // Title
            var titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleText = titleGo.GetComponent<TextMeshProUGUI>();
            titleText.text = "Mission Title";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.white;
            hudComp.missionTitleText = titleText;

            // Description
            var descGo = new GameObject("DescriptionText", typeof(RectTransform), typeof(TextMeshProUGUI));
            descGo.transform.SetParent(panelGo.transform, false);
            var descText = descGo.GetComponent<TextMeshProUGUI>();
            descText.text = "Mission description goes here...";
            descText.fontSize = 12;
            descText.color = new Color(0.75f, 0.75f, 0.8f, 1f);
            hudComp.descriptionText = descText;

            // Separator Line
            var sepGo = new GameObject("Separator", typeof(RectTransform), typeof(Image));
            sepGo.transform.SetParent(panelGo.transform, false);
            var sepRt = sepGo.GetComponent<RectTransform>();
            sepRt.sizeDelta = new Vector2(0f, 1.5f);
            var sepImg = sepGo.GetComponent<Image>();
            sepImg.color = new Color(1f, 1f, 1f, 0.15f);

            // Objectives Container
            var objContainerGo = new GameObject("ObjectivesContainer", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            objContainerGo.transform.SetParent(panelGo.transform, false);
            var objVlg = objContainerGo.GetComponent<VerticalLayoutGroup>();
            objVlg.spacing = 4f;
            objVlg.childControlWidth = true;
            objVlg.childControlHeight = true;
            objVlg.childForceExpandWidth = true;
            objVlg.childForceExpandHeight = false;

            var objCsf = objContainerGo.GetComponent<ContentSizeFitter>();
            objCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            hudComp.objectivesContainer = objContainerGo.GetComponent<RectTransform>();

            // --- TOAST NOTIFICATION BANNER (Top Center) ---
            var toastGo = new GameObject("ToastBanner", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            toastGo.transform.SetParent(hudGo.transform, false);

            var toastRt = toastGo.GetComponent<RectTransform>();
            toastRt.anchorMin = new Vector2(0.5f, 1f);
            toastRt.anchorMax = new Vector2(0.5f, 1f);
            toastRt.pivot = new Vector2(0.5f, 1f);
            toastRt.anchoredPosition = new Vector2(0f, -40f);
            toastRt.sizeDelta = new Vector2(500f, 48f);

            var toastImg = toastGo.GetComponent<Image>();
            toastImg.color = new Color(0.06f, 0.08f, 0.12f, 0.92f);

            var toastCg = toastGo.GetComponent<CanvasGroup>();
            toastCg.alpha = 0f;
            hudComp.toastCanvasGroup = toastCg;

            var toastTextGo = new GameObject("ToastText", typeof(RectTransform), typeof(TextMeshProUGUI));
            toastTextGo.transform.SetParent(toastGo.transform, false);
            var toastTextRt = toastTextGo.GetComponent<RectTransform>();
            toastTextRt.anchorMin = Vector2.zero;
            toastTextRt.anchorMax = Vector2.one;
            toastTextRt.offsetMin = new Vector2(16, 0);
            toastTextRt.offsetMax = new Vector2(-16, 0);

            var toastTmp = toastTextGo.GetComponent<TextMeshProUGUI>();
            toastTmp.alignment = TextAlignmentOptions.Center;
            toastTmp.fontSize = 16;
            toastTmp.fontStyle = FontStyles.Bold;
            toastTmp.color = new Color(1f, 0.85f, 0.3f, 1f);
            hudComp.toastText = toastTmp;

            return hudGo;
        }
    }
}
