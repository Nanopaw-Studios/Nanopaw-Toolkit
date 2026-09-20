using UnityEngine;

namespace Nanodogs.UniversalScripts
{
    public class LadderBehaviour : NanoTrigger
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var rb = other.attachedRigidbody;
                var movement = other.GetComponent<FirstPersonPlayerMovement>();

                rb.useGravity = false;
                movement.onLadder = true;
                movement.SetLadderData(transform.forward, GetLadderCenter(other.transform.position));

                // zero out velocity for instant stick
                rb.linearVelocity = Vector3.zero;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var rb = other.attachedRigidbody;
                var movement = other.GetComponent<FirstPersonPlayerMovement>();

                rb.useGravity = true;
                movement.onLadder = false;
            }
        }

        private Vector3 GetLadderCenter(Vector3 playerPosition)
        {
            // project player's position onto ladder plane to get the center
            Vector3 ladderPos = transform.position;
            Vector3 ladderRight = transform.right;
            Vector3 offset = playerPosition - ladderPos;
            float distance = Vector3.Dot(offset, ladderRight);
            return ladderPos + ladderRight * distance * 0f; // always snap to ladder center
        }
    }
}
