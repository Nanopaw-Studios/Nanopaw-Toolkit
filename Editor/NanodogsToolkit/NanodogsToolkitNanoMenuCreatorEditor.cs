// © 2025 Nanodogs Studios. All rights reserved.

using Nanodogs.Toolkit.NanoMenu;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Nanodogs.Toolkit
{
    /// <summary>
    /// Nanodogs Toolkit NanoMenu Creator Editor Script
    /// </summary>
    public class NanodogsToolkitNanoMenuCreatorEditor : NDSEditorWindow
    {
        private string gameName = "a game";
        public Sprite gameLogo;
        public TMP_FontAsset menuFont;
        public Color themeColor = Color.red;
        public string[] credits = {
            "!Credits for this title",
            "Lead Dev - ",
            "Writer - ",
            "Music - ",
            "!Nanodogs Team",
            "Founder - itslaymxd",
            "Co-Founder - Prospect",
            "Ext. Help - YykVR",
            "Reviewer - Catzinho49",
            "Voice Actor/Reviewer - Tyler",
        };

        // cright
        public string customPrefix = "© Nanodogs Studios";
        public string Seperator = " | ";
        public bool showAppVersion = true;
        public bool showUnityVersion = true;
        public string CustomSuffix = "";

        [MenuItem("Nanodogs/Tools/Utilites/NanoMenu Creation", false, 1)]
        public static void ShowWindow()
        {
            GetWindow<NanodogsToolkitNanoMenuCreatorEditor>("NanoMenu Creator");
        }

        new private void OnGUI()
        {
            GUILayout.Label("NanoMenu Creator", EditorStyles.boldLabel);
            GUILayout.Label("Create and customize Menus for your game.", EditorStyles.label);
            GUILayout.Space(10);

            // GUI fields
            EditorGUILayout.LabelField("Basic Info", EditorStyles.boldLabel);
            GUILayout.Space(5);
            gameName = EditorGUILayout.TextField("Game Name", gameName);
            gameLogo = (Sprite)EditorGUILayout.ObjectField("Game Logo", gameLogo, typeof(Sprite), false);
            menuFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Menu Font", menuFont, typeof(TMP_FontAsset), false);
            //themeColor = EditorGUILayout.ColorField("Theme Color", themeColor);
            ScriptableObject scriptableObj = this;
            SerializedObject serialObj = new SerializedObject(scriptableObj);
            SerializedProperty serialProp = serialObj.FindProperty("credits");
            EditorGUILayout.LabelField("Credits (one per line). add ! at the start of a line for a title line");
            EditorGUILayout.PropertyField(serialProp, true);
            serialObj.ApplyModifiedProperties();

            GUILayout.Space(10);
            if (GUILayout.Button("Create Menu in current scene"))
            {
                CreateMenu();
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Other - CRight Text", EditorStyles.boldLabel);
            GUILayout.Space(5);
            customPrefix = EditorGUILayout.TextField("Custom Prefix", customPrefix);
            Seperator = EditorGUILayout.TextField("Separator", Seperator);
            showAppVersion = EditorGUILayout.Toggle("Show App Version", showAppVersion);
            showUnityVersion = EditorGUILayout.Toggle("Show Unity Version", showUnityVersion);
            CustomSuffix = EditorGUILayout.TextField("Custom Suffix", CustomSuffix);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Your game's scenes should be named 01, 02, 03, 04, etc.");

            base.OnGUI();
        }

        private void CreateMenu()
        {
            // Load base prefab
            string basePrefabPath = Path.Combine(NanoPath.PrefabsAssetPath, "NanoMenu.prefab").Replace("\\", "/");
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);

            if (basePrefab == null)
            {
                Debug.LogError($"NanoMenu prefab not found at path: {basePrefabPath}");
                return;
            }

            GameObject sceneInstance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
            sceneInstance.transform.position = Vector3.zero;
            sceneInstance.name = "NanoMenu";
            Undo.RegisterCreatedObjectUndo(sceneInstance, "Spawned Menu in Scene");

            // Customize menu
            //sceneInstance.transform.GetComponent<MainMenu>().ThemeColor = themeColor;
            var main = sceneInstance.transform.Find("Main");
            main.Find("Logo").GetComponent<Image>().sprite = gameLogo;

            var credits = main.Find("CreditsButtons");
            var realBackButton = credits.Find("Back");

            // --- COLLECT OBJECTS TO DESTROY (old credit lines etc.) ---
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (Transform child in credits)
            {
                // Skip the real Back button
                if (child == realBackButton)
                    continue;

                toDestroy.Add(child.gameObject);
            }

            foreach (var obj in toDestroy)
            {
                GameObject.DestroyImmediate(obj);
            }

            // --- RECREATE CREDIT LINES ---
            foreach (var line in this.credits)
            {
                var creditLine = GameObject.Instantiate(realBackButton.gameObject, credits);
                creditLine.name = "CreditLine";

                // Remove button so it isn't clickable
                var button = creditLine.GetComponent<Button>();
                if (button != null)
                    GameObject.DestroyImmediate(button);

                // ---- FIND OR CREATE TMP CHILD ----
                TMP_Text textComp = creditLine.GetComponentInChildren<TMP_Text>();

                if (textComp == null)
                {
                    // If Back button had no TMP child (or you want a fresh one), create one
                    var textGO = new GameObject("Text", typeof(RectTransform));
                    textGO.transform.SetParent(creditLine.transform, false);

                    textComp = textGO.AddComponent<TextMeshProUGUI>();

                    // Make it stretch full area
                    var rt = (RectTransform)textGO.transform;
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }

                // Fix shit
                DestroyImmediate(creditLine.GetComponent<Animator>());
                DestroyImmediate(creditLine.GetComponent<HoverActionButton>());

                // ---- SET TEXT ----
                string content = line;
                bool isHeader = false;

                if (content.StartsWith("!"))
                {
                    isHeader = true;
                    content = content.Substring(1);
                }

                textComp.text = content;
                textComp.font = menuFont;
                textComp.alignment = TextAlignmentOptions.MidlineRight;
                textComp.fontStyle = isHeader ? FontStyles.Bold : FontStyles.Normal;
                textComp.enableAutoSizing = true;

                // If header, tweak background color (parent Image)
                if (isHeader)
                {
                    textComp.enableAutoSizing = false;
                    textComp.fontSize = 32;
                    textComp.fontStyle = FontStyles.Bold;
                    var img = creditLine.GetComponent<Image>();
                    if (img != null)
                    {
                        var c = img.color;
                        c.a = 1f; // 0–1, not 0–255
                        img.color = c;
                    }
                }
            }

            // Ensure all TMP text uses correct font
            foreach (var textComponent in main.GetComponentsInChildren<TMP_Text>())
            {
                textComponent.font = menuFont;
            }

            // set as last sibling to keep back button at bottom
            realBackButton.SetSiblingIndex(credits.childCount - 1);

            // Customize CRight Text
            var cRight = main.Find("CRightText").GetComponent<CRightText>();
            cRight.CustomPrefix = customPrefix;
            cRight.Separator = Seperator;
            cRight.ShowAppVersion = showAppVersion;
            cRight.ShowUnityVersion = showUnityVersion;
            cRight.CustomSuffix = CustomSuffix;

            Debug.Log("NanoMenu created in scene. Customize further as needed!");
        }
    }
}
