// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;

namespace Nanodogs.UniversalScripts
{
    public class NanoMovementBase : MonoBehaviour
    {
        public float speed = 5f;
        public float jumpForce = 5f;


        public float rayDistance = 1.1f;
        public Rigidbody rb;

        protected virtual void Start()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.freezeRotation = true; // prevent tipping
        }

        public virtual bool IsGrounded()
        {
            // Simple ground check
            return Physics.Raycast(transform.position, Vector3.down, rayDistance);
        }

        private void OnDrawGizmosSelected()
        {
            // draw ray down
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayDistance);
            // draw sphere at where ray hits
            Gizmos.DrawWireSphere(transform.position + Vector3.down * rayDistance, 0.1f);

        }
    }
}