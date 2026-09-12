using System;
using UnityEngine;
using UnityEngine.UI;
using Nobi.UiRoundedCorners;

namespace NanodogsToolkit.UI.LiquidGlass
{
    public enum CornerRoundingMode
    {
        None = 0,
        Uniform = 1,
        Independent = 2
    }

    public enum TintBlendMode
    {
        AlphaBlend = 0,
        Multiply = 1,
        Additive = 2
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("UI/Liquid Glass")]
    public class LiquidGlass : MonoBehaviour
    {
        // Vector2.right rotated clockwise by 45 degrees
        private static readonly Vector2 wNorm = new Vector2(0.7071068f, -0.7071068f);
        // Vector2.right rotated counter-clockwise by 45 degrees
        private static readonly Vector2 hNorm = new Vector2(0.7071068f, 0.7071068f);

        private static readonly int prop_MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int prop_CornerMode = Shader.PropertyToID("_CornerMode");
        private static readonly int prop_WidthHeightRadius = Shader.PropertyToID("_WidthHeightRadius");
        private static readonly int prop_halfSize = Shader.PropertyToID("_halfSize");
        private static readonly int prop_r = Shader.PropertyToID("_r");
        private static readonly int prop_rect2props = Shader.PropertyToID("_rect2props");

        private static readonly int prop_TintColor = Shader.PropertyToID("_TintColor");
        private static readonly int prop_TintBlendMode = Shader.PropertyToID("_TintBlendMode");
        private static readonly int prop_BlurAmount = Shader.PropertyToID("_BlurAmount");
        private static readonly int prop_Saturation = Shader.PropertyToID("_Saturation");
        private static readonly int prop_Brightness = Shader.PropertyToID("_Brightness");
        private static readonly int prop_NoiseScale = Shader.PropertyToID("_NoiseScale");
        private static readonly int prop_NoiseStrength = Shader.PropertyToID("_NoiseStrength");
        private static readonly int prop_NoiseAnimate = Shader.PropertyToID("_NoiseAnimate");

        private static readonly int prop_RefractionStrength = Shader.PropertyToID("_RefractionStrength");
        private static readonly int prop_ChromaticAberration = Shader.PropertyToID("_ChromaticAberration");
        private static readonly int prop_BumpMap = Shader.PropertyToID("_BumpMap");
        private static readonly int prop_BumpTiling = Shader.PropertyToID("_BumpTiling");
        private static readonly int prop_WaveSpeed = Shader.PropertyToID("_WaveSpeed");
        private static readonly int prop_WaveFreq = Shader.PropertyToID("_WaveFreq");
        private static readonly int prop_WaveStrength = Shader.PropertyToID("_WaveStrength");

        private static readonly int prop_MeniscusWidth = Shader.PropertyToID("_MeniscusWidth");
        private static readonly int prop_MeniscusExp = Shader.PropertyToID("_MeniscusExp");
        private static readonly int prop_MeniscusStrength = Shader.PropertyToID("_MeniscusStrength");

        private static readonly int prop_LightAngle = Shader.PropertyToID("_LightAngle");
        private static readonly int prop_BevelWidth = Shader.PropertyToID("_BevelWidth");
        private static readonly int prop_BevelHighlightIntensity = Shader.PropertyToID("_BevelHighlightIntensity");
        private static readonly int prop_BevelHighlightColor = Shader.PropertyToID("_BevelHighlightColor");
        private static readonly int prop_BevelShadowIntensity = Shader.PropertyToID("_BevelShadowIntensity");
        private static readonly int prop_BevelShadowColor = Shader.PropertyToID("_BevelShadowColor");

        private static readonly int prop_RimWidth = Shader.PropertyToID("_RimWidth");
        private static readonly int prop_RimIntensity = Shader.PropertyToID("_RimIntensity");
        private static readonly int prop_RimColor = Shader.PropertyToID("_RimColor");
        private static readonly int prop_BorderWidth = Shader.PropertyToID("_BorderWidth");
        private static readonly int prop_BorderColor = Shader.PropertyToID("_BorderColor");

        private static readonly int prop_SheenAngle = Shader.PropertyToID("_SheenAngle");
        private static readonly int prop_SheenOffset = Shader.PropertyToID("_SheenOffset");
        private static readonly int prop_SheenWidth = Shader.PropertyToID("_SheenWidth");
        private static readonly int prop_SheenIntensity = Shader.PropertyToID("_SheenIntensity");
        private static readonly int prop_SheenColor = Shader.PropertyToID("_SheenColor");
        private static readonly int prop_SheenAnimate = Shader.PropertyToID("_SheenAnimate");
        private static readonly int prop_SheenSpeed = Shader.PropertyToID("_SheenSpeed");
        private static readonly int prop_FlipScreenY = Shader.PropertyToID("_FlipScreenY");

        [Header("Corner Rounding (UI Rounded Corners Support)")]
        [SerializeField] private CornerRoundingMode m_CornerMode = CornerRoundingMode.Uniform;
        [SerializeField, Min(0f)] private float m_UniformRadius = 24f;
        [SerializeField] private Vector4 m_IndependentRadii = new Vector4(20f, 20f, 20f, 20f); // TL, TR, BR, BL

        [Header("Surface Tint")]
        [SerializeField] private Color m_TintColor = new Color(1f, 1f, 1f, 0.25f);
        [SerializeField] private TintBlendMode m_TintBlendMode = TintBlendMode.AlphaBlend;

        [Header("Blur & Vibrancy")]
        [SerializeField, Range(0f, 1f)] private float m_BlurAmount = 1.0f;
        [SerializeField, Range(0f, 3f)] private float m_Saturation = 1.25f;
        [SerializeField, Range(0f, 3f)] private float m_Brightness = 1.05f;
        [SerializeField, Min(1f)] private float m_NoiseScale = 180f;
        [SerializeField, Range(0f, 0.2f)] private float m_NoiseStrength = 0.02f;
        [SerializeField] private bool m_AnimateNoise = false;

        [Header("Liquid Refraction & Waves")]
        [SerializeField, Range(0f, 0.15f)] private float m_RefractionStrength = 0.025f;
        [SerializeField, Range(0f, 1f)] private float m_ChromaticAberration = 0.35f;
        [SerializeField] private Texture2D m_DistortionMap;
        [SerializeField] private Vector2 m_DistortionTiling = Vector2.one;
        [SerializeField] private float m_WaveSpeed = 1.2f;
        [SerializeField] private Vector2 m_WaveFrequency = new Vector2(3f, 3f);
        [SerializeField, Range(0f, 0.05f)] private float m_WaveStrength = 0.006f;

        [Header("Edge Meniscus Refraction")]
        [SerializeField, Range(0f, 60f)] private float m_MeniscusWidth = 18f;
        [SerializeField, Range(0.5f, 4f)] private float m_MeniscusExponent = 2.0f;
        [SerializeField, Range(0f, 0.1f)] private float m_MeniscusStrength = 0.04f;

        [Header("3D Directional Bevel")]
        [SerializeField, Range(0f, 360f)] private float m_LightAngle = 135f;
        [SerializeField, Range(0f, 40f)] private float m_BevelWidth = 8f;
        [SerializeField, Range(0f, 3f)] private float m_BevelHighlightIntensity = 1.3f;
        [SerializeField] private Color m_BevelHighlightColor = Color.white;
        [SerializeField, Range(0f, 2f)] private float m_BevelShadowIntensity = 0.4f;
        [SerializeField] private Color m_BevelShadowColor = Color.black;

        [Header("Rim Glow & Border")]
        [SerializeField, Range(0f, 40f)] private float m_RimWidth = 10f;
        [SerializeField, Range(0f, 3f)] private float m_RimIntensity = 0.7f;
        [SerializeField] private Color m_RimColor = Color.white;
        [SerializeField, Range(0f, 10f)] private float m_BorderWidth = 0f;
        [SerializeField] private Color m_BorderColor = new Color(1f, 1f, 1f, 0.6f);

        [Header("Specular Sheen")]
        [SerializeField, Range(0f, 360f)] private float m_SheenAngle = 45f;
        [SerializeField, Range(-1f, 1f)] private float m_SheenOffset = 0f;
        [SerializeField, Range(0.01f, 1f)] private float m_SheenWidth = 0.3f;
        [SerializeField, Range(0f, 2f)] private float m_SheenIntensity = 0.4f;
        [SerializeField] private Color m_SheenColor = Color.white;
        [SerializeField] private bool m_AnimateSheen = false;
        [SerializeField] private float m_SheenSpeed = 0.4f;

        [Header("Compatibility")]
        [SerializeField] private bool m_FlipScreenY = false;

        private Graphic m_Graphic;
        private RectTransform m_RectTransform;
        private Material m_Material;
        private ImageWithRoundedCorners m_CompanionRounded;
        private ImageWithIndependentRoundedCorners m_CompanionIndependent;

        // Public Properties for dynamic runtime scripting
        public CornerRoundingMode CornerMode { get => m_CornerMode; set { m_CornerMode = value; Refresh(); } }
        public float UniformRadius { get => m_UniformRadius; set { m_UniformRadius = Mathf.Max(0f, value); Refresh(); } }
        public Vector4 IndependentRadii { get => m_IndependentRadii; set { m_IndependentRadii = value; Refresh(); } }
        public Color TintColor { get => m_TintColor; set { m_TintColor = value; Refresh(); } }
        public float BlurAmount { get => m_BlurAmount; set { m_BlurAmount = Mathf.Clamp01(value); Refresh(); } }
        public float RefractionStrength { get => m_RefractionStrength; set { m_RefractionStrength = value; Refresh(); } }
        public float ChromaticAberration { get => m_ChromaticAberration; set { m_ChromaticAberration = value; Refresh(); } }
        public Material GlassMaterial => m_Material;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
            Refresh();
        }

        private void OnDisable()
        {
            CleanupMaterial();
        }

        private void OnDestroy()
        {
            CleanupMaterial();
        }

        private void OnValidate()
        {
            Initialize();
            Refresh();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (isActiveAndEnabled)
            {
                Refresh();
            }
        }

        public void Initialize()
        {
            if (m_Graphic == null)
                m_Graphic = GetComponent<Graphic>();

            if (m_RectTransform == null)
                m_RectTransform = GetComponent<RectTransform>();

            // Check if UI Rounded Corners components are present on this GameObject
            m_CompanionRounded = GetComponent<ImageWithRoundedCorners>();
            m_CompanionIndependent = GetComponent<ImageWithIndependentRoundedCorners>();

            // Determine appropriate shader based on attached components
            Shader targetShader = null;
            if (m_CompanionIndependent != null)
            {
                targetShader = Shader.Find("UI/RoundedCorners/IndependentRoundedCornersLiquidGlass");
                m_CornerMode = CornerRoundingMode.Independent;
            }
            else if (m_CompanionRounded != null)
            {
                targetShader = Shader.Find("UI/RoundedCorners/RoundedCornersLiquidGlass");
                m_CornerMode = CornerRoundingMode.Uniform;
            }
            else
            {
                targetShader = Shader.Find("UI/LiquidGlass/LiquidGlassUI");
            }

            if (targetShader == null)
            {
                targetShader = Shader.Find("UI/LiquidGlass/LiquidGlassUI");
            }

            if (m_Material == null || m_Material.shader != targetShader)
            {
                CleanupMaterial();
                if (targetShader != null)
                {
                    m_Material = new Material(targetShader)
                    {
                        name = $"{gameObject.name}_LiquidGlass_Mat",
                        hideFlags = HideFlags.DontSave
                    };
                }
            }

            if (m_Graphic != null && m_Material != null && m_Graphic.material != m_Material)
            {
                m_Graphic.material = m_Material;
            }
        }

        public void Refresh()
        {
            if (m_Material == null)
            {
                Initialize();
                if (m_Material == null) return;
            }

            if (m_RectTransform == null)
                m_RectTransform = GetComponent<RectTransform>();

            Rect rect = m_RectTransform != null ? m_RectTransform.rect : Rect.zero;
            Vector2 size = rect.size;

            // Sync with UI Rounded Corners components if present
            if (m_CompanionRounded != null)
            {
                m_UniformRadius = m_CompanionRounded.radius;
            }
            else if (m_CompanionIndependent != null)
            {
                m_IndependentRadii = m_CompanionIndependent.r;
            }

            // 1. Corner calculations
            m_Material.SetFloat(prop_CornerMode, (float)m_CornerMode);
            m_Material.SetVector(prop_WidthHeightRadius, new Vector4(size.x, size.y, m_UniformRadius, 0f));

            Vector4 independentProps = RecalculateIndependentProps(size, m_IndependentRadii);
            m_Material.SetVector(prop_rect2props, independentProps);
            m_Material.SetVector(prop_halfSize, size * 0.5f);
            m_Material.SetVector(prop_r, m_IndependentRadii);

            // 2. Tint & Vibrancy
            m_Material.SetColor(prop_TintColor, m_TintColor);
            m_Material.SetFloat(prop_TintBlendMode, (float)m_TintBlendMode);
            m_Material.SetFloat(prop_BlurAmount, m_BlurAmount);
            m_Material.SetFloat(prop_Saturation, m_Saturation);
            m_Material.SetFloat(prop_Brightness, m_Brightness);
            m_Material.SetFloat(prop_NoiseScale, m_NoiseScale);
            m_Material.SetFloat(prop_NoiseStrength, m_NoiseStrength);
            m_Material.SetFloat(prop_NoiseAnimate, m_AnimateNoise ? 1f : 0f);

            // 3. Liquid Refraction
            m_Material.SetFloat(prop_RefractionStrength, m_RefractionStrength);
            m_Material.SetFloat(prop_ChromaticAberration, m_ChromaticAberration);
            if (m_DistortionMap != null)
                m_Material.SetTexture(prop_BumpMap, m_DistortionMap);
            m_Material.SetVector(prop_BumpTiling, new Vector4(m_DistortionTiling.x, m_DistortionTiling.y, 0f, 0f));
            m_Material.SetFloat(prop_WaveSpeed, m_WaveSpeed);
            m_Material.SetVector(prop_WaveFreq, m_WaveFrequency);
            m_Material.SetFloat(prop_WaveStrength, m_WaveStrength);

            // 4. Edge Meniscus Refraction
            m_Material.SetFloat(prop_MeniscusWidth, m_MeniscusWidth);
            m_Material.SetFloat(prop_MeniscusExp, m_MeniscusExponent);
            m_Material.SetFloat(prop_MeniscusStrength, m_MeniscusStrength);

            // 5. 3D Directional Bevel
            m_Material.SetFloat(prop_LightAngle, m_LightAngle);
            m_Material.SetFloat(prop_BevelWidth, m_BevelWidth);
            m_Material.SetFloat(prop_BevelHighlightIntensity, m_BevelHighlightIntensity);
            m_Material.SetColor(prop_BevelHighlightColor, m_BevelHighlightColor);
            m_Material.SetFloat(prop_BevelShadowIntensity, m_BevelShadowIntensity);
            m_Material.SetColor(prop_BevelShadowColor, m_BevelShadowColor);

            // 6. Rim Glow & Border
            m_Material.SetFloat(prop_RimWidth, m_RimWidth);
            m_Material.SetFloat(prop_RimIntensity, m_RimIntensity);
            m_Material.SetColor(prop_RimColor, m_RimColor);
            m_Material.SetFloat(prop_BorderWidth, m_BorderWidth);
            m_Material.SetColor(prop_BorderColor, m_BorderColor);

            // 7. Specular Sheen
            m_Material.SetFloat(prop_SheenAngle, m_SheenAngle);
            m_Material.SetFloat(prop_SheenOffset, m_SheenOffset);
            m_Material.SetFloat(prop_SheenWidth, m_SheenWidth);
            m_Material.SetFloat(prop_SheenIntensity, m_SheenIntensity);
            m_Material.SetColor(prop_SheenColor, m_SheenColor);
            m_Material.SetFloat(prop_SheenAnimate, m_AnimateSheen ? 1f : 0f);
            m_Material.SetFloat(prop_SheenSpeed, m_SheenSpeed);

            // 8. Compatibility
            m_Material.SetFloat(prop_FlipScreenY, m_FlipScreenY ? 1f : 0f);
        }

        private Vector4 RecalculateIndependentProps(Vector2 size, Vector4 r)
        {
            var aVec = new Vector2(size.x, -size.y + r.x + r.z);
            var halfWidth = Vector2.Dot(aVec, wNorm) * 0.5f;

            var bVec = new Vector2(size.x, size.y - r.w - r.y);
            var halfHeight = Vector2.Dot(bVec, hNorm) * 0.5f;

            var efVec = new Vector2(size.x - r.x - r.y, 0f);
            var egVec = hNorm * Vector2.Dot(efVec, hNorm);

            var ePoint = new Vector2(r.x - (size.x * 0.5f), size.y * 0.5f);
            var origin = ePoint + egVec + wNorm * halfWidth + hNorm * -halfHeight;

            return new Vector4(origin.x, origin.y, halfWidth, halfHeight);
        }

        private void CleanupMaterial()
        {
            if (m_Material != null)
            {
                if (Application.isPlaying)
                    Destroy(m_Material);
                else
                    DestroyImmediate(m_Material);

                m_Material = null;
            }

            if (m_Graphic != null && m_Graphic.material == m_Material)
            {
                m_Graphic.material = null;
            }
        }
    }
}
