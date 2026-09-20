// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;

namespace Nanodogs.API.Explosion
{
    /// <summary>
    /// Defines the settings for an explosion, including radius, force, damage, and visual effects.
    /// </summary>
    [CreateAssetMenu(fileName = "New Explosion Settings", menuName = "Nanodogs ScriptableObjects/ExplosionAPI/Explosion Settings")]
    public class ExplosionSettings : ScriptableObject
    {
        public float radius = 5f;
        public ForceMode forceMode = ForceMode.Force;
        public float force = 700f;
        public float upwardsModifier = 1f;
        public bool useGravity = true;
        public float damage = 50f;
        public GameObject explosionEffectPrefab;
        public AudioClip explosionSFX;
    }
}