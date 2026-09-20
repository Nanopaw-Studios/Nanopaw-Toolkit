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

            // Determine iteration count safely
            int requestedIters = Mathf.Clamp(m_Settings.iterations, 1, 8);

            // Precompute pyramid level dimensions
            int[] levelWidths = new int[requestedIters + 1];
            int[] levelHeights = new int[requestedIters + 1];
            levelWidths[0] = width;
            levelHeights[0] = height;

            int actualIters = 0;
            for (int i = 0; i < requestedIters; i++)
            {
                int nextW = Mathf.Max(1, levelWidths[i] / 2);
                int nextH = Mathf.Max(1, levelHeights[i] / 2);
                if (nextW <= 1 && nextH <= 1 && i > 0)
                    break;

                actualIters++;
                levelWidths[actualIters] = nextW;
                levelHeights[actualIters] = nextH;
            }

            // Create downsampling pyramid transient textures
            TextureHandle[] downPyramid = new TextureHandle[actualIters + 1];
            for (int i = 0; i <= actualIters; i++)
            {
                TextureDesc desc = new TextureDesc(levelWidths[i], levelHeights[i])
                {
                    colorFormat = cameraTargetDesc.graphicsFormat,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    name = $"_ScreenBlur_Down_{i}"
                };
                downPyramid[i] = renderGraph.CreateTexture(desc);
            }

            // 1. Initial capture & box downsample from screen color to Level 0
            RenderGraphUtils.BlitMaterialParameters blit0 = new(source, downPyramid[0], m_BlurMaterial, 0);
            using (var b0 = renderGraph.AddBlitPass(blit0, "ScreenBlur_Downsample0", returnBuilder: true))
            {
                b0?.AllowPassCulling(false);
            }

            TextureHandle currentSource;

            // 2. Multi-resolution pyramid execution
            if (m_Settings.algorithm == BlurAlgorithm.DualKawase)
            {
                // Downsample passes: Level i -> Level i + 1 (Pass 3: Kawase Downsample)
                for (int i = 0; i < actualIters; i++)
                {
                    RenderGraphUtils.BlitMaterialParameters passDown = new(downPyramid[i], downPyramid[i + 1], m_BlurMaterial, 3);
                    using (var bDown = renderGraph.AddBlitPass(passDown, $"ScreenBlur_KawaseDown_{i}", returnBuilder: true))
                    {
                        bDown?.AllowPassCulling(false);
                    }
                }

                // Upsample passes: Level i + 1 -> Level i (Pass 4: Kawase 8-tap Upsample)
                currentSource = downPyramid[actualIters];
                for (int i = actualIters - 1; i >= 0; i--)
                {
                    TextureDesc descUp = new TextureDesc(levelWidths[i], levelHeights[i])
                    {
                        colorFormat = cameraTargetDesc.graphicsFormat,
                        filterMode = FilterMode.Bilinear,
                        wrapMode = TextureWrapMode.Clamp,
                        name = $"_ScreenBlur_Up_{i}"
                    };
                    TextureHandle upTarget = renderGraph.CreateTexture(descUp);

                    RenderGraphUtils.BlitMaterialParameters passUp = new(currentSource, upTarget, m_BlurMaterial, 4);
                    using (var bUp = renderGraph.AddBlitPass(passUp, $"ScreenBlur_KawaseUp_{i}", returnBuilder: true))
                    {
                        bUp?.AllowPassCulling(false);
                    }

                    currentSource = upTarget;
                }
            }
            else // Separable Gaussian with multi-scale pyramid
            {
                // Downsample passes: Level i -> Level i + 1 (Pass 0: Box Downsample)
                for (int i = 0; i < actualIters; i++)
                {
                    RenderGraphUtils.BlitMaterialParameters passDown = new(downPyramid[i], downPyramid[i + 1], m_BlurMaterial, 0);
                    using (var bDown = renderGraph.AddBlitPass(passDown, $"ScreenBlur_GaussDown_{i}", returnBuilder: true))
                    {
                        bDown?.AllowPassCulling(false);
                    }
                }

                // Base level Gaussian filtering
                TextureDesc descTemp = new TextureDesc(levelWidths[actualIters], levelHeights[actualIters])
                {
                    colorFormat = cameraTargetDesc.graphicsFormat,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    name = "_ScreenBlur_GaussTemp"
                };
                TextureHandle tempG = renderGraph.CreateTexture(descTemp);

                RenderGraphUtils.BlitMaterialParameters passH = new(downPyramid[actualIters], tempG, m_BlurMaterial, 1);
                using (var bH = renderGraph.AddBlitPass(passH, "ScreenBlur_GaussianH_Base", returnBuilder: true))
                {
                    bH?.AllowPassCulling(false);
                }

                RenderGraphUtils.BlitMaterialParameters passV = new(tempG, downPyramid[actualIters], m_BlurMaterial, 2);
                using (var bV = renderGraph.AddBlitPass(passV, "ScreenBlur_GaussianV_Base", returnBuilder: true))
                {
                    bV?.AllowPassCulling(false);
                }

                currentSource = downPyramid[actualIters];

                // Upsample passes with Gaussian reconstruction at each scale
                for (int i = actualIters - 1; i >= 0; i--)
                {
                    TextureDesc descUp = new TextureDesc(levelWidths[i], levelHeights[i])
                    {
                        colorFormat = cameraTargetDesc.graphicsFormat,
                        filterMode = FilterMode.Bilinear,
                        wrapMode = TextureWrapMode.Clamp,
                        name = $"_ScreenBlur_Up_{i}"
                    };
                    TextureHandle upTarget = renderGraph.CreateTexture(descUp);

                    RenderGraphUtils.BlitMaterialParameters passUp = new(currentSource, upTarget, m_BlurMaterial, 4);
                    using (var bUp = renderGraph.AddBlitPass(passUp, $"ScreenBlur_GaussUp_{i}", returnBuilder: true))
                    {
                        bUp?.AllowPassCulling(false);
                    }

                    // Gaussian H & V at level i
                    TextureDesc descH = new TextureDesc(levelWidths[i], levelHeights[i])
                    {
                        colorFormat = cameraTargetDesc.graphicsFormat,
                        filterMode = FilterMode.Bilinear,
                        wrapMode = TextureWrapMode.Clamp,
                        name = $"_ScreenBlur_GaussH_{i}"
                    };
                    TextureHandle tempH = renderGraph.CreateTexture(descH);

                    RenderGraphUtils.BlitMaterialParameters pGH = new(upTarget, tempH, m_BlurMaterial, 1);
                    using (var bGH = renderGraph.AddBlitPass(pGH, $"ScreenBlur_GaussianH_{i}", returnBuilder: true))
                    {
                        bGH?.AllowPassCulling(false);
                    }

                    TextureDesc descV = new TextureDesc(levelWidths[i], levelHeights[i])
                    {
                        colorFormat = cameraTargetDesc.graphicsFormat,
                        filterMode = FilterMode.Bilinear,
                        wrapMode = TextureWrapMode.Clamp,
                        name = $"_ScreenBlur_GaussV_{i}"
                    };
                    TextureHandle targetV = renderGraph.CreateTexture(descV);

                    RenderGraphUtils.BlitMaterialParameters pGV = new(tempH, targetV, m_BlurMaterial, 2);
                    using (var bGV = renderGraph.AddBlitPass(pGV, $"ScreenBlur_GaussianV_{i}", returnBuilder: true))
                    {
                        bGV?.AllowPassCulling(false);
                    }

                    currentSource = targetV;
                }
            }

            // 3. Final copy into persistent global output with vibrancy boost (Pass 5)
            TextureHandle destination = renderGraph.ImportTexture(m_BlurOutputHandle);
            RenderGraphUtils.BlitMaterialParameters finalBlit = new(currentSource, destination, m_BlurMaterial, 5);
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
