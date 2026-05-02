using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class NanoInventoryManager : MonoBehaviour
{
    [Header("Data")]
    public List<NanoItem> inventory = new List<NanoItem>();
    public int maxInventorySize = 20;
    public NanoItem equipped;

    [Header("UI")]
    [SerializeField] private Transform inventoryUI;           // assign in inspector (Canvas/Inventory)
    [SerializeField] private GameObject inventorySlotPrefab;  // assign in inspector

    private readonly List<NanoItemHolder> slots = new();

    private void Awake()
    {
        // Fallback if you didn't assign it
        if (inventoryUI == null)
        {
            var go = GameObject.Find("Canvas/Inventory");
            if (go) inventoryUI = go.transform;
        }

        if (inventoryUI == null)
            Debug.LogError("Inventory UI not found. Assign inventoryUI or ensure Canvas/Inventory exists.", this);

        if (inventorySlotPrefab == null)
            Debug.LogError("InventorySlotPrefab not assigned.", this);
    }

    private void Start()
    {
        BuildSlots();
        RefreshUI();
    }

    private void BuildSlots()
    {
        if (inventoryUI == null || inventorySlotPrefab == null) return;

        // optional: clear existing children if you re-enter playmode and duplicates happen
        // for (int i = inventoryUI.childCount - 1; i >= 0; i--) Destroy(inventoryUI.GetChild(i).gameObject);

        slots.Clear();

        for (int i = 0; i < maxInventorySize; i++)
        {
            GameObject slotGO = Instantiate(inventorySlotPrefab, inventoryUI);
            slotGO.name = $"Slot_{i}";

            var num = slotGO.transform.Find("Num");
            if (num)
            {
                var tmp = num.GetComponent<TMP_Text>();
                if (tmp) tmp.text = (i + 1).ToString(); // or i.ToString() if you want 0-based
            }

            var holder = slotGO.GetComponent<NanoItemHolder>();
            if (holder == null)
                Debug.LogError($"InventorySlotPrefab is missing NanoItemHolder component.", slotGO);
            else
                slots.Add(holder);
        }
    }

    private void RefreshUI()
    {
        // Fill slots based on current inventory
        for (int i = 0; i < slots.Count; i++)
        {
            NanoItem item = (i < inventory.Count) ? inventory[i] : null;

            slots[i].item = item;

            // If your holder has a method to update icon/text, call it here:
            // slots[i].Refresh();
        }
    }

    public void AddItem(NanoItem item)
    {
        if (item == null) return;

        if (inventory.Count >= maxInventorySize)
        {
            Debug.LogWarning("Inventory is full. Cannot add more items.");
            return;
        }

        inventory.Add(item);
        Debug.Log($"Added item: {item.itemName}");

        RefreshUI();
    }

    public void RemoveItem(NanoItem item)
    {
        if (item == null) return;

        if (inventory.Contains(item))
        {
            if (equipped == item) UnequipItem();

            inventory.Remove(item);
            Debug.Log($"Removed item: {item.itemName}");

            RefreshUI();
        }
        else
        {
            Debug.LogWarning($"Item not found in inventory: {item.itemName}");
        }
    }

    public void ClearInventory()
    {
        inventory.Clear();
        equipped = null;
        RefreshUI();
        Debug.Log("Inventory cleared.");
    }

    public void SortInventoryByName()
    {
        inventory.Sort((a, b) => a.itemName.CompareTo(b.itemName));
        RefreshUI();
        Debug.Log("Inventory sorted by item name.");
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
}
