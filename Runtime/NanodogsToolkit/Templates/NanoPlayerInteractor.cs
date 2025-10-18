using UnityEngine;
using UnityEngine.InputSystem;

namespace Nanodogs.UniversalScripts
{
    public class NanoPlayerInteractor : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("Key used to interact with objects.")]
        public InputActionReference interactKey;

        [Tooltip("Maximum distance to interactable objects.")]
        public float interactDistance = 3f;

        [Tooltip("Camera used for raycasting.")]
        public Camera cam;

        private Ray lastRay;
        private bool hasHit;
        private RaycastHit lastHit;

        // Keep track of what we’re hovering over
        private NanoInteractable currentHovered;

        private void Update()
        {
            CheckForInteraction();
        }

        private void CheckForInteraction()
        {
            lastRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            hasHit = Physics.Raycast(lastRay, out lastHit, interactDistance);

            if (hasHit)
            {
                NanoInteractable interactable = lastHit.collider.GetComponent<NanoInteractable>();

                if (interactable != null && interactable.isCurrentlyInteractable)
                {
                    // If we were hovering something else before, stop hovering it
                    if (currentHovered != null && currentHovered != interactable)
                        currentHovered.hovering = false;

                    // Start hovering this one
                    currentHovered = interactable;
                    currentHovered.hovering = true;

                    if (interactKey != null && interactKey.action.WasPerformedThisFrame())
                    {
                        currentHovered.Interact();
                    }
                }
                else
                {
                    // We hit something that's not interactable — stop hovering the old one
                    if (currentHovered != null)
                    {
                        currentHovered.hovering = false;
                        currentHovered = null;
                    }
                }
            }
            else
            {
                // No hit at all — stop hovering if needed
                if (currentHovered != null)
                {
                    currentHovered.hovering = false;
                    currentHovered = null;
                }
            }

            Debug.DrawRay(lastRay.origin, lastRay.direction * interactDistance, hasHit ? Color.green : Color.red);
        }

        private void OnDrawGizmos()
        {
            if (cam == null) return;

            Gizmos.color = hasHit ? Color.green : Color.red;
            Gizmos.DrawLine(lastRay.origin, lastRay.origin + lastRay.direction * interactDistance);

            if (hasHit)
                Gizmos.DrawSphere(lastHit.point, 0.05f);
        }
    }
}
