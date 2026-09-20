// © 2025-2026 Nanodogs Studios. All rights reserved.

using UnityEngine;
using UnityEngine.Events;

namespace Nanodogs.Toolkit.Missions
{
    /// <summary>
    /// Place on a GameObject with a Trigger Collider to complete TriggerZone objectives.
    /// Draws gizmos in the Scene View for easy visual placement.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class NanoMissionTriggerZone : MonoBehaviour
    {
        [Header("Zone Configuration")]
        [Tooltip("The ID matching the targetZoneId of an objective.")]
        public string zoneId = "Zone_A";

        [Tooltip("If true, only activates once and disables or destroys the zone.")]
        public bool triggerOnce = true;

        [Tooltip("If true, destroys the GameObject after triggering. If false, disables the component or collider.")]
        public bool destroyOnTrigger = false;

        [Tooltip("Only trigger if the entering object has the tag 'Player' or matches MissionManager.playerTransform.")]
        public bool requirePlayer = true;

        [Tooltip("Filter by specific collision layers.")]
        public LayerMask allowedLayers = ~0;

        [Header("Events")]
        public UnityEvent onZoneEntered;

        private bool _hasTriggered = false;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasTriggered && triggerOnce) return;

            // Check layer
            if (((1 << other.gameObject.layer) & allowedLayers) == 0) return;

            // Check player filter
            if (requirePlayer)
            {
                bool isPlayer = other.CompareTag("Player");
                if (!isPlayer && NanoMissionManager.Instance != null && NanoMissionManager.Instance.playerTransform != null)
                {
                    isPlayer = other.transform == NanoMissionManager.Instance.playerTransform ||
                               other.transform.IsChildOf(NanoMissionManager.Instance.playerTransform);
                }

                if (!isPlayer) return;
            }

            _hasTriggered = true;

            if (NanoMissionManager.Instance != null)
            {
                NanoMissionManager.Instance.ReportTriggerEnter(zoneId, other.gameObject);
            }

            onZoneEntered?.Invoke();

            if (triggerOnce)
            {
                if (destroyOnTrigger)
                {
                    Destroy(gameObject);
                }
                else
                {
                    var col = GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                    enabled = false;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1.0f, 0.4f);
            var col = GetComponent<Collider>();
            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
            }

#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"[TriggerZone: {zoneId}]");
#endif
        }
    }
}
