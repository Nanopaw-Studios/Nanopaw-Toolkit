using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NanodogsToolkit.UI.LiquidGlass
{
    public enum BlurAlgorithm
    {
        SeparableGaussian = 0,
        DualKawase = 1
    }

    public enum BlurDownsample
    {
        None = 1,
        Half = 2,
        Quarter = 4,
        Eighth = 8
    }

    [Serializable]
    public class ScreenBlurSettings
    {
        [Tooltip("The blur algorithm: Separable Gaussian (accurate 9-tap 1D filter) or Dual Kawase (smooth multi-scale blur).")]
        public BlurAlgorithm algorithm = BlurAlgorithm.SeparableGaussian;

        [Tooltip("Downsample factor for the blur pass buffer. 2x or 4x is recommended for optimal performance and smoothness.")]
        public BlurDownsample downsample = BlurDownsample.Half;

        [Range(1, 8), Tooltip("Number of blur iterations. Higher values increase blur smoothness.")]
        public int iterations = 3;

        [Range(0.2f, 5f), Tooltip("Blur radius / kernel spread multiplier.")]
        public float spread = 1.4f;

        [Tooltip("When in the rendering pipeline the screen should be captured and blurred.")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        [Tooltip("Whether to compute and display the blur in the Unity Scene View.")]
        public bool renderInSceneView = true;

        [Header("Global Vibrancy")]
        [Range(0f, 3f), Tooltip("Boost background color saturation seen through glass.")]
        public float saturation = 1.2f;

        [Range(0f, 3f), Tooltip("Boost background exposure / brightness seen through glass.")]
        public float brightness = 1.05f;
    }
}
