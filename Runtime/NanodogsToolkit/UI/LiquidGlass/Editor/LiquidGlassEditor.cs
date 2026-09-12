#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Nobi.UiRoundedCorners;

namespace NanodogsToolkit.UI.LiquidGlass.Editor
{
    [CustomEditor(typeof(LiquidGlass))]
    [CanEditMultipleObjects]
    public class LiquidGlassEditor : UnityEditor.Editor
    {
        private SerializedProperty m_CornerMode;
        private SerializedProperty m_UniformRadius;
        private SerializedProperty m_IndependentRadii;

        private SerializedProperty m_TintColor;
        private SerializedProperty m_TintBlendMode;

        private SerializedProperty m_BlurAmount;
        private SerializedProperty m_Saturation;
        private SerializedProperty m_Brightness;
        private SerializedProperty m_NoiseScale;
        private SerializedProperty m_NoiseStrength;
        private SerializedProperty m_AnimateNoise;

        private SerializedProperty m_RefractionStrength;
        private SerializedProperty m_ChromaticAberration;
        private SerializedProperty m_DistortionMap;
        private SerializedProperty m_DistortionTiling;
        private SerializedProperty m_WaveSpeed;
        private SerializedProperty m_WaveFrequency;
        private SerializedProperty m_WaveStrength;

        private SerializedProperty m_MeniscusWidth;
        private SerializedProperty m_MeniscusExponent;
        private SerializedProperty m_MeniscusStrength;

        private SerializedProperty m_LightAngle;
        private SerializedProperty m_BevelWidth;
        private SerializedProperty m_BevelHighlightIntensity;
        private SerializedProperty m_BevelHighlightColor;
        private SerializedProperty m_BevelShadowIntensity;
        private SerializedProperty m_BevelShadowColor;

        private SerializedProperty m_RimWidth;
        private SerializedProperty m_RimIntensity;
        private SerializedProperty m_RimColor;
        private SerializedProperty m_BorderWidth;
        private SerializedProperty m_BorderColor;

        private SerializedProperty m_SheenAngle;
        private SerializedProperty m_SheenOffset;
        private SerializedProperty m_SheenWidth;
        private SerializedProperty m_SheenIntensity;
        private SerializedProperty m_SheenColor;
        private SerializedProperty m_AnimateSheen;
        private SerializedProperty m_SheenSpeed;

        private SerializedProperty m_FlipScreenY;

        private static bool s_ShowPresets = true;
        private static bool s_ShowCorners = true;
        private static bool s_ShowTint = true;
        private static bool s_ShowBlur = true;
        private static bool s_ShowRefraction = true;
        private static bool s_ShowMeniscus = true;
        private static bool s_ShowBevel = true;
        private static bool s_ShowRim = true;
        private static bool s_ShowSheen = true;

        private void OnEnable()
        {
            m_CornerMode = serializedObject.FindProperty("m_CornerMode");
            m_UniformRadius = serializedObject.FindProperty("m_UniformRadius");
            m_IndependentRadii = serializedObject.FindProperty("m_IndependentRadii");

            m_TintColor = serializedObject.FindProperty("m_TintColor");
            m_TintBlendMode = serializedObject.FindProperty("m_TintBlendMode");

            m_BlurAmount = serializedObject.FindProperty("m_BlurAmount");
            m_Saturation = serializedObject.FindProperty("m_Saturation");
            m_Brightness = serializedObject.FindProperty("m_Brightness");
            m_NoiseScale = serializedObject.FindProperty("m_NoiseScale");
            m_NoiseStrength = serializedObject.FindProperty("m_NoiseStrength");
            m_AnimateNoise = serializedObject.FindProperty("m_AnimateNoise");

            m_RefractionStrength = serializedObject.FindProperty("m_RefractionStrength");
            m_ChromaticAberration = serializedObject.FindProperty("m_ChromaticAberration");
            m_DistortionMap = serializedObject.FindProperty("m_DistortionMap");
            m_DistortionTiling = serializedObject.FindProperty("m_DistortionTiling");
            m_WaveSpeed = serializedObject.FindProperty("m_WaveSpeed");
            m_WaveFrequency = serializedObject.FindProperty("m_WaveFrequency");
            m_WaveStrength = serializedObject.FindProperty("m_WaveStrength");

            m_MeniscusWidth = serializedObject.FindProperty("m_MeniscusWidth");
            m_MeniscusExponent = serializedObject.FindProperty("m_MeniscusExponent");
            m_MeniscusStrength = serializedObject.FindProperty("m_MeniscusStrength");

            m_LightAngle = serializedObject.FindProperty("m_LightAngle");
            m_BevelWidth = serializedObject.FindProperty("m_BevelWidth");
            m_BevelHighlightIntensity = serializedObject.FindProperty("m_BevelHighlightIntensity");
            m_BevelHighlightColor = serializedObject.FindProperty("m_BevelHighlightColor");
            m_BevelShadowIntensity = serializedObject.FindProperty("m_BevelShadowIntensity");
            m_BevelShadowColor = serializedObject.FindProperty("m_BevelShadowColor");

            m_RimWidth = serializedObject.FindProperty("m_RimWidth");
            m_RimIntensity = serializedObject.FindProperty("m_RimIntensity");
            m_RimColor = serializedObject.FindProperty("m_RimColor");
            m_BorderWidth = serializedObject.FindProperty("m_BorderWidth");
            m_BorderColor = serializedObject.FindProperty("m_BorderColor");

            m_SheenAngle = serializedObject.FindProperty("m_SheenAngle");
            m_SheenOffset = serializedObject.FindProperty("m_SheenOffset");
            m_SheenWidth = serializedObject.FindProperty("m_SheenWidth");
            m_SheenIntensity = serializedObject.FindProperty("m_SheenIntensity");
            m_SheenColor = serializedObject.FindProperty("m_SheenColor");
            m_AnimateSheen = serializedObject.FindProperty("m_AnimateSheen");
            m_SheenSpeed = serializedObject.FindProperty("m_SheenSpeed");

            m_FlipScreenY = serializedObject.FindProperty("m_FlipScreenY");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            LiquidGlass lg = (LiquidGlass)target;

            // 1. Check URP Renderer Feature Status
            DrawURPFeatureBanner();

            // 2. Presets Header
            DrawPresetsSection(lg);

            EditorGUILayout.Space(4);

            // 3. Corner Rounding & UI Rounded Corners integration
            DrawCornersSection(lg);

            // 4. Surface Tint & Mode
            DrawTintSection();

            // 5. Blur & Vibrancy
            DrawBlurSection();

            // 6. Liquid Refraction & Waves
            DrawRefractionSection();

            // 7. Edge Meniscus Refraction
            DrawMeniscusSection();

            // 8. 3D Bevel & Chamfer
            DrawBevelSection();

            // 9. Rim Glow & Border
            DrawRimSection();

            // 10. Specular Sheen
            DrawSheenSection();

            // 11. Compatibility
            EditorGUILayout.PropertyField(m_FlipScreenY);

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                foreach (var obj in targets)
                {
                    if (obj is LiquidGlass item)
                    {
                        item.Refresh();
                        EditorUtility.SetDirty(item);
                    }
                }
            }
        }

        private void DrawPresetsSection(LiquidGlass lg)
        {
            s_ShowPresets = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowPresets, "✨ Style Presets");
            if (s_ShowPresets)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apple Glass", EditorStyles.miniButtonLeft))
                {
                    ApplyAppleGlassPreset();
                }
                if (GUILayout.Button("Liquid Ripple", EditorStyles.miniButtonMid))
                {
                    ApplyLiquidRipplePreset();
                }
                if (GUILayout.Button("Dark Acrylic", EditorStyles.miniButtonMid))
                {
                    ApplyDarkAcrylicPreset();
                }
                if (GUILayout.Button("Cyber Neon", EditorStyles.miniButtonMid))
                {
                    ApplyCyberNeonPreset();
                }
                if (GUILayout.Button("Clear Lens", EditorStyles.miniButtonRight))
                {
                    ApplyClearLensPreset();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawCornersSection(LiquidGlass lg)
        {
            s_ShowCorners = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowCorners, "🔲 Corner Rounding (UI Rounded Corners)");
            if (s_ShowCorners)
            {
                // Check if companion components exist
                var roundedComp = lg.GetComponent<ImageWithRoundedCorners>();
                var independentComp = lg.GetComponent<ImageWithIndependentRoundedCorners>();

                if (independentComp != null)
                {
                    EditorGUILayout.HelpBox("Controlled by attached [ImageWithIndependentRoundedCorners] component.", MessageType.Info);
                }
                else if (roundedComp != null)
                {
                    EditorGUILayout.HelpBox("Controlled by attached [ImageWithRoundedCorners] component.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.PropertyField(m_CornerMode);
                    var mode = (CornerRoundingMode)m_CornerMode.enumValueIndex;
                    if (mode == CornerRoundingMode.Uniform)
                    {
                        EditorGUILayout.PropertyField(m_UniformRadius, new GUIContent("Corner Radius"));
                    }
                    else if (mode == CornerRoundingMode.Independent)
                    {
                        EditorGUILayout.LabelField("Independent Radii (Pixels)", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        float tl = EditorGUILayout.FloatField("Top Left", m_IndependentRadii.vector4Value.x);
                        float tr = EditorGUILayout.FloatField("Top Right", m_IndependentRadii.vector4Value.y);
                        float br = EditorGUILayout.FloatField("Bottom Right", m_IndependentRadii.vector4Value.z);
                        float bl = EditorGUILayout.FloatField("Bottom Left", m_IndependentRadii.vector4Value.w);
                        m_IndependentRadii.vector4Value = new Vector4(tl, tr, br, bl);
                        EditorGUI.indentLevel--;
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawTintSection()
        {
            s_ShowTint = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowTint, "🎨 Surface Tint & Color");
            if (s_ShowTint)
            {
                EditorGUILayout.PropertyField(m_TintColor);
                EditorGUILayout.PropertyField(m_TintBlendMode);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawBlurSection()
        {
            s_ShowBlur = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowBlur, "🌫️ Blur & Vibrancy");
            if (s_ShowBlur)
            {
                EditorGUILayout.PropertyField(m_BlurAmount);
                EditorGUILayout.PropertyField(m_Saturation);
                EditorGUILayout.PropertyField(m_Brightness);
                EditorGUILayout.PropertyField(m_NoiseScale);
                EditorGUILayout.PropertyField(m_NoiseStrength);
                EditorGUILayout.PropertyField(m_AnimateNoise);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawRefractionSection()
        {
            s_ShowRefraction = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowRefraction, "💧 Liquid Refraction & Waves");
            if (s_ShowRefraction)
            {
                EditorGUILayout.PropertyField(m_RefractionStrength);
                EditorGUILayout.PropertyField(m_ChromaticAberration);
                EditorGUILayout.PropertyField(m_DistortionMap);
                EditorGUILayout.PropertyField(m_DistortionTiling);
                EditorGUILayout.PropertyField(m_WaveSpeed);
                EditorGUILayout.PropertyField(m_WaveFrequency);
                EditorGUILayout.PropertyField(m_WaveStrength);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawMeniscusSection()
        {
            s_ShowMeniscus = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowMeniscus, "🫧 Edge Meniscus (Fluid Surface Tension)");
            if (s_ShowMeniscus)
            {
                EditorGUILayout.PropertyField(m_MeniscusWidth);
                EditorGUILayout.PropertyField(m_MeniscusExponent);
                EditorGUILayout.PropertyField(m_MeniscusStrength);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawBevelSection()
        {
            s_ShowBevel = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowBevel, "📐 3D Directional Bevel & Chamfer");
            if (s_ShowBevel)
            {
                EditorGUILayout.PropertyField(m_LightAngle);
                EditorGUILayout.PropertyField(m_BevelWidth);
                EditorGUILayout.PropertyField(m_BevelHighlightIntensity);
                EditorGUILayout.PropertyField(m_BevelHighlightColor);
                EditorGUILayout.PropertyField(m_BevelShadowIntensity);
                EditorGUILayout.PropertyField(m_BevelShadowColor);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawRimSection()
        {
            s_ShowRim = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowRim, "✨ Rim Glow & Border Stroke");
            if (s_ShowRim)
            {
                EditorGUILayout.PropertyField(m_RimWidth);
                EditorGUILayout.PropertyField(m_RimIntensity);
                EditorGUILayout.PropertyField(m_RimColor);
                EditorGUILayout.PropertyField(m_BorderWidth);
                EditorGUILayout.PropertyField(m_BorderColor);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawSheenSection()
        {
            s_ShowSheen = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowSheen, "⚡ Specular Sheen & Glint");
            if (s_ShowSheen)
            {
                EditorGUILayout.PropertyField(m_SheenAngle);
                EditorGUILayout.PropertyField(m_SheenOffset);
                EditorGUILayout.PropertyField(m_SheenWidth);
                EditorGUILayout.PropertyField(m_SheenIntensity);
                EditorGUILayout.PropertyField(m_SheenColor);
                EditorGUILayout.PropertyField(m_AnimateSheen);
                if (m_AnimateSheen.boolValue)
                {
                    EditorGUILayout.PropertyField(m_SheenSpeed);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawURPFeatureBanner()
        {
            UniversalRendererData rendererData = GetActiveRendererData();
            if (rendererData != null)
            {
                bool hasFeature = false;
                foreach (var feature in rendererData.rendererFeatures)
                {
                    if (feature is ScreenBlurRendererFeature)
                    {
                        hasFeature = true;
                        break;
                    }
                }

                if (!hasFeature)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("⚠️ Screen Blur Feature Required", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("ScreenBlurRendererFeature is missing from the active URP Renderer. Blur will use fallback rendering.", EditorStyles.wordWrappedMiniLabel);
                    if (GUILayout.Button("Add Screen Blur Feature to Active URP Renderer", GUILayout.Height(24)))
                    {
                        AddFeatureToRenderer(rendererData);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(4);
                }
            }
        }

        private UniversalRendererData GetActiveRendererData()
        {
            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                urpAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            }

            if (urpAsset != null)
            {
                // Access m_RendererDataList via reflection
                var field = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    var list = field.GetValue(urpAsset) as ScriptableRendererData[];
                    if (list != null && list.Length > 0)
                    {
                        return list[0] as UniversalRendererData;
                    }
                }
            }

            // Fallback: search assets
            string[] guids = AssetDatabase.FindAssets("t:UniversalRendererData");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
            }

            return null;
        }

        private void AddFeatureToRenderer(UniversalRendererData rendererData)
        {
            var feature = ScriptableObject.CreateInstance<ScreenBlurRendererFeature>();
            feature.name = "Screen Blur (UGUI Liquid Glass)";

            AssetDatabase.AddObjectToAsset(feature, rendererData);

            // Add to renderer features list
            var prop = new SerializedObject(rendererData).FindProperty("m_RendererFeatures");
            if (prop != null)
            {
                prop.serializedObject.Update();
                int idx = prop.arraySize;
                prop.InsertArrayElementAtIndex(idx);
                prop.GetArrayElementAtIndex(idx).objectReferenceValue = feature;
                prop.serializedObject.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=#4CAF50><b>[Liquid Glass]</b></color> Successfully added ScreenBlurRendererFeature to " + rendererData.name, rendererData);
        }

        // Preset Configurations
        private void ApplyAppleGlassPreset()
        {
            m_TintColor.colorValue = new Color(1f, 1f, 1f, 0.22f);
            m_TintBlendMode.enumValueIndex = (int)TintBlendMode.AlphaBlend;
            m_BlurAmount.floatValue = 1.0f;
            m_Saturation.floatValue = 1.3f;
            m_Brightness.floatValue = 1.08f;
            m_RefractionStrength.floatValue = 0.015f;
            m_ChromaticAberration.floatValue = 0.25f;
            m_WaveStrength.floatValue = 0f;
            m_MeniscusWidth.floatValue = 16f;
            m_MeniscusStrength.floatValue = 0.035f;
            m_LightAngle.floatValue = 135f;
            m_BevelWidth.floatValue = 7f;
            m_BevelHighlightIntensity.floatValue = 1.2f;
            m_BevelShadowIntensity.floatValue = 0.35f;
            m_RimWidth.floatValue = 8f;
            m_RimIntensity.floatValue = 0.5f;
            m_SheenIntensity.floatValue = 0.35f;
            m_NoiseStrength.floatValue = 0.015f;
        }

        private void ApplyLiquidRipplePreset()
        {
            m_TintColor.colorValue = new Color(0.7f, 0.9f, 1f, 0.2f);
            m_TintBlendMode.enumValueIndex = (int)TintBlendMode.AlphaBlend;
            m_BlurAmount.floatValue = 0.9f;
            m_Saturation.floatValue = 1.4f;
            m_Brightness.floatValue = 1.05f;
            m_RefractionStrength.floatValue = 0.045f;
            m_ChromaticAberration.floatValue = 0.55f;
            m_WaveSpeed.floatValue = 2.0f;
            m_WaveFrequency.vector2Value = new Vector2(4f, 4f);
            m_WaveStrength.floatValue = 0.012f;
            m_MeniscusWidth.floatValue = 22f;
            m_MeniscusStrength.floatValue = 0.06f;
            m_BevelHighlightIntensity.floatValue = 1.5f;
            m_RimIntensity.floatValue = 0.8f;
            m_RimColor.colorValue = new Color(0.8f, 0.95f, 1f, 1f);
        }

        private void ApplyDarkAcrylicPreset()
        {
            m_TintColor.colorValue = new Color(0.08f, 0.08f, 0.12f, 0.55f);
            m_TintBlendMode.enumValueIndex = (int)TintBlendMode.AlphaBlend;
            m_BlurAmount.floatValue = 1.0f;
            m_Saturation.floatValue = 1.45f;
            m_Brightness.floatValue = 1.15f;
            m_RefractionStrength.floatValue = 0.02f;
            m_ChromaticAberration.floatValue = 0.2f;
            m_WaveStrength.floatValue = 0f;
            m_LightAngle.floatValue = 135f;
            m_BevelHighlightIntensity.floatValue = 0.9f;
            m_BevelHighlightColor.colorValue = new Color(0.8f, 0.85f, 1f, 1f);
            m_BevelShadowIntensity.floatValue = 0.6f;
            m_RimWidth.floatValue = 12f;
            m_RimIntensity.floatValue = 0.6f;
            m_RimColor.colorValue = new Color(0.6f, 0.7f, 0.9f, 0.8f);
            m_BorderWidth.floatValue = 1f;
            m_BorderColor.colorValue = new Color(1f, 1f, 1f, 0.15f);
            m_NoiseStrength.floatValue = 0.025f;
        }

        private void ApplyCyberNeonPreset()
        {
            m_TintColor.colorValue = new Color(0.1f, 0.7f, 1f, 0.25f);
            m_TintBlendMode.enumValueIndex = (int)TintBlendMode.Additive;
            m_BlurAmount.floatValue = 1.0f;
            m_Saturation.floatValue = 1.6f;
            m_Brightness.floatValue = 1.2f;
            m_RefractionStrength.floatValue = 0.035f;
            m_ChromaticAberration.floatValue = 0.6f;
            m_LightAngle.floatValue = 135f;
            m_BevelHighlightColor.colorValue = new Color(0f, 1f, 1f, 1f);
            m_BevelHighlightIntensity.floatValue = 2.0f;
            m_RimWidth.floatValue = 14f;
            m_RimIntensity.floatValue = 1.8f;
            m_RimColor.colorValue = new Color(0f, 0.9f, 1f, 1f);
            m_BorderWidth.floatValue = 2f;
            m_BorderColor.colorValue = new Color(0.2f, 0.8f, 1f, 0.8f);
            m_SheenIntensity.floatValue = 0.8f;
            m_SheenColor.colorValue = new Color(0.5f, 1f, 1f, 1f);
        }

        private void ApplyClearLensPreset()
        {
            m_TintColor.colorValue = new Color(1f, 1f, 1f, 0.1f);
            m_TintBlendMode.enumValueIndex = (int)TintBlendMode.AlphaBlend;
            m_BlurAmount.floatValue = 0.15f;
            m_Saturation.floatValue = 1.1f;
            m_Brightness.floatValue = 1.02f;
            m_RefractionStrength.floatValue = 0.05f;
            m_ChromaticAberration.floatValue = 0.7f;
            m_WaveStrength.floatValue = 0f;
            m_MeniscusWidth.floatValue = 25f;
            m_MeniscusStrength.floatValue = 0.07f;
            m_LightAngle.floatValue = 135f;
            m_BevelWidth.floatValue = 10f;
            m_BevelHighlightIntensity.floatValue = 1.6f;
            m_SheenIntensity.floatValue = 0.5f;
            m_NoiseStrength.floatValue = 0.005f;
        }
    }
}
#endif
