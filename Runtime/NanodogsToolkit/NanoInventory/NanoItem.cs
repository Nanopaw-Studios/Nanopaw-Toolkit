using UnityEngine;

public class NanoItem : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public GameObject viewModelPrefab;

    public virtual void Equip()
    {
        Debug.Log($"Equipping item: {itemName}");
    }

    public virtual void Use()
    {
        Debug.Log($"Using item: {itemName}");
    }
}