// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;
using System.Collections;

namespace Nanodogs.API.Nanoshake
{
    /// <summary>
    /// Provides functionality to apply a smooth, customizable camera shake effect with adjustable roughness and intensity.
    /// </summary>
    public class Nanoshake : MonoBehaviour
    {
        private static Nanoshake instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// Applies a camera shake with smooth, natural movement.
        /// </summary>
        /// <param name="useCustomCamera">If true, uses the specified custom camera; otherwise, uses the main camera.</param>
        /// <param name="customCamera">The custom camera to shake (ignored if useCustomCamera is false).</param>
        /// <param name="duration">Duration of the shake in seconds.</param>
        /// <param name="magnitude">Overall intensity of the shake (0.1–1 is typical).</param>
        /// <param name="roughness">How rapidly the shake direction changes (lower = smoother, higher = more chaotic).</param>
        public static void Shake(bool useCustomCamera, Camera customCamera, float duration, float magnitude, float roughness = 2f)
        {
            if (instance == null)
            {
                GameObject shakeHost = new GameObject("NanoshakeHost");
                instance = shakeHost.AddComponent<Nanoshake>();
                DontDestroyOnLoad(shakeHost);
            }

            Camera targetCamera = (useCustomCamera && customCamera != null) ? customCamera : Camera.main;
            if (targetCamera == null)
            {
                Debug.LogWarning("[Nanoshake] No camera found to shake.");
                return;
            }

            instance.StopAllCoroutines();
            instance.StartCoroutine(instance.PerformShake(targetCamera, duration, magnitude, roughness));
        }

        private IEnumerator PerformShake(Camera camera, float duration, float magnitude, float roughness)
        {
            if (duration <= 0 || magnitude <= 0)
                yield break;

            Transform camTransform = camera.transform;
            Vector3 originalPosition = camTransform.localPosition;
            Quaternion originalRotation = camTransform.localRotation;

            float elapsed = 0f;

            // Use Perlin noise for smoother, organic shake
            float seedX = Random.value * 100f;
            float seedY = Random.value * 100f;
            float seedZ = Random.value * 100f;

            // Easing curve for fade-in/out
            AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / duration;

                float strength = magnitude * fadeCurve.Evaluate(normalizedTime);
                float noiseTime = Time.time * roughness;

                float offsetX = (Mathf.PerlinNoise(seedX, noiseTime) - 0.5f) * 2f;
                float offsetY = (Mathf.PerlinNoise(seedY, noiseTime) - 0.5f) * 2f;
                float offsetZ = (Mathf.PerlinNoise(seedZ, noiseTime) - 0.5f) * 2f * 0.3f;

                Vector3 offset = new Vector3(offsetX, offsetY, offsetZ) * strength;

                camTransform.localPosition = originalPosition + offset;

                yield return null;
            }

            // Smoothly restore to original position at the end of the shake
            float t = 0f;
            const float restoreTime = 0.1f;
            while (t < restoreTime)
            {
                t += Time.deltaTime;
                camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, new Vector3(0, 0.5f, 0), t / restoreTime);
                yield return null;
            }

            camTransform.localPosition = new Vector3(0, 0.5f, 0);
        }
    }
}
