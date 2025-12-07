using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NanoInventoryManager : MonoBehaviour
{
    public List<NanoItem> inventory = new List<NanoItem>();
    public int maxInventorySize = 20;

    public NanoItem equipped;

    private GameObject InventoryUI;
    public GameObject InventorySlotPrefab;

    private void Start()
    {
        // this will be true anyway, but just in case
        if (InventoryUI == null)
        {
            InventoryUI = GameObject.Find("Canvas/Inventory");
        }
        // if still null, log an error
        if (InventoryUI == null)
        {
            Debug.LogError("Inventory UI not found in the scene. Please ensure there is a Canvas with an Inventory GameObject.");
        }

        if (InventoryUI != null && InventorySlotPrefab != null)
        {
            // Populate the inventory UI with slots
            for (int i = 0; i < maxInventorySize; i++)
            {
                GameObject slot = Instantiate(InventorySlotPrefab, InventoryUI.transform);
                slot.GetComponent<NanoItemHolder>().item = inventory[i];

                slot.transform.Find("Num").gameObject.GetComponent<TMP_Text>().text = i.ToString();
            }
        }
    }

    public void EquipItem(NanoItem item)
    {
        if (inventory.Contains(item))
        {
            equipped = item;
            Debug.Log($"Equipped item: {item.itemName}");
        }
        else
        {
            Debug.LogWarning($"Item not found in inventory: {item.itemName}");
        }
    }

    public void UnequipItem()
    {
        if (equipped != null)
        {
            Debug.Log($"Unequipped item: {equipped.itemName}");
            equipped = null;
        }
        else
        {
            Debug.LogWarning("No item is currently equipped.");
        }
    }

    public void AddItem(NanoItem item)
    {
        if (inventory.Count >= maxInventorySize)
        {
            Debug.LogWarning("Inventory is full. Cannot add more items.");
            return;
        }

        inventory.Add(item);
        Debug.Log($"Added item: {item.itemName}");
    }

    public void RemoveItem(NanoItem item)
    {
        if (inventory.Contains(item))
        {
            if (equipped == item)
                UnequipItem();

            inventory.Remove(item);
            Debug.Log($"Removed item: {item.itemName}");
        }
        else
        {
            Debug.LogWarning($"Item not found in inventory: {item.itemName}");
        }
    }

    public void UseItem(NanoItem item)
    {
        // Ensure the item is equipped before use
        if (equipped != item)
            return;

        if (inventory.Contains(item))
        {
            item.Use();
            RemoveItem(item);
        }
        else
        {
            Debug.LogWarning($"Item not found in inventory: {item.itemName}");
        }
    }

    public void ListInventory()
    {
        Debug.Log("Current Inventory:");
        foreach (var item in inventory)
        {
            Debug.Log($"- {item.itemName}");
        }
    }

    public void ClearInventory()
    {
        inventory.Clear();
        equipped = null;
        Debug.Log("Inventory cleared.");
    }

    public bool IsInventoryFull()
    {
        return inventory.Count >= maxInventorySize;
    }

    public int GetInventoryCount()
    {
        return inventory.Count;
    }

    public bool HasItem(NanoItem item)
    {
        return inventory.Contains(item);
    }

    public void SortInventoryByName()
    {
        inventory.Sort((item1, item2) => item1.itemName.CompareTo(item2.itemName));
        Debug.Log("Inventory sorted by item name.");
    }

    public void SortInventoryByType<T>() where T : NanoItem
    {
        inventory.Sort((item1, item2) =>
        {
            bool isItem1OfType = item1 is T;
            bool isItem2OfType = item2 is T;
            if (isItem1OfType && !isItem2OfType) return -1;
            if (!isItem1OfType && isItem2OfType) return 1;
            return 0;
        });
        Debug.Log($"Inventory sorted by type: {typeof(T).Name}");
    }

    public void TransferItemToInventory(NanoItem item, NanoInventoryManager targetInventory)
    {
        if (inventory.Contains(item))
        {
            if (targetInventory.IsInventoryFull())
            {
                Debug.LogWarning("Target inventory is full. Cannot transfer item.");
                return;
            }
            RemoveItem(item);
            targetInventory.AddItem(item);
            Debug.Log($"Transferred item: {item.itemName} to target inventory.");
        }
        else
        {
            Debug.LogWarning($"Item not found in inventory: {item.itemName}");
        }
    }
}
