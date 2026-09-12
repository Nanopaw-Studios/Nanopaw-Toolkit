Shader "UI/RoundedCorners/IndependentRoundedCornersLiquidGlass" {
    Properties {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}

        // --- Mask support ---
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        // Definition in Properties section is required for UI Rounded Corners
        _rect2props ("rect2props", Vector) = (0,0,0,0)
        _halfSize ("halfSize", Vector) = (50,50,0,0)
        _r ("r", Vector) = (10,10,10,10)

        // --- Liquid Glass Properties ---
        [Header(Surface Tint)]
        _TintColor ("Glass Tint Color", Color) = (1, 1, 1, 0.25)
        _TintBlendMode ("Tint Mode (0:Blend, 1:Multiply, 2:Add)", Float) = 0
        
        [Header(Blur and Vibrancy)]
        _BlurAmount ("Blur Mix", Range(0, 1)) = 1.0
        _Saturation ("Saturation Boost", Range(0, 3)) = 1.2
        _Brightness ("Brightness Boost", Range(0, 3)) = 1.05
        _NoiseScale ("Frosted Noise Scale", Float) = 180.0
        _NoiseStrength ("Frosted Noise Strength", Range(0, 0.2)) = 0.02
        [Toggle] _NoiseAnimate ("Animate Frosted Noise", Float) = 0

        [Header(Liquid Refraction)]
        _RefractionStrength ("Refraction Strength", Range(0, 0.15)) = 0.025
        _ChromaticAberration ("Chromatic Dispersion", Range(0, 1)) = 0.35
        _BumpMap ("Distortion / Ripple Map", 2D) = "bump" {}
        _BumpTiling ("Distortion Tiling", Vector) = (1, 1, 0, 0)
        _WaveSpeed ("Liquid Wave Speed", Float) = 1.2
        _WaveFreq ("Liquid Wave Frequency", Vector) = (3, 3, 0, 0)
        _WaveStrength ("Liquid Wave Strength", Range(0, 0.05)) = 0.006

        [Header(Edge Meniscus Refraction)]
        _MeniscusWidth ("Meniscus Width", Range(0, 60)) = 18.0
        _MeniscusExp ("Meniscus Curve Exponent", Range(0.5, 4.0)) = 2.0
        _MeniscusStrength ("Meniscus Distortion", Range(0, 0.1)) = 0.04

        [Header(3D Directional Bevel)]
        _LightAngle ("Light Direction (Deg)", Range(0, 360)) = 135.0
        _BevelWidth ("Bevel Width", Range(0, 40)) = 8.0
        _BevelHighlightIntensity ("Bevel Highlight", Range(0, 3)) = 1.3
        _BevelHighlightColor ("Highlight Color", Color) = (1, 1, 1, 1)
        _BevelShadowIntensity ("Bevel Shadow", Range(0, 2)) = 0.4
        _BevelShadowColor ("Shadow Color", Color) = (0, 0, 0, 1)

        [Header(Rim Glow and Border)]
        _RimWidth ("Rim Glow Width", Range(0, 40)) = 10.0
        _RimIntensity ("Rim Glow Intensity", Range(0, 3)) = 0.7
        _RimColor ("Rim Glow Color", Color) = (1, 1, 1, 1)
        _BorderWidth ("Border Stroke Width", Range(0, 10)) = 0.0
        _BorderColor ("Border Stroke Color", Color) = (1, 1, 1, 0.6)

        [Header(Specular Sheen)]
        _SheenAngle ("Sheen Angle (Deg)", Range(0, 360)) = 45.0
        _SheenOffset ("Sheen Position", Range(-1, 1)) = 0.0
        _SheenWidth ("Sheen Width", Range(0.01, 1)) = 0.3
        _SheenIntensity ("Sheen Intensity", Range(0, 2)) = 0.4
        _SheenColor ("Sheen Color", Color) = (1, 1, 1, 1)
        [Toggle] _SheenAnimate ("Animate Sheen Streak", Float) = 0
        _SheenSpeed ("Sheen Sweep Speed", Float) = 0.4

        [Header(Compatibility)]
        [Toggle] _FlipScreenY ("Flip Screen Y", Float) = 0
    }

    SubShader {
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass {
            Name "IndependentLiquidGlassRounded"
            CGPROGRAM
            #pragma vertex vert_lg
            #pragma fragment frag_lg
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "LiquidGlassCommon.cginc"

            struct appdata_lg {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f_lg {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float4 color : COLOR;
                float4 worldPosition : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _BumpMap;
            sampler2D _ScreenBlurTexture;
            float4 _ScreenBlurTexture_TexelSize;

            float4 _rect2props;
            float4 _halfSize;
            float4 _r; // x: topLeft, y: topRight, z: bottomRight, w: bottomLeft

            half4 _TintColor;
            float _TintBlendMode;
            half _BlurAmount;
            half _Saturation;
            half _Brightness;
            float _NoiseScale;
            half _NoiseStrength;
            float _NoiseAnimate;

            half _RefractionStrength;
            half _ChromaticAberration;
            float4 _BumpTiling;
            float _WaveSpeed;
            float2 _WaveFreq;
            float _WaveStrength;

            float _MeniscusWidth;
            float _MeniscusExp;
            half _MeniscusStrength;

            float _LightAngle;
            float _BevelWidth;
            half _BevelHighlightIntensity;
            half4 _BevelHighlightColor;
            half _BevelShadowIntensity;
            half4 _BevelShadowColor;

            float _RimWidth;
            half _RimIntensity;
            half4 _RimColor;
            float _BorderWidth;
            half4 _BorderColor;

            float _SheenAngle;
            float _SheenOffset;
            float _SheenWidth;
            half _SheenIntensity;
            half4 _SheenColor;
            float _SheenAnimate;
            float _SheenSpeed;

            float _FlipScreenY;
            float4 _ClipRect;

            v2f_lg vert_lg(appdata_lg v) {
                v2f_lg o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag_lg(v2f_lg i) : SV_Target {
                // 1. Calculate Signed Distance Field for independent corners
                float dist = LGCalcDistanceIndependent(i.uv, _halfSize.xy, _rect2props, _r);

                // Antialiased corner cut alpha
                float cornerAlpha = LGAntialiasedCutoff(dist);
                #ifdef UNITY_UI_CLIP_RECT
                cornerAlpha *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                clip(cornerAlpha - 0.001);

                // 2. Compute 2D SDF normal on edges
                float2 normal = LGGetSDFNormal(dist, i.uv, _halfSize.xy * 2.0);

                // 3. Screen coordinates
                float2 screenUV = LGGetScreenUV(i.screenPos, _FlipScreenY);

                // 4. Normal map and procedural liquid waves
                float2 bumpUV = i.uv * _BumpTiling.xy + _BumpTiling.zw;
                half3 unpackedNormal = UnpackNormal(tex2D(_BumpMap, bumpUV));
                float2 waveDisp = LGEvaluateLiquidWaves(i.uv, _WaveSpeed, _WaveFreq, _WaveStrength);

                // 5. Edge meniscus refraction
                float2 meniscusDisp = LGGetMeniscusDisplacement(dist, normal, _MeniscusWidth, _MeniscusExp, _MeniscusStrength);

                // Total refraction displacement
                float2 totalDisp = (unpackedNormal.xy * _RefractionStrength) + waveDisp + meniscusDisp;

                // 6. Sample background with chromatic dispersion
                half3 sceneBlur = LGSampleChromaticAberration(_ScreenBlurTexture, screenUV, totalDisp, _ChromaticAberration);

                // Fallback preview when texture is unrendered
                if (dot(sceneBlur, sceneBlur) < 0.0001) {
                    sceneBlur = lerp(half3(0.2, 0.25, 0.32), half3(0.4, 0.48, 0.6), screenUV.y);
                }

                // 7. Vibrancy & Saturation boost
                sceneBlur = LGAdjustVibrancy(sceneBlur, _Saturation, _Brightness);

                // 8. Frosted noise dither
                float noise = LGComputeFrostedNoise(i.uv, _NoiseScale, _NoiseAnimate);
                sceneBlur += (noise - 0.5) * _NoiseStrength;

                // 9. 3D Directional Bevel & Rim Light
                half3 bevelLight = 0;
                half3 rimLight = 0;
                LGComputeBevelAndRim(
                    dist, normal, _LightAngle, _BevelWidth,
                    _BevelHighlightIntensity, _BevelShadowIntensity,
                    _BevelHighlightColor.rgb, _BevelShadowColor.rgb,
                    _RimWidth, _RimIntensity, _RimColor.rgb,
                    bevelLight, rimLight
                );

                // 10. Specular Sheen
                half3 sheen = LGComputeSpecularSheen(
                    i.uv, _SheenAngle, _SheenOffset, _SheenWidth,
                    _SheenIntensity, _SheenColor.rgb, _SheenSpeed, _SheenAnimate
                );

                // 11. Color Tinting and Composition
                half4 tint = _TintColor * i.color;
                half3 finalRGB = sceneBlur;

                // Tint blend mode: 0 = Alpha blend, 1 = Multiply, 2 = Additive
                if (_TintBlendMode > 1.5) {
                    finalRGB += tint.rgb * tint.a;
                } else if (_TintBlendMode > 0.5) {
                    finalRGB = lerp(finalRGB, finalRGB * tint.rgb, tint.a);
                } else {
                    finalRGB = lerp(finalRGB, tint.rgb, tint.a);
                }

                // Add glass specular, rim, and bevel lighting
                finalRGB += bevelLight + rimLight + sheen;

                // 12. Border stroke
                if (_BorderWidth > 0.01) {
                    half4 borderCol = LGComputeBorderStroke(dist, _BorderWidth, _BorderColor);
                    finalRGB = lerp(finalRGB, borderCol.rgb, borderCol.a);
                }

                // Final alpha: 1.0 inside the panel, clipped by corner SDF
                float finalAlpha = cornerAlpha * i.color.a;

                return fixed4(finalRGB, finalAlpha);
            }
            ENDCG
        }
    }
    FallBack "UI/RoundedCorners/IndependentRoundedCorners"
}
