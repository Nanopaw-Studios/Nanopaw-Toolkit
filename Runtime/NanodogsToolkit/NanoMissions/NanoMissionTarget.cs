// © 2025-2026 Nanodogs Studios. All rights reserved.

using UnityEngine;
using UnityEngine.Events;

namespace Nanodogs.Toolkit.Missions
{
    /// <summary>
    /// Attach to enemies, targets, or destructible objects for DefeatTargets objectives.
    /// Call Defeat() from combat/damage scripts or enable reportOnDestroy.
    /// </summary>
    public class NanoMissionTarget : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("The tag or identifier matching targetTag in the mission objective.")]
        public string targetTag = "Enemy";

        [Tooltip("Amount of progress contributed when defeated.")]
        public int amount = 1;

        [Tooltip("If true, automatically reports defeat when this GameObject is destroyed.")]
        public bool reportOnDestroy = true;

        [Header("Events")]
        public UnityEvent onDefeated;

        private bool _isDefeated = false;

        public void Defeat()
        {
            if (_isDefeated) return;
            _isDefeated = true;

            if (NanoMissionManager.Instance != null)
            {
                NanoMissionManager.Instance.ReportDefeat(targetTag, amount);
            }

            onDefeated?.Invoke();
        }

        private void OnDestroy()
        {
            if (reportOnDestroy && !_isDefeated && Application.isPlaying)
            {
                Defeat();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.25f, 0.25f, 0.6f);
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.8f);

#if UNITY_EDITOR
            UnityEditor.Handles.color = new Color(1f, 0.4f, 0.4f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.2f, $"[Target: {targetTag}]");
#endif
        }
    }
}
