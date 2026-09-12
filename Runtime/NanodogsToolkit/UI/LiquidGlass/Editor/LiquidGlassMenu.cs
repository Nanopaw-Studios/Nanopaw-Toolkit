#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Nobi.UiRoundedCorners;

namespace NanodogsToolkit.UI.LiquidGlass.Editor
{
    public static class LiquidGlassMenu
    {
        [MenuItem("GameObject/UI/Liquid Glass Panel", false, 20)]
        public static void CreateLiquidGlassPanel(MenuCommand menuCommand)
        {
            // Ensure Canvas exists
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            GameObject parent = menuCommand.context as GameObject;

            if (parent == null)
            {
                if (canvas == null)
                {
                    GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                    canvas = canvasGo.GetComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");

                    if (Object.FindFirstObjectByType<EventSystem>() == null)
                    {
                        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                        Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
                    }
                }
                parent = canvas.gameObject;
            }
            else if (parent.GetComponentInParent<Canvas>() == null)
            {
                if (canvas != null)
                {
                    parent = canvas.gameObject;
                }
            }

            // Create panel
            GameObject panelGo = new GameObject("LiquidGlassPanel", typeof(RectTransform), typeof(Image), typeof(LiquidGlass));
            GameObjectUtility.SetParentAndAlign(panelGo, parent);

            RectTransform rect = panelGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400f, 260f);
            rect.anchoredPosition = Vector2.zero;

            Image image = panelGo.GetComponent<Image>();
            image.color = Color.white;

            LiquidGlass glass = panelGo.GetComponent<LiquidGlass>();
            glass.Initialize();
            glass.Refresh();

            Undo.RegisterCreatedObjectUndo(panelGo, "Create Liquid Glass Panel");
            Selection.activeGameObject = panelGo;
        }

        [MenuItem("CONTEXT/ImageWithRoundedCorners/Add Liquid Glass Blur")]
        public static void AddLiquidGlassToRounded(MenuCommand command)
        {
            var comp = (ImageWithRoundedCorners)command.context;
            if (comp != null && !comp.TryGetComponent<LiquidGlass>(out var _))
            {
                Undo.AddComponent<LiquidGlass>(comp.gameObject);
            }
        }

        [MenuItem("CONTEXT/ImageWithIndependentRoundedCorners/Add Liquid Glass Blur")]
        public static void AddLiquidGlassToIndependent(MenuCommand command)
        {
            var comp = (ImageWithIndependentRoundedCorners)command.context;
            if (comp != null && !comp.TryGetComponent<LiquidGlass>(out var _))
            {
                Undo.AddComponent<LiquidGlass>(comp.gameObject);
            }
        }
    }
}
#endif
