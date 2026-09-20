using Nanodogs.API.NanoHealth;
using Nanodogs.API.Nanosaves;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nanodogs.UniversalScripts
{
    /// <summary>
    /// A simple debug HUD that displays information dynamically.
    /// </summary>
    public class DebugHUD : MonoBehaviour
    {
        public int startX = 10;
        public int startY = 10;
        public int lineHeight = 20;
        public int labelWidth = 400;

        public KeyCode toggleKey = KeyCode.F1;
        bool isVisible = true;

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                isVisible = !isVisible;
            }
        }

        private void OnGUI()
        {
            if (!isVisible) return;

            // Build the debug info list
            var debugInfo = new System.Collections.Generic.List<string>
            {
                "-- NDS Debug HUD -- (" + toggleKey + " To Hide)",
                "FPS: " + (1.0f / Time.deltaTime).ToString("F2"),
                "Screen Resolution: " + Screen.currentResolution.width + "x" + Screen.currentResolution.height,
                "Platform: " + Application.platform,
                "Unity Version: " + Application.unityVersion,
                "Build Type: " + (Debug.isDebugBuild ? "Development" : "Release"),
                "Time Since Startup: " + Time.realtimeSinceStartup.ToString("F2") + "s",
                "Memory Usage: " + (System.GC.GetTotalMemory(false) / (1024 * 1024)).ToString("F2") + " MB",
                "Quality Level: " + QualitySettings.names[QualitySettings.GetQualityLevel()],
                "VSync Count: " + QualitySettings.vSyncCount,
                "Active Scene: " + SceneManager.GetActiveScene().name,
                "Is Mobile Platform: " + Application.isMobilePlatform,
                "Is Console Platform: " + Application.isConsolePlatform,
                "Is Editor: " + Application.isEditor,
                "Screen Orientation: " + Screen.orientation,
                "Screen DPI: " + Screen.dpi.ToString("F2"),
                "Screen Fullscreen: " + Screen.fullScreen,
                "Processor Type: " + SystemInfo.processorType,
                "Graphics Device Name: " + SystemInfo.graphicsDeviceName,
                "Graphics Memory Size: " + SystemInfo.graphicsMemorySize + " MB",
                "System Memory Size: " + SystemInfo.systemMemorySize + " MB",
                "Device Name: " + SystemInfo.deviceName,
                " ",
                "-- Other Info: NanoPooler --",
                " ",
                "Pooled Object Types: " + Nanodogs.API.NanoPooling.NanoPooler.poolDictionary.Count,
                "Total Pooled Objects: " + System.Linq.Enumerable.Sum(Nanodogs.API.NanoPooling.NanoPooler.poolDictionary.Values, q => q.Count),
                "Total Unique Pool Keys: " + Nanodogs.API.NanoPooling.NanoPooler.poolLookup.Count,
                " ",
                "-- Other Info: NanoPlayer --",
                " ",
                "NanoPlayer Instance Null?: " + (NanoPlayerSpawnpoint.GetPlayer() == null),
                "NanoPlayer Position: " + (NanoPlayerSpawnpoint.GetPlayer() != null ? NanoPlayerSpawnpoint.GetPlayer().transform.position.ToString() : "N/A"),
                "NanoPlayer ActiveInHierarchy: " + (NanoPlayerSpawnpoint.GetPlayer() != null ? NanoPlayerSpawnpoint.GetPlayer().activeInHierarchy.ToString() : "N/A"),
                "NanoPlayer Tag: " + (NanoPlayerSpawnpoint.GetPlayer() != null ? NanoPlayerSpawnpoint.GetPlayer().tag : "N/A"),
                "NanoPlayer Name: " + (NanoPlayerSpawnpoint.GetPlayer() != null ? NanoPlayerSpawnpoint.GetPlayer().name : "N/A"),
                "NanoPlayer Has Rigidbody: " + (NanoPlayerSpawnpoint.GetPlayer() != null ? (NanoPlayerSpawnpoint.GetPlayer().GetComponent<Rigidbody>() != null).ToString() : "N/A"),
                "NanoPlayer RB linearVelocity: " + (NanoPlayerSpawnpoint.GetPlayer() != null && NanoPlayerSpawnpoint.GetPlayer().GetComponent<Rigidbody>() != null ? NanoPlayerSpawnpoint.GetPlayer().GetComponent<Rigidbody>().linearVelocity.ToString() : "N/A"),
                "NanoPlayer RB angularVelocity: " + (NanoPlayerSpawnpoint.GetPlayer() != null && NanoPlayerSpawnpoint.GetPlayer().GetComponent<Rigidbody>() != null ? NanoPlayerSpawnpoint.GetPlayer().GetComponent<Rigidbody>().angularVelocity.ToString() : "N/A"),
                "NanoPlayer RB isKinematic: " + (NanoPlayerSpawnpoint.GetPlayer() != null && NanoPlayerSpawnpoint.GetPlayer().GetComponent<Rigidbody>() != null ? NanoPlayerSpawnpoint.GetPlayer().GetComponent<Rigidbody>().isKinematic.ToString() : "N/A"),
                "NanoPlayer RB useGravity: " + (NanoPlayerSpawnpoint.GetPlayer() != null && NanoPlayerSpawnpoint.GetPlayer().GetComponent<Rigidbody>() != null ? NanoPlayerSpawnpoint.GetPlayer().GetComponent<Rigidbody>().useGravity.ToString() : "N/A"),
                //" ",
               // "-- Other Info: NanoHealth --",
                //" ",
                //"NanoHealth Instance Null?: " + (NanoPlayerSpawnpoint.GetPlayer() != null && NanoPlayerSpawnpoint.GetPlayer().GetComponent<NanoHealthData>() != null ? "False" : "True"),
                //"NanoHealth Current Health: " + (NanoPlayerSpawnpoint.GetPlayer() != null && NanoPlayerSpawnpoint.GetPlayer().GetComponent<NanoHealthData>() != null ? NanoPlayerSpawnpoint.GetPlayer().GetComponent<NanoHealthData>().currentHealth.ToString() : "N/A"),
                //"NanoHealth Max Health: " + (NanoPlayerSpawnpoint.GetPlayer() != null && NanoPlayerSpawnpoint.GetPlayer().GetComponent<NanoHealthData>() != null ? NanoPlayerSpawnpoint.GetPlayer().GetComponent<NanoHealthData>().maxHealth.ToString() : "N/A"),
                //"NanoHealth Percentage: " + (NanoPlayerSpawnpoint.GetPlayer() != null && NanoPlayerSpawnpoint.GetPlayer().GetComponent<NanoHealthData>() != null ? (NanoHealth.GetHealthPercentage(NanoPlayerSpawnpoint.GetPlayer().GetComponent<NanoHealthData>()) * 100).ToString("F2") + "%" : "N/A"),
                //"NanoHealth Is Alive: " + (NanoPlayerSpawnpoint.GetPlayer() != null && NanoPlayerSpawnpoint.GetPlayer().GetComponent<NanoHealthData>() != null ? NanoPlayerSpawnpoint.GetPlayer().GetComponent<NanoHealthData>().isAlive.ToString() : "N/A"),
                //"NanoHealth State: " + (NanoPlayerSpawnpoint.GetPlayer() != null && NanoPlayerSpawnpoint.GetPlayer().GetComponent<NanoHealthData>() != null ? NanoPlayerSpawnpoint.GetPlayer().GetComponent<NanoHealthData>().state.ToString() : "N/A"),
                " ",
                "-- End of Debug Info --"
            };

            // Loop through and draw labels dynamically
            for (int i = 0; i < debugInfo.Count; i++)
            {
                GUI.Label(new Rect(startX, startY + (i * lineHeight), labelWidth, lineHeight), debugInfo[i]);
            }
        }
    }
}
