// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;
using UnityEditor;

public class NanodogsToolkitVolumetricGenerator : EditorWindow
{
    private Light lightSource;
    private int size = 64;
    private string savePath = "Assets/LightVolume.asset";

    [MenuItem("Nanodogs/Tools/Utilites/Generate Light Volume 3D Texture")]
    public static void ShowWindow()
    {
        GetWindow<NanodogsToolkitVolumetricGenerator>("Light Volume Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Light Volume Texture Generator", EditorStyles.boldLabel);

        lightSource = (Light)EditorGUILayout.ObjectField("Light Source", lightSource, typeof(Light), true);
        size = EditorGUILayout.IntSlider("Texture Size", size, 8, 256);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        if (GUILayout.Button("Generate 3D Light Volume"))
        {
            GenerateLightVolume();
        }
    }

    private void GenerateLightVolume()
    {
        if (lightSource == null)
        {
            Debug.LogError("Assign a light source first.");
            return;
        }

        var volumeTex = new Texture3D(size, size, size, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size * size * size];

        Vector3 lightDir = -lightSource.transform.forward;
        Vector3 center = Vector3.one * 0.5f;

        for (int z = 0; z < size; z++)
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Vector3 pos = new Vector3(x, y, z) / (float)size;
                    Vector3 dir = (pos - center).normalized;
                    float alignment = Mathf.Clamp01(Vector3.Dot(dir, lightDir));

                    colors[x + y * size + z * size * size] = new Color(alignment, alignment, alignment, 1f);
                }

        volumeTex.SetPixels(colors);
        volumeTex.Apply();

        AssetDatabase.CreateAsset(volumeTex, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Light Volume saved at {savePath}");
    }
}
