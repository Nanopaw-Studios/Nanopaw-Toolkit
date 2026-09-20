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
        [Tooltip("The blur algorithm: Dual Kawase (smooth multi-scale pyramid blur) or Separable Gaussian (accurate 1D filter).")]
        public BlurAlgorithm algorithm = BlurAlgorithm.DualKawase;

        [Tooltip("Downsample factor for the blur pass buffer. 2x (Half) or 4x (Quarter) is recommended for deep, creamy blur and optimal performance.")]
        public BlurDownsample downsample = BlurDownsample.Half;

        [Range(1, 8), Tooltip("Number of pyramid blur iterations. Higher values dramatically increase blur radius and smoothness.")]
        public int iterations = 4;

        [Range(0.2f, 10f), Tooltip("Blur radius / kernel spread multiplier. Higher values expand the blur across a larger screen area.")]
        public float spread = 2.0f;

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
