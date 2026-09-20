using UnityEngine;
using UnityEngine.Events;

namespace Nanodogs.UniversalScripts
{
    [RequireComponent(typeof(Collider))] // Ensures it has a collider
    public class NanoInteractable : MonoBehaviour
    {
        [HideInInspector]
        public bool hovering = false;

        [Header("Interaction Settings")]
        [Tooltip("Can the player currently interact with this object?")]
        public bool isCurrentlyInteractable = true;

        [Header("Events")]
        public UnityEvent onInteract;
        public UnityEvent onHoverEnter;
        public UnityEvent onHoverExit;

        /// <summary>
        /// Called when the player starts hovering over this object.
        /// </summary>
        public virtual void StartHover()
        {
            if (hovering) return;
            hovering = true;
            onHoverEnter?.Invoke();
        }

        /// <summary>
        /// Called when the player stops hovering over this object.
        /// </summary>
        public virtual void EndHover()
        {
            if (!hovering) return;
            hovering = false;
            onHoverExit?.Invoke();
        }

        /// <summary>
        /// Called when the player interacts with this object.
        /// </summary>
        public virtual void Interact()
        {
            if (!isCurrentlyInteractable) return;

            Debug.Log($"{gameObject.name} got interacted with!");
            onInteract?.Invoke();
        }

        public void SetInteractable(bool value)
        {
            isCurrentlyInteractable = value;
            if (!isCurrentlyInteractable && hovering)
            {
                EndHover();
            }
        }

        protected virtual void OnDisable()
        {
            if (hovering)
            {
                EndHover();
            }
        }
    }
}
