using TMPro;
using UnityEngine;

namespace Nanodogs.Toolkit.NanoMenu
{
    /// <summary>
    /// Component to set the right-aligned text in the NanoMenu
    /// </summary>
    public class CRightText : MonoBehaviour
    {
        [Header("Customization")]
        [Tooltip("Custom prefix text before the generated info.")]
        public string CustomPrefix = "© Nanodogs Studios";

        [Tooltip("Separator between each info field.")]
        public string Separator = " | ";

        [Tooltip("Show the app version (Application.version)")]
        public bool ShowAppVersion = true;

        [Tooltip("Show the Unity version (Application.unityVersion)")]
        public bool ShowUnityVersion = true;

        [Tooltip("Custom extra text appended at the end.")]
        public string CustomSuffix = "";

        private TMP_Text text;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
            if (text != null)
            {
                text.text = BuildText();
            }
        }

        private string BuildText()
        {
            System.Collections.Generic.List<string> parts = new();

            if (!string.IsNullOrEmpty(CustomPrefix))
                parts.Add(CustomPrefix);

            if (ShowAppVersion)
                parts.Add("v" + Application.version);

            if (ShowUnityVersion)
                parts.Add("Unity " + Application.unityVersion);

            if (!string.IsNullOrEmpty(CustomSuffix))
                parts.Add(CustomSuffix);

            return string.Join(Separator, parts);
        }
    }
}
