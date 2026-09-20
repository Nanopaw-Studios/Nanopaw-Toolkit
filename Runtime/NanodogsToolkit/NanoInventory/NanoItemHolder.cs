using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NanoItemHolder : MonoBehaviour
{
    public NanoItem item;
    public Image image;

    public bool emptySlot = true;

    private void Start()
    {
        if (item != null && emptySlot == false)
        {
            UpdateItem();
        }
    }

    public void UpdateItem()
    {
        if (item != null)
        {
            image.sprite = item.itemIcon;
            image.color = Color.white;
            item.name = item.itemName;
        }
        else
        {
            image.sprite = null;
            image.color = new Color(1, 1, 1, 0);
        }
    }

    private void OnValidate() => UpdateItem();
}