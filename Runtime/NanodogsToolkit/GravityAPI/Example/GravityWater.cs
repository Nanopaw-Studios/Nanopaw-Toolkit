using UnityEngine;

namespace Nanodogs.API.Gravity.Example
{
    [RequireComponent(typeof(Collider))]
    public class GravityWater : MonoBehaviour
    {
        [Header("Gravity Settings")]
        public float gravityScaleInWater = 0.4f; // weaker gravity underwater

        [Header("Buoyancy Settings")]
        public float baseBuoyancy = 9.81f;       // buoyant acceleration for 1 kg
        public float damping = 0.5f;             // smooths motion

        [Header("Drag Settings")]
        public float waterDrag = 3f;             // slows underwater movement
        public float airDrag = 0.05f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody && other.CompareTag("Player"))
            {
                var rb = other.attachedRigidbody;

                // Reduce overall gravity influence (optional global effect)
                GravityAPI.ChangeYGravity(-9.81f * gravityScaleInWater);

                // Add drag to simulate water resistance
                rb.linearDamping = waterDrag;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.attachedRigidbody && other.CompareTag("Player"))
            {
                var rb = other.attachedRigidbody;

                // Calculate buoyant acceleration based on mass
                float buoyancyAccel = baseBuoyancy / rb.mass; // lighter = stronger buoyancy per mass
                float gravityAccel = 9.81f * gravityScaleInWater;

                // Net upward force (positive = float, negative = sink)
                float netAccel = buoyancyAccel - gravityAccel;

                // Apply damping so it doesn't oscillate
                float upwardForce = (netAccel - rb.linearVelocity.y * damping);

                rb.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody && other.CompareTag("Player"))
            {
                var rb = other.attachedRigidbody;

                // Restore normal gravity and drag
                GravityAPI.ChangeYGravity(-9.81f);
                rb.linearDamping = airDrag;
            }
        }
    }
}
