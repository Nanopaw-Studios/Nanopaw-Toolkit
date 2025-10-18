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

        private void Update()
        {
            CheckForInteraction();
        }

        private void CheckForInteraction()
        {
            // Create a ray from the center of the screen
            lastRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            hasHit = Physics.Raycast(lastRay, out lastHit, interactDistance);

            if (hasHit)
            {
                NanoInteractable interactable = lastHit.collider.GetComponent<NanoInteractable>();
                if (interactable != null && interactable.isCurrentlyInteractable)
                {
                    interactable.hovering = true;

                    if (interactKey != null && interactKey.action.WasPerformedThisFrame())
                    {
                        interactable.Interact();
                    }
                }
            }

            // Debug ray (visible in Game if Gizmos are on)
            Debug.DrawRay(lastRay.origin, lastRay.direction * interactDistance, hasHit ? Color.green : Color.red);
        }

        private void OnDrawGizmos()
        {
            if (cam == null) return;

            Gizmos.color = hasHit ? Color.green : Color.red;
            Gizmos.DrawLine(lastRay.origin, lastRay.origin + lastRay.direction * interactDistance);

            if (hasHit)
            {
                Gizmos.DrawSphere(lastHit.point, 0.05f);
            }
        }
    }
}
