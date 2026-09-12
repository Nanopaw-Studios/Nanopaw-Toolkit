#ifndef LIQUID_GLASS_COMMON_INCLUDED
#define LIQUID_GLASS_COMMON_INCLUDED

#include "UnityCG.cginc"
#include "UnityUI.cginc"

// =========================================================================
// 1. SIGNED DISTANCE FIELD (SDF) FUNCTIONS FOR ROUNDED CORNERS
// Compatible with UI Rounded Corners package
// =========================================================================

inline float2 lg_translate(float2 samplePosition, float2 offset) {
    return samplePosition - offset;
}

inline float2 lg_rotate(float2 samplePosition, float rotation) {
    const float PI = 3.141592653589793;
    float angle = rotation * PI * 2.0 * -1.0;
    float sine, cosine;
    sincos(angle, sine, cosine);
    return float2(cosine * samplePosition.x + sine * samplePosition.y,
                  cosine * samplePosition.y - sine * samplePosition.x);
}

inline float lg_rectangle(float2 samplePosition, float2 halfSize) {
    float2 distanceToEdge = abs(samplePosition) - halfSize;
    float outsideDistance = length(max(distanceToEdge, 0.0));
    float insideDistance = min(max(distanceToEdge.x, distanceToEdge.y), 0.0);
    return outsideDistance + insideDistance;
}

inline float lg_roundedRectangle(float2 samplePosition, float absoluteRound, float2 halfSize) {
    return lg_rectangle(samplePosition, halfSize - absoluteRound) - absoluteRound;
}

inline float lg_circle(float2 position, float radius) {
    return length(position) - radius;
}

// Antialiased cutoff function matching UI Rounded Corners SDFUtils.cginc
inline float LGAntialiasedCutoff(float distance) {
    float distanceChange = fwidth(distance) * 0.5;
    distanceChange = max(distanceChange, 0.0001);
    return smoothstep(distanceChange, -distanceChange, distance);
}

// Compute raw signed distance for uniform rounded corners
inline float LGCalcDistanceUniform(float2 uv, float2 size, float radius) {
    float2 samplePosition = (uv - 0.5) * size;
    return lg_roundedRectangle(samplePosition, radius * 0.5, size * 0.5);
}

// Compute raw signed distance for independent 4-corner radiuses
// r = (top-left, top-right, bottom-right, bottom-left)
inline float LGCalcDistanceIndependent(float2 uv, float2 halfSize, float4 rect2props, float4 r) {
    float2 samplePosition = (uv - 0.5) * halfSize * 2.0;

    float r1 = lg_rectangle(samplePosition, halfSize);

    float2 r2Position = lg_rotate(lg_translate(samplePosition, rect2props.xy), 0.125);
    float r2 = lg_rectangle(r2Position, rect2props.zw);

    float2 circle0Position = lg_translate(samplePosition, float2(-halfSize.x + r.x, halfSize.y - r.x));
    float c0 = lg_circle(circle0Position, r.x);

    float2 circle1Position = lg_translate(samplePosition, float2(halfSize.x - r.y, halfSize.y - r.y));
    float c1 = lg_circle(circle1Position, r.y);

    float2 circle2Position = lg_translate(samplePosition, float2(halfSize.x - r.z, -halfSize.y + r.z));
    float c2 = lg_circle(circle2Position, r.z);

    float2 circle3Position = lg_translate(samplePosition, -halfSize + r.w);
    float c3 = lg_circle(circle3Position, r.w);

    return max(r1, min(min(min(min(r2, c0), c1), c2), c3));
}

// Calculate 2D normal vector on the boundary of the SDF
inline float2 LGGetSDFNormal(float dist, float2 uv, float2 size) {
    float2 grad = float2(ddx(dist), ddy(dist));
    float len = length(grad);
    if (len > 1e-5) {
        return grad / len;
    }
    float2 toCenter = (uv - 0.5) * size;
    float toCenterLen = length(toCenter);
    return toCenterLen > 1e-5 ? (toCenter / toCenterLen) : float2(0.0, 1.0);
}

// =========================================================================
// 2. SCREEN SPACE COORDINATES
// Works reliably across Screen Space Overlay, Camera, and World Space
// =========================================================================

inline float2 LGGetScreenUV(float4 screenPos, float flipY) {
    float2 screenUV = screenPos.xy / max(screenPos.w, 1e-5);
    #if UNITY_UV_STARTS_AT_TOP
    if (_ProjectionParams.x < 0.0) {
        screenUV.y = 1.0 - screenUV.y;
    }
    #endif
    if (flipY > 0.5) {
        screenUV.y = 1.0 - screenUV.y;
    }
    return screenUV;
}

// =========================================================================
// 3. LIQUID GLASS OPTICAL EFFECTS & DISPERSION
// =========================================================================

// Procedural wave perturbation for living liquid surface
inline float2 LGEvaluateLiquidWaves(float2 uv, float speed, float2 freq, float strength) {
    float t = _Time.y * speed;
    float w1 = sin((uv.x * freq.x + uv.y * freq.y) * 6.2831853 + t);
    float w2 = cos((-uv.x * freq.y + uv.y * freq.x) * 6.2831853 + t * 0.85);
    return float2(w1, w2) * strength;
}

// Edge meniscus curvature refraction (surface-tension along rounded boundaries)
inline float2 LGGetMeniscusDisplacement(float dist, float2 normal, float edgeWidth, float exponent, float strength) {
    float curveFactor = smoothstep(-max(edgeWidth, 0.001), 0.0, dist);
    return normal * pow(curveFactor, max(exponent, 0.1)) * strength;
}

// Sample texture with chromatic dispersion (RGB channel splitting)
inline half3 LGSampleChromaticAberration(sampler2D blurTex, float2 baseUV, float2 displacement, float dispersion) {
    float2 dispR = displacement * (1.0 + dispersion);
    float2 dispG = displacement;
    float2 dispB = displacement * (1.0 - dispersion);

    float2 uvR = clamp(baseUV + dispR, 0.001, 0.999);
    float2 uvG = clamp(baseUV + dispG, 0.001, 0.999);
    float2 uvB = clamp(baseUV + dispB, 0.001, 0.999);

    half r = tex2D(blurTex, uvR).r;
    half g = tex2D(blurTex, uvG).g;
    half b = tex2D(blurTex, uvB).b;

    return half3(r, g, b);
}

// Saturation & exposure adjustments (Vibrancy)
inline half3 LGAdjustVibrancy(half3 col, half saturation, half brightness) {
    half luma = dot(col, half3(0.2126729, 0.7151522, 0.0721750));
    col = lerp(half3(luma, luma, luma), col, saturation);
    return col * brightness;
}

// =========================================================================
// 4. LIGHTING: BEVEL, RIM, SHEEN & FROSTED GRAIN
// =========================================================================

// 3D Directional Bevel & Rim Light calculation based on SDF
inline void LGComputeBevelAndRim(
    float dist, float2 normal, float lightAngleDeg, float bevelWidth,
    half bevelHighlightIntensity, half bevelShadowIntensity,
    half3 highlightColor, half3 shadowColor,
    float rimWidth, half rimIntensity, half3 rimColor,
    out half3 bevelLight, out half3 rimLight)
{
    float angleRad = radians(lightAngleDeg);
    float2 lightDir = float2(cos(angleRad), sin(angleRad));

    float bevelFactor = smoothstep(-max(bevelWidth, 0.001), 0.0, dist);
    float lightDot = dot(normal, lightDir);

    half3 highlight = highlightColor * (max(0.0, lightDot) * bevelFactor * bevelHighlightIntensity);
    half3 shadow = shadowColor * (max(0.0, -lightDot) * bevelFactor * bevelShadowIntensity);
    bevelLight = highlight - shadow;

    float rimFactor = smoothstep(-max(rimWidth, 0.001), 0.0, dist) * (1.0 - smoothstep(-0.5, 0.0, dist));
    rimLight = rimColor * (rimFactor * rimIntensity);
}

// Specular sweeping sheen
inline half3 LGComputeSpecularSheen(
    float2 uv, float angleDeg, float offset, float width,
    half intensity, half3 sheenColor, float animSpeed, float animate)
{
    float angleRad = radians(angleDeg);
    float2 dir = float2(cos(angleRad), sin(angleRad));
    float pos = dot(uv - 0.5, dir);
    float currentOffset = offset + (animate > 0.5 ? (frac(_Time.y * animSpeed) * 2.0 - 1.0) : 0.0);
    float sheen = exp(-pow((pos - currentOffset) / max(width, 0.001), 2.0));
    return sheenColor * (sheen * intensity);
}

// High-frequency frosted noise dither
inline float LGComputeFrostedNoise(float2 uv, float scale, float animate) {
    float2 noiseUV = uv * scale + (animate > 0.5 ? frac(_Time.y * 12.345) : 0.0);
    return frac(sin(dot(noiseUV, float2(12.9898, 78.233))) * 43758.5453);
}

// Inner/outer sharp border stroke
inline half4 LGComputeBorderStroke(float dist, float borderWidth, half4 borderColor) {
    float border = smoothstep(-borderWidth - 0.5, -borderWidth + 0.5, dist) - smoothstep(-0.5, 0.5, dist);
    return borderColor * saturate(border);
}

#endif // LIQUID_GLASS_COMMON_INCLUDED
