using UnityEngine;
using System.Collections.Generic;

namespace Nanodogs.Console
{
    public class NanoConsole : MonoBehaviour
    {
        public static NanoConsoleSettings settings;

        private static List<LogEntry> logEntries = new List<LogEntry>();
        private Vector2 scrollPosition;
        private string inputText = "";
        private bool isConsoleVisible = false;

        private class LogEntry
        {
            public string Message;
            public LogType Type;
        }

        private void Awake()
        {
            // Load settings asset
            if (settings == null)
            {
                settings = Resources.Load<NanoConsoleSettings>("NanoConsoleSettings");
                if (settings == null)
                {
                    Debug.LogError("NanoConsoleSettings asset not found in Resources folder, ensure it is named correctly.");
                    return;
                }
            }

            // Subscribe to Unity's log callback (optional, lets us see all logs)
            Application.logMessageReceived += HandleUnityLog;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleUnityLog;
        }

        private void Update()
        {
            if (Input.GetKeyDown(settings.toggleKey))
            {
                isConsoleVisible = !isConsoleVisible;
            }

            if(isConsoleVisible)
            {
                Time.timeScale = 0; // Pause the game when console is open
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Time.timeScale = 1;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void OnGUI()
        {
            if (!isConsoleVisible) return;

            // limit logs
            if (logEntries.Count > settings.maxLogs)
            {
                logEntries.RemoveRange(0, logEntries.Count - settings.maxLogs);
            }

            GUI.backgroundColor = settings.backgroundColor;

            float consoleHeight = Screen.height / 2;
            GUI.Box(new Rect(10, 10, Screen.width - 20, consoleHeight), "Nano Console");

            GUILayout.BeginArea(new Rect(20, 40, Screen.width - 40, consoleHeight - 60));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (var entry in logEntries)
            {
                GUI.color = entry.Type switch
                {
                    LogType.Warning => settings.warningColor,
                    LogType.Error => settings.errorColor,
                    LogType.Exception => Color.magenta,
                    _ => settings.logColor,
                };
                GUILayout.Label(entry.Message);
            }

            GUI.color = Color.white;
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            // Input field
            GUILayout.BeginArea(new Rect(20, consoleHeight - 10 + 40, Screen.width - 40, 30));
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("ConsoleInput");
            inputText = GUILayout.TextField(inputText, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Enter", GUILayout.Width(80)))
            {
                ExecuteCommand(inputText);
                inputText = "";
                GUI.FocusControl("ConsoleInput");
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        // Command handling
        private void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;

            Log("> " + command);

            // Simple built-in commands
            if (command.Equals("clear", System.StringComparison.OrdinalIgnoreCase))
            {
                logEntries.Clear();
                return;
            }

            if (command.Equals("help", System.StringComparison.OrdinalIgnoreCase))
            {
                Log("Available commands:\n - clear\n - help");
                return;
            }

            // Add your own commands here!
            LogWarning("Unknown command: " + command);
        }

        // Static logging methods
        public static void Log(object message) => AddLog(message, LogType.Log);
        public static void LogWarning(object message) => AddLog(message, LogType.Warning);
        public static void LogError(object message) => AddLog(message, LogType.Error);

        private static void AddLog(object message, LogType type)
        {
            logEntries.Add(new LogEntry { Message = message.ToString(), Type = type });
        }

        // Hook into Unity's built-in Debug.Log
        private void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            logEntries.Add(new LogEntry { Message = logString, Type = type });
        }
    }
}
