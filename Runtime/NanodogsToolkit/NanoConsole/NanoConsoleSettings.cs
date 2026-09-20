using UnityEngine;

namespace Nanodogs.Console
{
    public class NanoConsoleSettings : ScriptableObject
    {
        public bool enableNanoConsole = true;
        public KeyCode toggleKey = KeyCode.BackQuote;
        public bool enableInEditor = true;
        public bool enableInBuild = false;
        public bool enableLogs = true;
        public bool enableWarnings = true;
        public bool enableErrors = true;
        public int maxLogs = 1000;
        public bool collapseLogs = false;
        public bool showTimestamps = false;
        public bool showStackTrace = false;
        public Color backgroundColor = new Color(0f, 0f, 0f, 0.75f);
        public Color logColor = Color.white;
        public Color warningColor = Color.yellow;
        public Color errorColor = Color.red;
    }
}