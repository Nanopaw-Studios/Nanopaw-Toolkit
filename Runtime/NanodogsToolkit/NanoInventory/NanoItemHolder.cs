using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NanoItemHolder : MonoBehaviour
{
    public NanoItem item;
    public Image image;

    private void Start()
    {
        image.sprite = item.itemIcon;
        item.name = item.itemName;
    }
}