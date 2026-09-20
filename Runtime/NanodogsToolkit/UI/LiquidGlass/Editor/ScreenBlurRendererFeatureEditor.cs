#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NanodogsToolkit.UI.LiquidGlass.Editor
{
    [CustomEditor(typeof(ScreenBlurRendererFeature))]
    public class ScreenBlurRendererFeatureEditor : UnityEditor.Editor
    {
        private SerializedProperty m_Settings;
        private SerializedProperty m_Algorithm;
        private SerializedProperty m_Downsample;
        private SerializedProperty m_Iterations;
        private SerializedProperty m_Spread;
        private SerializedProperty m_RenderPassEvent;
        private SerializedProperty m_RenderInSceneView;
        private SerializedProperty m_Saturation;
        private SerializedProperty m_Brightness;

        private void OnEnable()
        {
            m_Settings = serializedObject.FindProperty("m_Settings");
            if (m_Settings != null)
            {
                m_Algorithm = m_Settings.FindPropertyRelative("algorithm");
                m_Downsample = m_Settings.FindPropertyRelative("downsample");
                m_Iterations = m_Settings.FindPropertyRelative("iterations");
                m_Spread = m_Settings.FindPropertyRelative("spread");
                m_RenderPassEvent = m_Settings.FindPropertyRelative("renderPassEvent");
                m_RenderInSceneView = m_Settings.FindPropertyRelative("renderInSceneView");
                m_Saturation = m_Settings.FindPropertyRelative("saturation");
                m_Brightness = m_Settings.FindPropertyRelative("brightness");
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🌫️ Screen Blur Feature (UGUI Liquid Glass)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Computes full-screen multi-scale pyramid blur for frosted glass UI elements.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // Presets
            EditorGUILayout.LabelField("Quick Blur Presets:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("VisionOS Glass", EditorStyles.miniButtonLeft))
            {
                m_Algorithm.enumValueIndex = (int)BlurAlgorithm.DualKawase;
                m_Downsample.enumValueIndex = (int)BlurDownsample.Half;
                m_Iterations.intValue = 4;
                m_Spread.floatValue = 2.2f;
                m_Saturation.floatValue = 1.25f;
                m_Brightness.floatValue = 1.05f;
            }
            if (GUILayout.Button("Deep Acrylic", EditorStyles.miniButtonMid))
            {
                m_Algorithm.enumValueIndex = (int)BlurAlgorithm.DualKawase;
                m_Downsample.enumValueIndex = (int)BlurDownsample.Quarter;
                m_Iterations.intValue = 5;
                m_Spread.floatValue = 3.0f;
                m_Saturation.floatValue = 1.35f;
                m_Brightness.floatValue = 1.1f;
            }
            if (GUILayout.Button("Ultra Haze", EditorStyles.miniButtonMid))
            {
                m_Algorithm.enumValueIndex = (int)BlurAlgorithm.DualKawase;
                m_Downsample.enumValueIndex = (int)BlurDownsample.Quarter;
                m_Iterations.intValue = 6;
                m_Spread.floatValue = 4.5f;
                m_Saturation.floatValue = 1.4f;
                m_Brightness.floatValue = 1.15f;
            }
            if (GUILayout.Button("Subtle Glaze", EditorStyles.miniButtonRight))
            {
                m_Algorithm.enumValueIndex = (int)BlurAlgorithm.DualKawase;
                m_Downsample.enumValueIndex = (int)BlurDownsample.Half;
                m_Iterations.intValue = 2;
                m_Spread.floatValue = 1.2f;
                m_Saturation.floatValue = 1.1f;
                m_Brightness.floatValue = 1.02f;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            // Blur Pipeline Properties
            EditorGUILayout.LabelField("Blur Pipeline Configuration", EditorStyles.boldLabel);
            if (m_Algorithm != null) EditorGUILayout.PropertyField(m_Algorithm);
            if (m_Downsample != null) EditorGUILayout.PropertyField(m_Downsample);
            if (m_Iterations != null) EditorGUILayout.PropertyField(m_Iterations);
            if (m_Spread != null) EditorGUILayout.PropertyField(m_Spread);
            if (m_RenderPassEvent != null) EditorGUILayout.PropertyField(m_RenderPassEvent);
            if (m_RenderInSceneView != null) EditorGUILayout.PropertyField(m_RenderInSceneView);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Global Vibrancy", EditorStyles.boldLabel);
            if (m_Saturation != null) EditorGUILayout.PropertyField(m_Saturation);
            if (m_Brightness != null) EditorGUILayout.PropertyField(m_Brightness);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
