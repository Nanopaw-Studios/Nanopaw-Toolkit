using UnityEngine;

[CreateAssetMenu(fileName = "NewNanoItem", menuName = "Nanodogs/Nano Item", order = 1)]
public class NanoItem : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;

    public ViewModelData viewModelData;

    public virtual void Equip()
    {
        Debug.Log($"Equipping item: {itemName}");
    }

    public virtual void Use()
    {
        Debug.Log($"Using item: {itemName}");
    }
}