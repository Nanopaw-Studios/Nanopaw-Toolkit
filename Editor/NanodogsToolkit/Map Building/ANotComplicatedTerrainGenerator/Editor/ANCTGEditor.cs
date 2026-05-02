using UnityEngine;
using UnityEditor;
using System.IO;

namespace Nanodogs.Toolkit.MapBuilding
{
    /// <summary>
    /// A terrain generator that supports both texture-based and fully procedural
    /// generation using layered Perlin noise, stepped terrain, and island falloff.
    /// </summary>
    public class ANCTGEditor : NDSEditorWindow
    {
        // ─────────────────────────────────────────────────────────────────────────────
        // SECTION FOLDOUTS
        // ─────────────────────────────────────────────────────────────────────────────
        private bool showManualSection       = true;
        private bool showProceduralSection   = false;
        private bool showObjectSection       = true;
        private bool showAdvancedObjSettings = false;

        // ─────────────────────────────────────────────────────────────────────────────
        // SHARED TERRAIN SETTINGS
        // ─────────────────────────────────────────────────────────────────────────────
        private float terrainHeightMultiplier = 50f;
        private int   terrainWidth            = 513;
        private int   terrainLength           = 513;

        // ─────────────────────────────────────────────────────────────────────────────
        // MANUAL (TEXTURE-BASED) SETTINGS
        // ─────────────────────────────────────────────────────────────────────────────
        private Texture2D heightMap;
        private Texture2D splatMap;

        // ─────────────────────────────────────────────────────────────────────────────
        // PROCEDURAL NOISE SETTINGS
        // ─────────────────────────────────────────────────────────────────────────────
        private int     noiseSeed       = 42;
        private float   noiseScale      = 80f;
        private int     noiseOctaves    = 4;
        private float   noisePersistence = 0.5f;
        private float   noiseLacunarity = 2.0f;
        private Vector2 noiseOffset     = Vector2.zero;

        // ─────────────────────────────────────────────────────────────────────────────
        // STEPPED / TERRACED TERRAIN
        // ─────────────────────────────────────────────────────────────────────────────
        private bool  useSteppedTerrain = false;
        private int   stepCount         = 6;
        private float stepSmoothness    = 0f;   // 0 = hard steps, 1 = fully smooth

        // ─────────────────────────────────────────────────────────────────────────────
        // FALLOFF MAP (ISLAND MODE)
        // ─────────────────────────────────────────────────────────────────────────────
        private bool  useFalloffMap   = false;
        private float falloffStrength = 3.0f;   // controls how aggressively edges fall
        private float falloffShift    = 2.2f;   // controls how far island land extends

        // ─────────────────────────────────────────────────────────────────────────────
        // PROCEDURAL SPLAT MAP ZONES (height thresholds)
        // ─────────────────────────────────────────────────────────────────────────────
        private float waterHeightThreshold = 0.28f;
        private float grassHeightThreshold = 0.62f;
        // Anything above grassHeightThreshold maps to rockColor

        // ─────────────────────────────────────────────────────────────────────────────
        // PREVIEW
        // ─────────────────────────────────────────────────────────────────────────────
        private bool      showPreview            = true;
        private Texture2D proceduralHeightPreview;
        private Texture2D proceduralSplatPreview;
        private const int PreviewSize            = 150;

        // ─────────────────────────────────────────────────────────────────────────────
        // OBJECT PLACEMENT – PREFABS & COLORS
        // ─────────────────────────────────────────────────────────────────────────────
        private GameObject treePrefab;
        private GameObject rockPrefab;
        private GameObject waterPrefab;

        private Color treeColor  = Color.green;
        private Color rockColor  = Color.gray;
        private Color waterColor = Color.blue;

        private float objectPlacementThreshold = 0.1f;
        private float placementDensity         = 1.0f;

        // ─────────────────────────────────────────────────────────────────────────────
        // OBJECT PLACEMENT – ADVANCED
        // ─────────────────────────────────────────────────────────────────────────────
        private float minPlacementSlope = 0f;
        private float maxPlacementSlope = 35f;
        private bool  randomScaleObjects = false;
        private float minObjectScale    = 0.8f;
        private float maxObjectScale    = 1.2f;
        private bool  alignToNormal     = false;

        // ─────────────────────────────────────────────────────────────────────────────
        // RUNTIME STATE
        // ─────────────────────────────────────────────────────────────────────────────
        private float[,] generatedHeights;
        private Vector2   scrollPos;

        private const string PlacedObjectsHolderName = "Placed Objects";

        // ─────────────────────────────────────────────────────────────────────────────
        // MENU ITEM
        // ─────────────────────────────────────────────────────────────────────────────
        [MenuItem("Nanodogs/Tools/Utilites/Map Building/A Not Complicated Terrain Generator")]
        public static void ShowWindow()
        {
            GetWindow<ANCTGEditor>("NDS ANCTG");
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GUI
        // ─────────────────────────────────────────────────────────────────────────────
        new void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawWindowHeader();
            GUILayout.Space(8);

            DrawSection("📁  Manual Input (Texture-Based)", ref showManualSection,    DrawManualSection);
            GUILayout.Space(4);
            DrawSection("🎲  Procedural Generation (Baseless)", ref showProceduralSection, DrawProceduralSection);
            GUILayout.Space(4);
            DrawSection("🌲  Object Placement", ref showObjectSection, DrawObjectPlacementSection);

            GUILayout.Space(12);
            DrawClearButton();

            EditorGUILayout.EndScrollView();
            base.OnGUI();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────────

        private void DrawWindowHeader()
        {
            EditorGUILayout.HelpBox(
                "NDS — A Not Complicated Terrain Generator\n" +
                "Generate terrain from textures or procedurally with noise, steps, and island falloff.",
                MessageType.None);

            GUILayout.Space(4);
            GUILayout.Label("Shared Terrain Size", EditorStyles.boldLabel);
            terrainWidth  = EditorGUILayout.IntField("Terrain Width",  terrainWidth);
            terrainLength = EditorGUILayout.IntField("Terrain Length", terrainLength);
            terrainHeightMultiplier = EditorGUILayout.FloatField("Height Multiplier", terrainHeightMultiplier);
        }

        /// <summary>Wraps a draw action in a foldout header group.</summary>
        private void DrawSection(string title, ref bool foldout, System.Action drawContent)
        {
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, title);
            if (foldout)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(2);
                drawContent?.Invoke();
                GUILayout.Space(6);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // SECTION: MANUAL
        // ─────────────────────────────────────────────────────────────────────────────
        private void DrawManualSection()
        {
            EditorGUI.BeginChangeCheck();
            var tempHM = (Texture2D)EditorGUILayout.ObjectField("Height Map", heightMap, typeof(Texture2D), false);
            var tempSM = (Texture2D)EditorGUILayout.ObjectField("Splat Map",  splatMap,  typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck())
            {
                heightMap = tempHM;
                splatMap  = tempSM;
            }

            GUILayout.Space(6);

            if (GUILayout.Button("Generate Terrain from Height Map"))
            {
                if (heightMap != null)
                    GenerateTerrainFromTexture();
                else
                    EditorUtility.DisplayDialog("Error", "Please assign a Height Map texture.", "OK");
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // SECTION: PROCEDURAL
        // ─────────────────────────────────────────────────────────────────────────────
        private void DrawProceduralSection()
        {
            // ── Noise ──
            GUILayout.Label("Noise", EditorStyles.boldLabel);

            // Seed row — undo-aware, with randomise and regenerate buttons
            EditorGUI.BeginChangeCheck();
            int     tempSeed   = EditorGUILayout.IntField("Seed", noiseSeed);
            if (EditorGUI.EndChangeCheck())
            {
                noiseSeed = tempSeed;
                RunFastPreview();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🎲  Randomise Seed"))
            {
                noiseSeed = Random.Range(0, 99999);
                RunFastPreview();
            }
            // Regenerate: picks a brand-new seed each time so the result changes
            if (GUILayout.Button("🔄  Regenerate"))
            {
                noiseSeed = Random.Range(0, 99999);
                RunProceduralGeneration();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "The seed is what keeps the result identical between runs. " +
                "Regenerate picks a new seed automatically.",
                MessageType.None);

            GUILayout.Space(6);

            // Noise shape fields — one undo group for the whole block
            EditorGUI.BeginChangeCheck();
            float   tempScale       = EditorGUILayout.Slider("Scale",       noiseScale,       1f,  500f);
            int     tempOctaves     = EditorGUILayout.IntSlider("Octaves",  noiseOctaves,     1,   8);
            float   tempPersistence = EditorGUILayout.Slider("Persistence", noisePersistence, 0f,  1f);
            float   tempLacunarity  = EditorGUILayout.Slider("Lacunarity",  noiseLacunarity,  1f,  4f);
            Vector2 tempOffset      = EditorGUILayout.Vector2Field("Offset", noiseOffset);
            if (EditorGUI.EndChangeCheck())
            {
                noiseScale       = tempScale;
                noiseOctaves     = tempOctaves;
                noisePersistence = tempPersistence;
                noiseLacunarity  = tempLacunarity;
                noiseOffset      = tempOffset;
                RunFastPreview();
            }

            GUILayout.Space(8);

            // ── Stepped Terrain ──
            GUILayout.Label("Stepped / Terraced", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            bool  tempUseStepped = EditorGUILayout.Toggle("Enable Stepped Terrain", useSteppedTerrain);
            int tempStepCount = tempUseStepped
                ? EditorGUILayout.IntSlider("Step Count", stepCount, 2, 20)
                : stepCount;

            float tempStepSmooth = tempUseStepped
                ? EditorGUILayout.Slider("Step Smoothness", stepSmoothness, 0f, 1f)
                : stepSmoothness;
            if (useSteppedTerrain)
                EditorGUILayout.HelpBox("Smoothness 0 = hard terracing · 1 = blend back to smooth noise.", MessageType.None);
            if (EditorGUI.EndChangeCheck())
            {
                useSteppedTerrain = tempUseStepped;
                stepCount         = tempStepCount;
                stepSmoothness    = tempStepSmooth;
                RunFastPreview();
            }

            GUILayout.Space(8);

            // ── Falloff / Island ──
            GUILayout.Label("Island Falloff", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            bool  tempUseFalloff = EditorGUILayout.Toggle("Enable Falloff Map", useFalloffMap);
            float tempFallStr = tempUseFalloff
                ? EditorGUILayout.Slider("Falloff Strength", falloffStrength, 1f, 10f)
                : falloffStrength;

            float tempFallShift = tempUseFalloff
                ? EditorGUILayout.Slider("Falloff Shift", falloffShift, 0f, 5f)
                : falloffShift;
            if (useFalloffMap)
                EditorGUILayout.HelpBox(
                    "Strength controls edge aggressiveness.\n" +
                    "Shift controls how far the island land extends from centre.",
                    MessageType.None);
            if (EditorGUI.EndChangeCheck())
            {
                useFalloffMap   = tempUseFalloff;
                falloffStrength = tempFallStr;
                falloffShift    = tempFallShift;
                RunFastPreview();
            }

            GUILayout.Space(8);

            // ── Auto Splat Zones ──
            GUILayout.Label("Auto Splat Map Zones (by Height)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Height below A → Water colour\n" +
                "Height A–B     → Tree colour\n" +
                "Height above B → Rock colour",
                MessageType.None);
            EditorGUI.BeginChangeCheck();
            float tempWater = EditorGUILayout.Slider("Water / Grass Cutoff", waterHeightThreshold, 0f, 1f);
            float tempGrass = EditorGUILayout.Slider("Grass / Rock Cutoff",  grassHeightThreshold, 0f, 1f);
            if (tempWater > tempGrass) tempGrass = tempWater;
            if (EditorGUI.EndChangeCheck())
            {
                waterHeightThreshold = tempWater;
                grassHeightThreshold = tempGrass;
                RunFastPreview();
            }

            GUILayout.Space(8);

            // ── Preview ──
            EditorGUI.BeginChangeCheck();
            bool tempShowPreview = EditorGUILayout.Toggle("Show Previews", showPreview);
            if (EditorGUI.EndChangeCheck())
            {
                showPreview = tempShowPreview;
            }

            if (showPreview && proceduralHeightPreview != null)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();
                GUILayout.Label("Height Map  (live)", EditorStyles.miniLabel);
                Rect hr = GUILayoutUtility.GetRect(PreviewSize, PreviewSize, GUILayout.ExpandWidth(false));
                EditorGUI.DrawPreviewTexture(hr, proceduralHeightPreview);
                EditorGUILayout.EndVertical();

                GUILayout.Space(8);

                if (proceduralSplatPreview != null)
                {
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label("Splat Map  (live)", EditorStyles.miniLabel);
                    Rect sr = GUILayoutUtility.GetRect(PreviewSize, PreviewSize, GUILayout.ExpandWidth(false));
                    EditorGUI.DrawPreviewTexture(sr, proceduralSplatPreview);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndHorizontal();
            }
            else if (showPreview && proceduralHeightPreview == null)
            {
                EditorGUILayout.HelpBox("Adjust any value above to generate a live preview.", MessageType.None);
            }

            GUILayout.Space(8);

            // ── Buttons ──
            if (GUILayout.Button("Generate Terrain Procedurally"))
            {
                RunProceduralGeneration();
                if (generatedHeights != null)
                    GenerateTerrainFromHeights(generatedHeights);
            }

            GUILayout.Space(4);

            GUI.backgroundColor = new Color(0.6f, 0.9f, 1f);
            if (GUILayout.Button("Export Generated Maps to Assets/GeneratedMaps"))
            {
                if (generatedHeights != null)
                    ExportMapsToAssets();
                else
                    EditorUtility.DisplayDialog("Nothing to Export",
                        "Generate terrain first, or adjust any value to trigger a preview.", "OK");
            }
            GUI.backgroundColor = Color.white;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // SECTION: OBJECT PLACEMENT
        // ─────────────────────────────────────────────────────────────────────────────
        private void DrawObjectPlacementSection()
        {
            GUILayout.Label("Prefabs", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            var tempTree  = (GameObject)EditorGUILayout.ObjectField("Tree Prefab",  treePrefab,  typeof(GameObject), false);
            var tempRock  = (GameObject)EditorGUILayout.ObjectField("Rock Prefab",  rockPrefab,  typeof(GameObject), false);
            var tempWater = (GameObject)EditorGUILayout.ObjectField("Water Prefab", waterPrefab, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck())
            {
                treePrefab  = tempTree;
                rockPrefab  = tempRock;
                waterPrefab = tempWater;
            }

            GUILayout.Space(6);

            GUILayout.Label("Splat Map Colors", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            Color tempTC = EditorGUILayout.ColorField("Tree Color",  treeColor);
            Color tempRC = EditorGUILayout.ColorField("Rock Color",  rockColor);
            Color tempWC = EditorGUILayout.ColorField("Water Color", waterColor);
            if (EditorGUI.EndChangeCheck())
            {
                treeColor  = tempTC;
                rockColor  = tempRC;
                waterColor = tempWC;
                // Splat zone colors affect the live preview too
                RunFastPreview();
            }

            GUILayout.Space(6);

            EditorGUI.BeginChangeCheck();
            float tempThreshold = EditorGUILayout.Slider("Color Match Threshold", objectPlacementThreshold, 0f, 1f);
            float tempDensity   = EditorGUILayout.Slider("Placement Density",     placementDensity,         0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                objectPlacementThreshold = tempThreshold;
                placementDensity         = tempDensity;
            }

            GUILayout.Space(6);

            // Advanced sub-foldout
            showAdvancedObjSettings = EditorGUILayout.Foldout(showAdvancedObjSettings, "Advanced Placement Settings", true);
            if (showAdvancedObjSettings)
            {
                EditorGUI.indentLevel++;

                GUILayout.Label("Slope Filter (degrees)", EditorStyles.miniLabel);
                EditorGUI.BeginChangeCheck();
                float tempMinSlope = EditorGUILayout.Slider("Min Slope", minPlacementSlope, 0f, 90f);
                float tempMaxSlope = EditorGUILayout.Slider("Max Slope", maxPlacementSlope, 0f, 90f);
                if (tempMinSlope > tempMaxSlope) tempMaxSlope = tempMinSlope;
                if (EditorGUI.EndChangeCheck())
                {
                    minPlacementSlope = tempMinSlope;
                    maxPlacementSlope = tempMaxSlope;
                }
                EditorGUILayout.HelpBox("Objects only spawn where terrain steepness is within this range.", MessageType.None);

                GUILayout.Space(4);

                EditorGUI.BeginChangeCheck();
                bool  tempRandScale = EditorGUILayout.Toggle("Random Scale", randomScaleObjects);
                float tempMinScale  = randomScaleObjects ? EditorGUILayout.FloatField("Min Scale", minObjectScale) : minObjectScale;
                float tempMaxScale  = randomScaleObjects ? EditorGUILayout.FloatField("Max Scale", maxObjectScale) : maxObjectScale;
                if (EditorGUI.EndChangeCheck())
                {
                    randomScaleObjects = tempRandScale;
                    minObjectScale     = tempMinScale;
                    maxObjectScale     = tempMaxScale;
                }

                GUILayout.Space(4);

                EditorGUI.BeginChangeCheck();
                bool tempAlign = EditorGUILayout.Toggle("Align to Terrain Normal", alignToNormal);
                if (EditorGUI.EndChangeCheck())
                {
                    alignToNormal = tempAlign;
                }
                EditorGUILayout.HelpBox(
                    "When enabled, objects tilt to match the slope of the terrain they rest on.",
                    MessageType.None);

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(8);

            if (GUILayout.Button("Place Objects from Splat Map (texture)"))
            {
                if (splatMap != null && AnyPrefabAssigned())
                    PlaceObjects(splatMap);
                else
                    EditorUtility.DisplayDialog("Error",
                        "Assign a Splat Map texture and at least one prefab.", "OK");
            }

            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
            if (GUILayout.Button("Place Objects from Procedural Splat Map"))
            {
                if (proceduralSplatPreview != null && AnyPrefabAssigned())
                    PlaceObjects(proceduralSplatPreview);
                else
                    EditorUtility.DisplayDialog("Error",
                        "Generate procedural maps first and assign at least one prefab.", "OK");
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawClearButton()
        {
            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
            if (GUILayout.Button("🗑  Clear Placed Objects"))
            {
                if (EditorUtility.DisplayDialog("Clear Placed Objects?",
                    $"Delete '{PlacedObjectsHolderName}' and all its children? This cannot be undone.",
                    "Yes, Clear", "Cancel"))
                {
                    ClearObjects();
                }
            }
            GUI.backgroundColor = Color.white;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // PROCEDURAL GENERATION LOGIC
        // ─────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Lightweight preview-only generation. Runs the noise loop at a capped
        /// resolution (max 256×256) so it's fast enough for real-time updates while
        /// a slider is being dragged. Does NOT update <see cref="generatedHeights"/>.
        /// </summary>
        private void RunFastPreview()
        {
            if (!showPreview) return;

            // Cap at 256 so the loop stays snappy regardless of terrain size
            const int maxPreviewRes = 256;
            int pw = Mathf.Min(terrainWidth,  maxPreviewRes);
            int ph = Mathf.Min(terrainLength, maxPreviewRes);

            float[,] heights = ComputeHeightData(pw, ph);

            proceduralHeightPreview = new Texture2D(pw, ph, TextureFormat.RGB24, false);
            proceduralSplatPreview  = new Texture2D(pw, ph, TextureFormat.RGB24, false);

            for (int y = 0; y < ph; y++)
            {
                for (int x = 0; x < pw; x++)
                {
                    float v = heights[y, x];
                    proceduralHeightPreview.SetPixel(x, y, new Color(v, v, v));
                    proceduralSplatPreview.SetPixel(x, y, HeightToSplatColor(v));
                }
            }

            proceduralHeightPreview.Apply();
            proceduralSplatPreview.Apply();
            Repaint();
        }

        /// <summary>
        /// Full-resolution generation. Updates <see cref="generatedHeights"/> and
        /// rebuilds both preview textures at <see cref="PreviewSize"/>.
        /// </summary>
        private void RunProceduralGeneration()
        {
            int w = terrainWidth;
            int h = terrainLength;

            generatedHeights = ComputeHeightData(w, h);

            // Rebuild preview textures sampled down from full-res data
            proceduralHeightPreview = new Texture2D(PreviewSize, PreviewSize, TextureFormat.RGB24, false);
            proceduralSplatPreview  = new Texture2D(PreviewSize, PreviewSize, TextureFormat.RGB24, false);

            for (int py = 0; py < PreviewSize; py++)
            {
                for (int px = 0; px < PreviewSize; px++)
                {
                    int hx = Mathf.Clamp(Mathf.FloorToInt((float)px / PreviewSize * w), 0, w - 1);
                    int hy = Mathf.Clamp(Mathf.FloorToInt((float)py / PreviewSize * h), 0, h - 1);
                    float v = generatedHeights[hy, hx];
                    proceduralHeightPreview.SetPixel(px, py, new Color(v, v, v));
                    proceduralSplatPreview.SetPixel(px, py, HeightToSplatColor(v));
                }
            }

            proceduralHeightPreview.Apply();
            proceduralSplatPreview.Apply();
            Repaint();
        }

        /// <summary>
        /// Core noise computation at an arbitrary resolution. Used by both
        /// <see cref="RunFastPreview"/> (small) and <see cref="RunProceduralGeneration"/> (full).
        /// </summary>
        private float[,] ComputeHeightData(int w, int h)
        {
            System.Random rng = new System.Random(noiseSeed);
            Vector2[] octaveOffsets = new Vector2[noiseOctaves];
            for (int i = 0; i < noiseOctaves; i++)
            {
                octaveOffsets[i] = new Vector2(
                    rng.Next(-100000, 100000) + noiseOffset.x,
                    rng.Next(-100000, 100000) + noiseOffset.y
                );
            }

            float maxVal = float.MinValue;
            float minVal = float.MaxValue;
            float[,] raw = new float[h, w];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float amplitude  = 1f;
                    float frequency  = 1f;
                    float noiseValue = 0f;

                    for (int oct = 0; oct < noiseOctaves; oct++)
                    {
                        float sx = (x + octaveOffsets[oct].x) / noiseScale * frequency;
                        float sy = (y + octaveOffsets[oct].y) / noiseScale * frequency;
                        noiseValue += (Mathf.PerlinNoise(sx, sy) * 2f - 1f) * amplitude;
                        amplitude *= noisePersistence;
                        frequency *= noiseLacunarity;
                    }

                    raw[y, x] = noiseValue;
                    if (noiseValue > maxVal) maxVal = noiseValue;
                    if (noiseValue < minVal) minVal = noiseValue;
                }
            }

            float[,] falloff = useFalloffMap ? BuildFalloffMap(w, h) : null;
            float[,] heights = new float[h, w];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float v = Mathf.InverseLerp(minVal, maxVal, raw[y, x]);
                    if (useFalloffMap)   v = Mathf.Clamp01(v - falloff[y, x]);
                    if (useSteppedTerrain) v = ApplyStep(v);
                    heights[y, x] = v;
                }
            }

            return heights;
        }

        // ── Falloff ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a square falloff map where the centre is 0 (no falloff) and
        /// the edges approach 1 (full falloff). The curve is controlled by
        /// <see cref="falloffStrength"/> and <see cref="falloffShift"/>.
        /// </summary>
        private float[,] BuildFalloffMap(int w, int h)
        {
            float[,] map = new float[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Map pixel coords to [-1, 1]
                    float nx = x / (float)w * 2f - 1f;
                    float ny = y / (float)h * 2f - 1f;

                    // Square falloff: take the larger axis distance (makes a square island)
                    float v = Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny));

                    map[y, x] = EvaluateFalloffCurve(v);
                }
            }
            return map;
        }

        /// <summary>
        /// Smooth S-curve falloff function. Parameters are taken from the inspector fields.
        /// </summary>
        private float EvaluateFalloffCurve(float v)
        {
            float a = falloffStrength;
            float b = falloffShift;
            float va = Mathf.Pow(v, a);
            float ba = Mathf.Pow(b - b * v, a);
            return va / (va + ba);
        }

        // ── Stepped Terrain ───────────────────────────────────────────────────────────

        /// <summary>
        /// Quantises a normalised height value to discrete steps (terracing).
        /// Smoothness blends linearly between hard steps and the original value.
        /// </summary>
        private float ApplyStep(float value)
        {
            float stepped = Mathf.Round(value * stepCount) / stepCount;
            return Mathf.Lerp(stepped, value, stepSmoothness);
        }

        // ── Splat Zone ────────────────────────────────────────────────────────────────

        private Color HeightToSplatColor(float height)
        {
            if (height < waterHeightThreshold) return waterColor;
            if (height < grassHeightThreshold) return treeColor;
            return rockColor;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // TERRAIN GENERATION
        // ─────────────────────────────────────────────────────────────────────────────

        private void GenerateTerrainFromTexture()
        {
            ClearExistingTerrain();

            TerrainData td = new TerrainData();
            td.heightmapResolution = terrainWidth;
            td.size = new Vector3(terrainWidth, terrainHeightMultiplier, terrainLength);

            float[,] heights = new float[terrainLength, terrainWidth];
            for (int y = 0; y < terrainLength; y++)
            {
                for (int x = 0; x < terrainWidth; x++)
                {
                    float u = (float)x / (terrainWidth  - 1);
                    float v = (float)y / (terrainLength - 1);
                    heights[y, x] = heightMap.GetPixelBilinear(u, v).grayscale;
                }
            }

            td.SetHeights(0, 0, heights);
            Terrain.CreateTerrainGameObject(td).name = "Generated Terrain";
        }

        private void GenerateTerrainFromHeights(float[,] heights)
        {
            ClearExistingTerrain();

            TerrainData td = new TerrainData();
            td.heightmapResolution = terrainWidth;
            td.size = new Vector3(terrainWidth, terrainHeightMultiplier, terrainLength);
            td.SetHeights(0, 0, heights);

            Terrain.CreateTerrainGameObject(td).name = "Generated Terrain (Procedural)";
        }

        private void ClearExistingTerrain()
        {
            Terrain existing = FindFirstObjectByType<Terrain>();
            if (existing != null)
                DestroyImmediate(existing.gameObject);
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // OBJECT PLACEMENT
        // ─────────────────────────────────────────────────────────────────────────────

        private void PlaceObjects(Texture2D splat)
        {
            Terrain terrain = FindFirstObjectByType<Terrain>();
            if (terrain == null)
            {
                EditorUtility.DisplayDialog("Error", "No terrain found. Generate terrain first.", "OK");
                return;
            }

            // Re-use existing holder or create a fresh one
            GameObject holder = GameObject.Find(PlacedObjectsHolderName)
                                ?? new GameObject(PlacedObjectsHolderName);

            int mapW = splat.width;
            int mapH = splat.height;

            Vector3 tSize = terrain.terrainData.size;

            // Water is treated specially: accumulate all water pixel world positions,
            // then place a single object at their centroid XZ and the terrain midpoint Y.
            double waterSumX  = 0;
            double waterSumZ  = 0;
            int    waterCount = 0;

            for (int x = 0; x < mapW; x++)
            {
                for (int y = 0; y < mapH; y++)
                {
                    Color pixel  = splat.GetPixel(x, y);
                    float worldX = (float)x / mapW * tSize.x;
                    float worldZ = (float)y / mapH * tSize.z;

                    if (ColorMatch(pixel, waterColor))
                    {
                        // Always accumulate every water pixel (ignore density for centroid accuracy)
                        waterSumX += worldX;
                        waterSumZ += worldZ;
                        waterCount++;
                        continue;
                    }

                    // Non-water objects respect placement density and slope filters as normal
                    if (Random.value > placementDensity) continue;

                    float worldY = terrain.SampleHeight(new Vector3(worldX, 0f, worldZ));

                    if (minPlacementSlope > 0f || maxPlacementSlope < 90f)
                    {
                        float nx    = worldX / tSize.x;
                        float nz    = worldZ / tSize.z;
                        float slope = terrain.terrainData.GetSteepness(nx, nz);
                        if (slope < minPlacementSlope || slope > maxPlacementSlope) continue;
                    }

                    Vector3 pos = new Vector3(worldX, worldY, worldZ);

                    if      (ColorMatch(pixel, treeColor)) SpawnPrefab(treePrefab, pos, holder.transform, terrain);
                    else if (ColorMatch(pixel, rockColor)) SpawnPrefab(rockPrefab, pos, holder.transform, terrain);
                }
            }

            // Place exactly one water object at the centroid of all water pixels,
            // vertically centred between the terrain's lowest and highest sampled points.
            if (waterCount > 0 && waterPrefab != null)
            {
                float centreX = (float)(waterSumX / waterCount);
                float centreZ = (float)(waterSumZ / waterCount);

                // Find the true min/max world-space heights across the whole terrain
                TerrainData td  = terrain.terrainData;
                int hmRes       = td.heightmapResolution;
                float[,] hmap   = td.GetHeights(0, 0, hmRes, hmRes);

                float minH = float.MaxValue;
                float maxH = float.MinValue;
                for (int hy = 0; hy < hmRes; hy++)
                {
                    for (int hx = 0; hx < hmRes; hx++)
                    {
                        float wh = hmap[hy, hx] * td.size.y;
                        if (wh < minH) minH = wh;
                        if (wh > maxH) maxH = wh;
                    }
                }

                float midY = (minH + maxH) * 0.5f;

                SpawnPrefab(waterPrefab, new Vector3(centreX, midY, centreZ), holder.transform, terrain);
            }
        }

        private void ClearObjects()
        {
            GameObject holder = GameObject.Find(PlacedObjectsHolderName);
            if (holder != null) DestroyImmediate(holder);
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────────────────────

        private bool ColorMatch(Color c1, Color c2)
        {
            float dr = c1.r - c2.r;
            float dg = c1.g - c2.g;
            float db = c1.b - c2.b;
            return (dr * dr + dg * dg + db * db) < objectPlacementThreshold;
        }

        private void SpawnPrefab(GameObject prefab, Vector3 position, Transform parent, Terrain terrain)
        {
            if (prefab == null) return;

            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.position = position;

            // Rotation: align to normal if requested, then add random Y spin
            if (alignToNormal)
            {
                float nx = position.x / terrain.terrainData.size.x;
                float nz = position.z / terrain.terrainData.size.z;
                Vector3 normal = terrain.terrainData.GetInterpolatedNormal(nx, nz);
                Quaternion toNormal = Quaternion.FromToRotation(Vector3.up, normal);
                Quaternion ySpin    = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                obj.transform.rotation = toNormal * ySpin;
            }
            else
            {
                obj.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }

            // Optional random uniform scale
            if (randomScaleObjects)
            {
                float s = Random.Range(minObjectScale, maxObjectScale);
                obj.transform.localScale = Vector3.one * s;
            }

            obj.transform.parent = parent;
        }

        private bool AnyPrefabAssigned() =>
            treePrefab != null || rockPrefab != null || waterPrefab != null;

        // ─────────────────────────────────────────────────────────────────────────────
        // EXPORT
        // ─────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Writes full-resolution PNG files for the generated height map and splat map
        /// into Assets/GeneratedMaps/ and refreshes the AssetDatabase.
        /// </summary>
        private void ExportMapsToAssets()
        {
            const string folder = "Assets/GeneratedMaps";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets", "GeneratedMaps");

            int w = terrainWidth;
            int h = terrainLength;

            // Height map
            Texture2D htex = new Texture2D(w, h, TextureFormat.RGB24, false);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float v = generatedHeights[y, x];
                    htex.SetPixel(x, y, new Color(v, v, v));
                }
            htex.Apply();
            File.WriteAllBytes(folder + "/HeightMap.png", htex.EncodeToPNG());
            DestroyImmediate(htex);

            // Splat map
            Texture2D stex = new Texture2D(w, h, TextureFormat.RGB24, false);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    stex.SetPixel(x, y, HeightToSplatColor(generatedHeights[y, x]));
            stex.Apply();
            File.WriteAllBytes(folder + "/SplatMap.png", stex.EncodeToPNG());
            DestroyImmediate(stex);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Export Complete",
                $"Full-resolution maps saved to {folder}/", "OK");
        }
    }
}