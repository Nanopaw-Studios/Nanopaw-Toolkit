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

        [Tooltip("Camera used for raycasting. If unassigned, automatically finds main camera.")]
        public Camera cam;

        [Tooltip("Layer mask for interactable objects. Defaults to Everything.")]
        public LayerMask interactableLayerMask = ~0;

        [Tooltip("Whether the raycast should detect trigger colliders.")]
        public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        private Ray lastRay;
        private bool hasHit;
        private RaycastHit lastHit;

        // Keep track of what we're hovering over
        private NanoInteractable currentHovered;

        protected void OnEnable()
        {
            if (interactKey != null && interactKey.action != null)
            {
                interactKey.action.Enable();
            }
        }

        protected void OnDisable()
        {
            if (interactKey != null && interactKey.action != null)
            {
                interactKey.action.Disable();
            }

            if (currentHovered != null)
            {
                currentHovered.EndHover();
                currentHovered = null;
            }
        }

        protected void Start()
        {
            if (cam == null)
            {
                cam = GetComponent<Camera>();
                if (cam == null) cam = GetComponentInChildren<Camera>();
                if (cam == null) cam = Camera.main;
            }
        }

        protected void Update()
        {
            CheckForInteraction();
        }

        protected void CheckForInteraction()
        {
            if (cam == null)
            {
                cam = Camera.main;
                if (cam == null) return;
            }

            lastRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            hasHit = Physics.Raycast(lastRay, out lastHit, interactDistance, interactableLayerMask, triggerInteraction);

            if (hasHit)
            {
                NanoInteractable interactable = lastHit.collider.GetComponent<NanoInteractable>();
                if (interactable == null)
                {
                    interactable = lastHit.collider.GetComponentInParent<NanoInteractable>();
                }

                if (interactable != null && interactable.isCurrentlyInteractable)
                {
                    // If we were hovering something else before, stop hovering it
                    if (currentHovered != null && currentHovered != interactable)
                    {
                        currentHovered.EndHover();
                    }

                    // Start hovering this one
                    currentHovered = interactable;
                    currentHovered.StartHover();

                    if (IsInteractPressed())
                    {
                        currentHovered.Interact();
                    }
                }
                else
                {
                    // We hit something that's not interactable – stop hovering the old one
                    if (currentHovered != null)
                    {
                        currentHovered.EndHover();
                        currentHovered = null;
                    }
                }
            }
            else
            {
                // No hit at all – stop hovering if needed
                if (currentHovered != null)
                {
                    currentHovered.EndHover();
                    currentHovered = null;
                }
            }

            Debug.DrawRay(lastRay.origin, lastRay.direction * interactDistance, hasHit ? Color.green : Color.red);
        }

        protected bool IsInteractPressed()
        {
            if (interactKey != null && interactKey.action != null)
            {
                if (interactKey.action.WasPressedThisFrame() || interactKey.action.triggered || interactKey.action.WasPerformedThisFrame())
                {
                    return true;
                }
            }

            // Fallback for keyboard E key if interactKey is unassigned or not triggered
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                return true;
            }

            return false;
        }

        protected void OnDrawGizmos()
        {
            if (cam == null) return;

            Gizmos.color = hasHit ? Color.green : Color.red;
            Gizmos.DrawLine(lastRay.origin, lastRay.origin + lastRay.direction * interactDistance);

            if (hasHit)
                Gizmos.DrawSphere(lastHit.point, 0.05f);
        }
    }
}
