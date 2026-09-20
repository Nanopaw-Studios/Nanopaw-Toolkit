using System;
using UnityEngine;

[Serializable]
public class ViewModelData
{
    public GameObject viewModelPrefab;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public Vector3 scale;

    public ViewModelData()
    {
        positionOffset = Vector3.zero;
        rotationOffset = Vector3.zero;
        scale = Vector3.one;
    }
}