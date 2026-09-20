// © 2025 Nanodogs Studios. All rights reserved.
using UnityEngine;
using System.Collections;

namespace Nanodogs.API.Nanoshake
{
    /// <summary>
    /// Provides functionality to apply a smooth, customizable camera shake effect with adjustable roughness and intensity.
    /// Supports seamless interrupt-and-restart without any positional snapping.
    /// </summary>
    public class Nanoshake : MonoBehaviour
    {
        private static Nanoshake instance;

        // Tracks the offset currently live on the camera so mid-shake interrupts are seamless.
        // Delta-based application means the camera can move freely while shaking.
        private Vector3 livePositionOffset = Vector3.zero;
        private Quaternion liveRotationOffset = Quaternion.identity;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// Applies a camera shake with smooth, natural movement.
        /// Safe to call mid-shake — the new shake blends from the current offset with no snap.
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

            // Stop the previous coroutine — livePositionOffset and liveRotationOffset are preserved,
            // so the new shake picks up from exactly where the camera currently sits.
            if (instance.shakeCoroutine != null)
                instance.StopCoroutine(instance.shakeCoroutine);

            instance.shakeCoroutine = instance.StartCoroutine(
                instance.PerformShake(targetCamera, duration, magnitude, roughness));
        }

        private IEnumerator PerformShake(Camera camera, float duration, float magnitude, float roughness)
        {
            if (duration <= 0f || magnitude <= 0f)
                yield break;

            Transform camTransform = camera.transform;
            float elapsed = 0f;

            // Independent seeds per axis so they don't move in lockstep.
            float seedX    = Random.value * 100f;
            float seedY    = Random.value * 100f;
            float seedZ    = Random.value * 100f;
            float seedRoll = Random.value * 100f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / duration);

                // Sine envelope: rises from 0, peaks at mid-duration, returns to 0.
                // No snap at either end — magnitude is always 0 when the offset changes start/stop.
                float envelope = Mathf.Sin(normalizedTime * Mathf.PI);
                float strength = magnitude * envelope;

                float noiseTime = Time.time * roughness;

                float offsetX = (Mathf.PerlinNoise(seedX,    noiseTime) - 0.5f) * 2f;
                float offsetY = (Mathf.PerlinNoise(seedY,    noiseTime) - 0.5f) * 2f;
                float offsetZ = (Mathf.PerlinNoise(seedZ,    noiseTime) - 0.5f) * 2f * 0.25f;
                float roll    = (Mathf.PerlinNoise(seedRoll, noiseTime) - 0.5f) * 2f * 1.5f;

                Vector3    newPositionOffset = new Vector3(offsetX, offsetY, offsetZ) * strength;
                Quaternion newRotationOffset = Quaternion.Euler(0f, 0f, roll * strength);

                // Delta application: only move by the *change* in offset each frame.
                // The camera is free to move independently — no originalPosition to go stale.
                camTransform.localPosition += newPositionOffset - livePositionOffset;
                camTransform.localRotation  = Quaternion.Inverse(liveRotationOffset)
                                              * camTransform.localRotation
                                              * newRotationOffset;

                livePositionOffset = newPositionOffset;
                liveRotationOffset = newRotationOffset;

                yield return null;
            }

            // At this point the sine envelope has already brought the offset very close to zero.
            // This short restore phase just cleans up any floating-point residual without any
            // perceptible movement — it's cosmetic safety, not correction of a large offset.
            Vector3    startPosOffset = livePositionOffset;
            Quaternion startRotOffset = liveRotationOffset;
            float t = 0f;
            const float restoreDuration = 0.08f;

            while (t < restoreDuration)
            {
                t += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, t / restoreDuration);

                Vector3    newPositionOffset = Vector3.Lerp(startPosOffset, Vector3.zero, progress);
                Quaternion newRotationOffset = Quaternion.Slerp(startRotOffset, Quaternion.identity, progress);

                camTransform.localPosition += newPositionOffset - livePositionOffset;
                camTransform.localRotation  = Quaternion.Inverse(liveRotationOffset)
                                              * camTransform.localRotation
                                              * newRotationOffset;

                livePositionOffset = newPositionOffset;
                liveRotationOffset = newRotationOffset;

                yield return null;
            }

            // Hard reset to guarantee zero residual — the delta is negligible at this point.
            camTransform.localPosition -= livePositionOffset;
            camTransform.localRotation  = Quaternion.Inverse(liveRotationOffset) * camTransform.localRotation;

            livePositionOffset = Vector3.zero;
            liveRotationOffset = Quaternion.identity;
            shakeCoroutine     = null;
        }
    }
}
