// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;

namespace Nanodogs.UniversalScripts
{
    /// <summary>
    /// Helper component for testing interactions and hover feedback.
    /// Can toggle a light, change a renderer's color, or log messages on interaction and hover events.
    /// </summary>
    public class InteractionTestFeedback : MonoBehaviour
    {
        [Header("Light Feedback")]
        [Tooltip("Optional Light to toggle or illuminate when interacted with.")]
        public Light targetLight;

        [Header("Visual Feedback")]
        [Tooltip("Optional Renderer to change color on hover or interact.")]
        public Renderer targetRenderer;
        public Color defaultColor = Color.white;
        public Color hoverColor = Color.yellow;
        public Color interactColor = Color.green;

        [Header("Sound Feedback")]
        [Tooltip("Optional AudioSource to play feedback sounds.")]
        public AudioSource audioSource;
        public AudioClip interactSound;

        private Material propertyMaterial;
        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProp = Shader.PropertyToID("_Color");

        protected void Awake()
        {
            if (targetRenderer != null)
            {
                propertyMaterial = targetRenderer.material;
                if (propertyMaterial.HasProperty(BaseColorProp))
                {
                    defaultColor = propertyMaterial.GetColor(BaseColorProp);
                }
                else if (propertyMaterial.HasProperty(ColorProp))
                {
                    defaultColor = propertyMaterial.GetColor(ColorProp);
                }
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        public void ToggleLight()
        {
            if (targetLight != null)
            {
                targetLight.enabled = !targetLight.enabled;
                Debug.Log($"[InteractionTestFeedback] Light toggled to: {(targetLight.enabled ? "ON" : "OFF")}");
            }
        }

        public void SetLightState(bool state)
        {
            if (targetLight != null)
            {
                targetLight.enabled = state;
            }
        }

        public void OnHoverEnterFeedback()
        {
            Debug.Log($"[InteractionTestFeedback] Hover entered on: {gameObject.name}");
            SetRendererColor(hoverColor);
        }

        public void OnHoverExitFeedback()
        {
            Debug.Log($"[InteractionTestFeedback] Hover exited on: {gameObject.name}");
            SetRendererColor(defaultColor);
        }

        public void OnInteractFeedback()
        {
            Debug.Log($"[InteractionTestFeedback] Interacted with: {gameObject.name}");
            ToggleLight();
            SetRendererColor(interactColor);

            if (audioSource != null && interactSound != null)
            {
                audioSource.PlayOneShot(interactSound);
            }
        }

        public void LogMessage(string message)
        {
            Debug.Log($"[InteractionTestFeedback] {message}");
        }

        private void SetRendererColor(Color color)
        {
            if (propertyMaterial != null)
            {
                if (propertyMaterial.HasProperty(BaseColorProp))
                {
                    propertyMaterial.SetColor(BaseColorProp, color);
                }
                else if (propertyMaterial.HasProperty(ColorProp))
                {
                    propertyMaterial.SetColor(ColorProp, color);
                }
            }
        }
    }
}
