using UnityEngine;
using UnityEditor;
using System.IO;

namespace Nanodogs.Toolkit
{
    /// <summary>
    /// A terrain generator that uses a "splat" map to specify where things
    /// in world-space should be on the height-mapped terrain.
    /// </summary>
    public class TerrainGeneratorWindow : NDSEditorWindow
    {
        // Texture2D variables to hold the heightmap and the splat map (for object placement)
        private Texture2D heightMap;
        private Texture2D splatMap;

        // Prefabs for the objects to be placed on the terrain
        private GameObject treePrefab;
        private GameObject rockPrefab;
        private GameObject waterPrefab;

        // Terrain settings
        private float terrainHeightMultiplier = 50f;
        private int terrainWidth = 513;
        private int terrainLength = 513;

        // Object placement settings
        private float objectPlacementThreshold = 0.1f;
        private float placementDensity = 1.0f; // 1.0 means 100% density

        // Scroll position for the window
        private Vector2 scrollPos;

        // Colors to look for in the splat map
        private Color treeColor = Color.green;
        private Color rockColor = Color.gray;
        private Color waterColor = Color.blue;

        // Name for the parent object holding placed items
        private const string PlacedObjectsHolderName = "Placed Objects";

        /// <summary>
        /// Creates a new menu item in the Unity Editor to open this window.
        /// </summary>
        [MenuItem("Nanodogs/Tools/Utilites/A Not Complicated Terrain Generator")]
        public static void ShowWindow()
        {
            // Get existing open window or if none, make a new one.
            GetWindow<TerrainGeneratorWindow>("NDS ANCTG");
        }

        /// <summary>
        /// Renders the UI for the custom editor window.
        /// </summary>
        new void OnGUI()
        {
            // Begin a scrollable view
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Label("Terrain Generation Settings", EditorStyles.boldLabel);

            // Fields for the user to drag and drop the heightmap and splat map textures
            heightMap = (Texture2D)EditorGUILayout.ObjectField("Height Map", heightMap, typeof(Texture2D), false);
            splatMap = (Texture2D)EditorGUILayout.ObjectField("Splat Map", splatMap, typeof(Texture2D), false);

            // Fields for terrain dimensions and height
            terrainWidth = EditorGUILayout.IntField("Terrain Width", terrainWidth);
            terrainLength = EditorGUILayout.IntField("Terrain Length", terrainLength);
            terrainHeightMultiplier = EditorGUILayout.FloatField("Terrain Height Multiplier", terrainHeightMultiplier);

            GUILayout.Space(10);
            GUILayout.Label("Object Placement Settings", EditorStyles.boldLabel);

            // Fields for the prefabs to be instantiated
            treePrefab = (GameObject)EditorGUILayout.ObjectField("Tree Prefab", treePrefab, typeof(GameObject), false);
            rockPrefab = (GameObject)EditorGUILayout.ObjectField("Rock Prefab", rockPrefab, typeof(GameObject), false);
            waterPrefab = (GameObject)EditorGUILayout.ObjectField("Water Prefab", waterPrefab, typeof(GameObject), false);

            // Threshold for color matching
            objectPlacementThreshold = EditorGUILayout.Slider("Placement Color Threshold", objectPlacementThreshold, 0.0f, 1.0f);

            // NEW: Slider to control the density of placed objects
            placementDensity = EditorGUILayout.Slider("Placement Density", placementDensity, 0.0f, 1.0f);


            GUILayout.Space(20);

            // Button to trigger the terrain generation process
            if (GUILayout.Button("Generate Terrain"))
            {
                if (heightMap != null)
                {
                    GenerateTerrain();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please assign a height map texture.", "OK");
                }
            }

            // Button to trigger the object placement process
            if (GUILayout.Button("Place Objects"))
            {
                if (splatMap != null && (treePrefab != null || rockPrefab != null || waterPrefab != null))
                {
                    PlaceObjects();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please assign a splat map and at least one prefab.", "OK");
                }
            }

            GUILayout.Space(10);

            // NEW: Button to clear previously placed objects
            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f); // Make the button red
            if (GUILayout.Button("Clear Placed Objects"))
            {
                if (EditorUtility.DisplayDialog("Clear Placed Objects?",
                    "Are you sure you want to delete the '" + PlacedObjectsHolderName + "' object and all its children? This cannot be undone.",
                    "Yes, Clear Them", "No"))
                {
                    ClearObjects();
                }
            }
            GUI.backgroundColor = Color.white; // Reset color

            EditorGUILayout.EndScrollView();

            base.OnGUI();
        }

        /// <summary>
        /// Generates the terrain based on the provided heightmap.
        /// </summary>
        private void GenerateTerrain()
        {
            // Find and delete existing terrain to avoid duplicates
            Terrain existingTerrain = FindFirstObjectByType<Terrain>();
            if (existingTerrain != null)
            {
                DestroyImmediate(existingTerrain.gameObject);
            }

            // Create a new TerrainData object
            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = terrainWidth;
            terrainData.size = new Vector3(terrainWidth, terrainHeightMultiplier, terrainLength);

            // Create the heightmap array
            float[,] heights = new float[terrainLength, terrainWidth];

            // FIX: Loop through each point in the terrain's heightmap
            for (int y = 0; y < terrainLength; y++)
            {
                for (int x = 0; x < terrainWidth; x++)
                {
                    // Calculate normalized coordinates (u,v) to sample the texture
                    float u = (float)x / (terrainWidth - 1);
                    float v = (float)y / (terrainLength - 1);

                    // Sample the texture at the normalized coordinates using bilinear filtering for a smooth result
                    // This correctly maps the entire texture to the terrain, regardless of resolution differences.
                    float height = heightMap.GetPixelBilinear(u, v).grayscale;

                    // IMPORTANT: Terrain heights are indexed [y, x]
                    heights[y, x] = height;
                }
            }

            // Set the heights of the terrain data
            terrainData.SetHeights(0, 0, heights);

            // Create a new terrain object in the scene
            GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.name = "Generated Terrain";
        }

        /// <summary>
        /// Places objects on the terrain based on the splat map.
        /// </summary>
        private void PlaceObjects()
        {
            // Find the generated terrain in the scene
            Terrain terrain = FindFirstObjectByType<Terrain>();
            if (terrain == null)
            {
                EditorUtility.DisplayDialog("Error", "No terrain found in the scene. Please generate the terrain first.", "OK");
                return;
            }

            // Find or create a parent object to hold the instantiated objects
            GameObject objectHolder = GameObject.Find(PlacedObjectsHolderName);
            if (objectHolder == null)
            {
                objectHolder = new GameObject(PlacedObjectsHolderName);
            }


            int mapWidth = splatMap.width;
            int mapHeight = splatMap.height;

            // Loop through each pixel of the splat map
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    // NEW: Check against placement density.
                    // If a random value is higher than our density, skip this pixel.
                    if (Random.value > placementDensity)
                    {
                        continue;
                    }

                    Color pixelColor = splatMap.GetPixel(x, y);

                    // Calculate the world position corresponding to the pixel
                    float worldX = (float)x / mapWidth * terrain.terrainData.size.x;
                    float worldZ = (float)y / mapHeight * terrain.terrainData.size.z;
                    float worldY = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));
                    Vector3 position = new Vector3(worldX, worldY, worldZ);

                    // Check if the pixel color matches any of the predefined colors
                    if (ColorMatch(pixelColor, treeColor))
                    {
                        InstantiatePrefab(treePrefab, position, objectHolder.transform);
                    }
                    else if (ColorMatch(pixelColor, rockColor))
                    {
                        InstantiatePrefab(rockPrefab, position, objectHolder.transform);
                    }
                    else if (ColorMatch(pixelColor, waterColor))
                    {
                        InstantiatePrefab(waterPrefab, position, objectHolder.transform);
                    }
                }
            }
        }

        /// <summary>
        /// NEW: Clears all objects that were placed by this tool.
        /// </summary>
        private void ClearObjects()
        {
            GameObject objectHolder = GameObject.Find(PlacedObjectsHolderName);
            if (objectHolder != null)
            {
                DestroyImmediate(objectHolder);
            }
        }

        /// <summary>
        /// Checks if two colors are similar within a given threshold.
        /// </summary>
        private bool ColorMatch(Color c1, Color c2)
        {
            float r = c1.r - c2.r;
            float g = c1.g - c2.g;
            float b = c1.b - c2.b;
            return (r * r + g * g + b * b) < objectPlacementThreshold;
        }

        /// <summary>
        /// Instantiates a prefab at a given position and parents it to a transform.
        /// </summary>
        private void InstantiatePrefab(GameObject prefab, Vector3 position, Transform parent)
        {
            if (prefab != null)
            {
                GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                newObject.transform.position = position;

                // FIX: Give the object a random rotation around the Y axis for a more natural look.
                newObject.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                newObject.transform.parent = parent;
            }
        }
    }
}