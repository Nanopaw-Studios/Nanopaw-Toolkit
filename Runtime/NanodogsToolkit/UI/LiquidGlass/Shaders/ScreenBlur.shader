Shader "Hidden/Nanodogs/ScreenBlur" {
    Properties {
        _BlurOffset ("Blur Offset / Spread", Float) = 1.4
        _Vibrancy ("Vibrancy Boost", Vector) = (1.2, 1.05, 0.0, 0.0) // x: Saturation, y: Brightness
    }

    SubShader {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest Always

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float _BlurOffset;
        float4 _Vibrancy; // x: saturation, y: brightness

        half3 BoostVibrancy(half3 c) {
            half luma = dot(c, half3(0.2126, 0.7152, 0.0722));
            c = lerp(half3(luma, luma, luma), c, _Vibrancy.x);
            c *= _Vibrancy.y;
            return c;
        }
        ENDHLSL

        // Pass 0: Box Downsample
        Pass {
            Name "BoxDownsample"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragDownsample

            float4 FragDownsample(Varyings i) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 d = _BlitTexture_TexelSize.xy * 0.5 * _BlurOffset;
                float2 uv = i.texcoord;

                half4 s = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(-d.x, -d.y), 0) +
                          SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2( d.x, -d.y), 0) +
                          SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(-d.x,  d.y), 0) +
                          SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2( d.x,  d.y), 0);
                return s * 0.25;
            }
            ENDHLSL
        }

        // Pass 1: Horizontal Gaussian Blur (9-tap via 5 bilinear fetches)
        Pass {
            Name "GaussianHorizontal"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragGaussianH

            float4 FragGaussianH(Varyings i) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float dx = _BlitTexture_TexelSize.x * _BlurOffset;
                float2 uv = i.texcoord;

                half4 col = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0) * 0.2270270270;
                col += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(1.3846153846 * dx, 0.0), 0) * 0.3162162162;
                col += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv - float2(1.3846153846 * dx, 0.0), 0) * 0.3162162162;
                col += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(3.2307692308 * dx, 0.0), 0) * 0.0702702703;
                col += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv - float2(3.2307692308 * dx, 0.0), 0) * 0.0702702703;

                return col;
            }
            ENDHLSL
        }

        // Pass 2: Vertical Gaussian Blur (9-tap via 5 bilinear fetches)
        Pass {
            Name "GaussianVertical"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragGaussianV

            float4 FragGaussianV(Varyings i) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float dy = _BlitTexture_TexelSize.y * _BlurOffset;
                float2 uv = i.texcoord;

                half4 col = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0) * 0.2270270270;
                col += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(0.0, 1.3846153846 * dy), 0) * 0.3162162162;
                col += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv - float2(0.0, 1.3846153846 * dy), 0) * 0.3162162162;
                col += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(0.0, 3.2307692308 * dy), 0) * 0.0702702703;
                col += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv - float2(0.0, 3.2307692308 * dy), 0) * 0.0702702703;

                return col;
            }
            ENDHLSL
        }

        // Pass 3: Dual Kawase Downsample
        Pass {
            Name "DualKawaseDownsample"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragKawaseDown

            float4 FragKawaseDown(Varyings i) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 halfPixel = _BlitTexture_TexelSize.xy * _BlurOffset;
                float2 uv = i.texcoord;

                half4 sum = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0) * 4.0;
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv - halfPixel, 0);
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + halfPixel, 0);
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(halfPixel.x, -halfPixel.y), 0);
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv - float2(halfPixel.x, -halfPixel.y), 0);
                return sum * 0.125;
            }
            ENDHLSL
        }

        // Pass 4: Dual Kawase Upsample
        Pass {
            Name "DualKawaseUpsample"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragKawaseUp

            float4 FragKawaseUp(Varyings i) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 halfPixel = _BlitTexture_TexelSize.xy * 0.5 * _BlurOffset;
                float2 uv = i.texcoord;

                half4 sum = 0;
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(-halfPixel.x * 2.0, 0.0), 0);
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(-halfPixel.x, halfPixel.y), 0) * 2.0;
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(0.0, halfPixel.y * 2.0), 0);
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(halfPixel.x, halfPixel.y), 0) * 2.0;
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(halfPixel.x * 2.0, 0.0), 0);
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(halfPixel.x, -halfPixel.y), 0) * 2.0;
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(0.0, -halfPixel.y * 2.0), 0);
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(-halfPixel.x, -halfPixel.y), 0) * 2.0;
                return sum * (1.0 / 12.0);
            }
            ENDHLSL
        }

        // Pass 5: Final Blit with Vibrancy
        Pass {
            Name "FinalBlit"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragFinal

            float4 FragFinal(Varyings i) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half4 col = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, i.texcoord, 0);
                col.rgb = BoostVibrancy(col.rgb);
                return col;
            }
            ENDHLSL
        }
    }
}
