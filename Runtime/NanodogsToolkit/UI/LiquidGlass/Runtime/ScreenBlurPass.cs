using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace NanodogsToolkit.UI.LiquidGlass
{
    public class ScreenBlurPass : ScriptableRenderPass
    {
        private const string k_OutputTextureName = "_ScreenBlurTexture";
        private static readonly int s_ScreenBlurTextureId = Shader.PropertyToID(k_OutputTextureName);
        private static readonly int s_ScreenBlurTexelSizeId = Shader.PropertyToID("_ScreenBlurTexture_TexelSize");
        private static readonly int s_BlurOffsetId = Shader.PropertyToID("_BlurOffset");
        private static readonly int s_VibrancyId = Shader.PropertyToID("_Vibrancy");

        private readonly ScreenBlurSettings m_Settings;
        private readonly Material m_BlurMaterial;
        private RTHandle m_BlurOutputHandle;

        public ScreenBlurPass(ScreenBlurSettings settings, Material material)
        {
            m_Settings = settings;
            m_BlurMaterial = material;
            renderPassEvent = settings.renderPassEvent;
            requiresIntermediateTexture = true;
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_BlurMaterial == null)
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            if (cameraData.camera == null)
                return;

            // Validate camera type
            if (cameraData.camera.cameraType != CameraType.Game)
            {
                if (!m_Settings.renderInSceneView || cameraData.camera.cameraType != CameraType.SceneView)
                    return;
            }

            TextureHandle source = resourceData.activeColorTexture;
            if (!source.IsValid())
                return;

            // Update shader properties
            m_BlurMaterial.SetFloat(s_BlurOffsetId, m_Settings.spread);
            m_BlurMaterial.SetVector(s_VibrancyId, new Vector4(m_Settings.saturation, m_Settings.brightness, 0f, 0f));

            // Determine dimensions based on downsampling factor
            RenderTextureDescriptor cameraTargetDesc = cameraData.cameraTargetDescriptor;
            int down = Mathf.Max(1, (int)m_Settings.downsample);
            int width = Mathf.Max(1, cameraTargetDesc.width / down);
            int height = Mathf.Max(1, cameraTargetDesc.height / down);

            RenderTextureDescriptor blurDesc = cameraTargetDesc;
            blurDesc.width = width;
            blurDesc.height = height;
            blurDesc.depthBufferBits = 0;
            blurDesc.msaaSamples = 1;

            // Reallocate persistent output handle if dimensions or format changed
            RenderingUtils.ReAllocateHandleIfNeeded(
                ref m_BlurOutputHandle,
                blurDesc,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp,
                name: k_OutputTextureName
            );

            // Bind output handle to global shader variable for UGUI shaders
            Shader.SetGlobalTexture(s_ScreenBlurTextureId, m_BlurOutputHandle);
            Shader.SetGlobalVector(s_ScreenBlurTexelSizeId, new Vector4(1f / width, 1f / height, width, height));

            // Create Render Graph transient textures
            TextureDesc descA = new TextureDesc(width, height)
            {
                colorFormat = cameraTargetDesc.graphicsFormat,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "_ScreenBlur_RG_TempA"
            };

            TextureDesc descB = new TextureDesc(width, height)
            {
                colorFormat = cameraTargetDesc.graphicsFormat,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "_ScreenBlur_RG_TempB"
            };

            TextureHandle tempA = renderGraph.CreateTexture(descA);
            TextureHandle tempB = renderGraph.CreateTexture(descB);

            // 1. Downsample pass (Pass 0: Box Downsample)
            RenderGraphUtils.BlitMaterialParameters downsampleBlit = new(source, tempA, m_BlurMaterial, 0);
            using (var b0 = renderGraph.AddBlitPass(downsampleBlit, "ScreenBlur_Downsample", returnBuilder: true))
            {
                b0?.AllowPassCulling(false);
            }

            // 2. Multi-iteration blur passes
            if (m_Settings.algorithm == BlurAlgorithm.SeparableGaussian)
            {
                for (int i = 0; i < m_Settings.iterations; i++)
                {
                    // Pass 1: Horizontal Gaussian
                    RenderGraphUtils.BlitMaterialParameters passH = new(tempA, tempB, m_BlurMaterial, 1);
                    using (var bH = renderGraph.AddBlitPass(passH, $"ScreenBlur_GaussianH_{i}", returnBuilder: true))
                    {
                        bH?.AllowPassCulling(false);
                    }

                    // Pass 2: Vertical Gaussian
                    RenderGraphUtils.BlitMaterialParameters passV = new(tempB, tempA, m_BlurMaterial, 2);
                    using (var bV = renderGraph.AddBlitPass(passV, $"ScreenBlur_GaussianV_{i}", returnBuilder: true))
                    {
                        bV?.AllowPassCulling(false);
                    }
                }
            }
            else // Dual Kawase
            {
                for (int i = 0; i < m_Settings.iterations; i++)
                {
                    // Pass 3: Kawase Downsample
                    RenderGraphUtils.BlitMaterialParameters passDown = new(tempA, tempB, m_BlurMaterial, 3);
                    using (var bDown = renderGraph.AddBlitPass(passDown, $"ScreenBlur_KawaseDown_{i}", returnBuilder: true))
                    {
                        bDown?.AllowPassCulling(false);
                    }

                    // Pass 4: Kawase Upsample
                    RenderGraphUtils.BlitMaterialParameters passUp = new(tempB, tempA, m_BlurMaterial, 4);
                    using (var bUp = renderGraph.AddBlitPass(passUp, $"ScreenBlur_KawaseUp_{i}", returnBuilder: true))
                    {
                        bUp?.AllowPassCulling(false);
                    }
                }
            }

            // 3. Final copy into persistent global output (Pass 5: FinalBlit with Vibrancy)
            TextureHandle destination = renderGraph.ImportTexture(m_BlurOutputHandle);
            RenderGraphUtils.BlitMaterialParameters finalBlit = new(tempA, destination, m_BlurMaterial, 5);
            using (var bFinal = renderGraph.AddBlitPass(finalBlit, "ScreenBlur_OutputCopy", returnBuilder: true))
            {
                bFinal?.AllowPassCulling(false);
                bFinal?.SetGlobalTextureAfterPass(destination, s_ScreenBlurTextureId);
            }
        }

        public void Dispose()
        {
            m_BlurOutputHandle?.Release();
        }
    }
}
