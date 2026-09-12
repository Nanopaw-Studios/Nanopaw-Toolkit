using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NanodogsToolkit.UI.LiquidGlass
{
    [DisallowMultipleRendererFeature("Screen Blur (UGUI Liquid Glass)")]
    [Tooltip("Captures and generates downsampled Gaussian / Dual Kawase screen blur for UGUI Liquid Glass UI elements.")]
    public class ScreenBlurRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private ScreenBlurSettings m_Settings = new ScreenBlurSettings();

        [SerializeField, HideInInspector]
        private Shader m_BlurShader;

        private Material m_BlurMaterial;
        private ScreenBlurPass m_BlurPass;

        public ScreenBlurSettings Settings => m_Settings;

        public override void Create()
        {
            if (m_BlurShader == null)
            {
                m_BlurShader = Shader.Find("Hidden/Nanodogs/ScreenBlur");
            }

            if (m_BlurShader != null)
            {
                if (m_BlurMaterial == null)
                {
                    m_BlurMaterial = CoreUtils.CreateEngineMaterial(m_BlurShader);
                }
                m_BlurPass = new ScreenBlurPass(m_Settings, m_BlurMaterial);
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_BlurPass == null || m_BlurMaterial == null)
            {
                Create();
            }

            if (m_BlurPass != null && m_BlurMaterial != null)
            {
                renderer.EnqueuePass(m_BlurPass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            m_BlurPass?.Dispose();
            m_BlurPass = null;
            CoreUtils.Destroy(m_BlurMaterial);
            m_BlurMaterial = null;
        }
    }
}
