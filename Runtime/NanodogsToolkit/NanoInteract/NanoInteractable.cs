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

        /// <summary>
        /// Called when the player interacts with this object.
        /// </summary>
        public virtual void Interact()
        {
            if (!isCurrentlyInteractable) return;

            Debug.Log($"{gameObject.name} got interacted with!");
            onInteract?.Invoke();
            //SetInteractable(false); ??
        }

        public void SetInteractable(bool value)
        {
            isCurrentlyInteractable = value;
        }
    }
}
