// © 2025 Nanodogs Studios. All rights reserved.

using Nanodogs.API.Nanoshake;
using UnityEngine;

namespace Nanodogs.API.Explosion
{
    public class ExplosionAPI
    {
        public static bool useCameraShake = true;

        // Method to create an explosion at a given position with specified settings
        /// <summary>
        /// creates an explosion at the specified position using the provided settings.
        /// </summary>
        /// <param name="position">where the explosion will be created in world space</param>
        /// <param name="settings">the provided settings, see ExplosionSettings for more info.</param>
        /// <returns></returns>
        public static GameObject Explosion(Vector3 position, ExplosionSettings settings)
        {
            // Instantiate explosion effect
            if (settings.explosionEffectPrefab != null)
            {
                GameObject explosionEffect = MonoBehaviour.Instantiate(settings.explosionEffectPrefab, position, Quaternion.Euler(new Vector3(-90, 0, 0)));
                explosionEffect.tag = "Explosion";
                MonoBehaviour.Destroy(explosionEffect, 4f); // Destroy effect after 2 seconds
            }
            if (settings.explosionSFX != null)
            {
                AudioSource.PlayClipAtPoint(settings.explosionSFX, position);
            }
            if (useCameraShake)
            {
                // Trigger camera shake effect
                Nanoshake.Nanoshake.Shake(false, null, 1, 1f, 8);
            }
            // Find all colliders in the explosion radius
            Collider[] colliders = Physics.OverlapSphere(position, settings.radius);
            foreach (Collider hit in colliders)
            {
                // Apply explosion force to rigidbodies
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(settings.force, position, settings.radius, settings.upwardsModifier, settings.forceMode);
                    rb.useGravity = settings.useGravity;
                }

                if(hit.CompareTag("Fracturer"))
                {
                    Fracture frac = hit.GetComponent<Fracture>();
                    frac.CauseFracture();
                }

                // Apply damage to objects with a Health component
                // add this!!!!
            }
            return null; // Return null or any relevant information if needed
        }

    }
}