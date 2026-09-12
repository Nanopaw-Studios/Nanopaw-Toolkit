// © 2025-2026 Nanodogs Studios. All rights reserved.

using UnityEngine;
using UnityEngine.Events;

namespace Nanodogs.Toolkit.Missions
{
    /// <summary>
    /// Attach to items that can be collected for CollectItems objectives.
    /// Handles player pickup on trigger enter or manual Collect() call.
    /// </summary>
    public class NanoMissionCollectable : MonoBehaviour
    {
        [Header("Collectable Settings")]
        [Tooltip("The tag or identifier matching targetTag in the mission objective.")]
        public string itemTag = "Coin";

        [Tooltip("Amount of progress contributed when picked up.")]
        public int amount = 1;

        [Tooltip("If true, picks up automatically when a player enters the trigger collider.")]
        public bool collectOnTrigger = true;

        [Tooltip("If true, destroys the GameObject after pickup.")]
        public bool destroyOnCollect = true;

        [Header("Visual & Audio")]
        [Tooltip("Optional rotation animation in place.")]
        public bool idleRotation = true;
        public Vector3 rotationAxis = Vector3.up;
        public float rotationSpeed = 90f;

        [Tooltip("Optional sound effect played upon collection.")]
        public AudioClip pickupAudio;

        [Tooltip("Optional prefab instantiated upon collection.")]
        public GameObject pickupVFX;

        [Header("Events")]
        public UnityEvent onCollected;

        private bool _isCollected = false;

        private void Update()
        {
            if (idleRotation)
            {
                transform.Rotate(rotationAxis * (rotationSpeed * Time.deltaTime), Space.Self);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!collectOnTrigger || _isCollected) return;

            bool isPlayer = other.CompareTag("Player");
            if (!isPlayer && NanoMissionManager.Instance != null && NanoMissionManager.Instance.playerTransform != null)
            {
                isPlayer = other.transform == NanoMissionManager.Instance.playerTransform ||
                           other.transform.IsChildOf(NanoMissionManager.Instance.playerTransform);
            }

            if (isPlayer)
            {
                Collect();
            }
        }

        public void Collect()
        {
            if (_isCollected) return;
            _isCollected = true;

            if (NanoMissionManager.Instance != null)
            {
                NanoMissionManager.Instance.ReportCollectable(itemTag, amount);
            }

            if (pickupAudio != null)
            {
                AudioSource.PlayClipAtPoint(pickupAudio, transform.position);
            }

            if (pickupVFX != null)
            {
                Instantiate(pickupVFX, transform.position, Quaternion.identity);
            }

            onCollected?.Invoke();

            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, 0.5f);

#if UNITY_EDITOR
            UnityEditor.Handles.color = new Color(1f, 0.9f, 0.3f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, $"[Collect: {itemTag}]");
#endif
        }
    }
}
