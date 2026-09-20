using UnityEngine;

namespace NanodogsToolkit.NanoInventory
{
    /// <summary>
    /// Handles the pickup of NanoItems in the game world.
    /// </summary>
    public class NanoItemPickup : MonoBehaviour
    {
        public NanoItem item; // Reference to the NanoItem scriptable object

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<NanoInventoryManager>().AddItem(item);

                // Destroy the item after pickup
                Destroy(gameObject);
            }
        }
    }
}
