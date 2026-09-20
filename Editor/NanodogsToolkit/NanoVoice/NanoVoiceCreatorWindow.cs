#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Nanodogs.Toolkit.NanoVoice
{
    public sealed class NanoVoiceLineRecorderWindow : NDSEditorWindow
    {
        private const int DefaultSampleRate = 48000;
        private const int MinRecordSeconds = 1;

        // Recording
        private string[] _devices = Array.Empty<string>();
        private int _deviceIndex;
        private int _sampleRate = DefaultSampleRate;
        private int _maxSeconds = 30;
        private bool _loop;

        private AudioClip? _recording;
        private double _recordStartTime;
        private bool _isRecording;

        // Save
        private DefaultAsset? _saveFolder;
        private string _fileName = "VoiceLine_001";
        private bool _autoSelectNewClip = true;

        // Metadata
        private MonoScript? _metadataScript;
        private ScriptableObject? _existingMetadata;
        private bool _createMetadataAsset = true;
        private string _metadataAssetNameSuffix = "_Meta";
        private string _linkAssetNameSuffix = "_Link";

        // Preview
        private AudioClip? _lastImportedClip;

        [MenuItem("Nanodogs/NanoAudio/NanoVoice Recorder")]
        public static void Open()
        {
            GetWindow<NanoVoiceLineRecorderWindow>(false, "NanoVoice Recorder", true);
        }

        private void OnEnable()
        {
            RefreshDevices();
            if (_saveFolder == null)
            {
                _saveFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets");
            }
        }

        private void OnDisable()
        {
            StopRecordingIfNeeded(discard: true);
        }

        private void RefreshDevices()
        {
            _devices = Microphone.devices ?? Array.Empty<string>();
            if (_devices.Length == 0)
            {
                _deviceIndex = 0;
            }
            else
            {
                _deviceIndex = Mathf.Clamp(_deviceIndex, 0, _devices.Length - 1);
            }
        }

        private new void OnGUI()
        {
            EditorGUILayout.Space(6);
            DrawHeader();
            EditorGUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawDeviceSection();
            }

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawRecordSection();
            }

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawSaveSection();
            }

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawMetadataSection();
            }

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawLinkToolsSection();
            }

            EditorGUILayout.Space(10);

            DrawFooter();

            base.OnGUI();

            // Repaint while recording so UI updates (timer)
            if (_isRecording)
                Repaint();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("NanoVoice Recorder", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Record from a microphone input and save as WAV into your project.", EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawDeviceSection()
        {
            EditorGUILayout.LabelField("Input Device", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (_devices.Length == 0)
                {
                    EditorGUILayout.HelpBox("No microphone devices detected by Unity.", MessageType.Warning);
                }
                else
                {
                    _deviceIndex = EditorGUILayout.Popup("Device", _deviceIndex, _devices);
                }

                if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                {
                    RefreshDevices();
                }
            }

            _sampleRate = EditorGUILayout.IntField(new GUIContent("Sample Rate", "Common values: 44100, 48000"), _sampleRate);
            if (_sampleRate < 8000) _sampleRate = 8000;
            if (_sampleRate > 192000) _sampleRate = 192000;

            _maxSeconds = EditorGUILayout.IntSlider(new GUIContent("Max Seconds"), _maxSeconds, 1, 180);
            _loop = EditorGUILayout.Toggle(new GUIContent("Loop Buffer", "If enabled, the mic buffer loops and you can stop whenever."), _loop);
        }

        private void DrawRecordSection()
        {
            EditorGUILayout.LabelField("Recording", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = !_isRecording && _devices.Length > 0;
                if (GUILayout.Button("Start Recording", GUILayout.Height(28)))
                {
                    StartRecording();
                }

                GUI.enabled = _isRecording;
                if (GUILayout.Button("Stop", GUILayout.Height(28), GUILayout.Width(100)))
                {
                    StopRecordingIfNeeded(discard: false);
                }

                GUI.enabled = true;
            }

            if (_isRecording)
            {
                double elapsed = EditorApplication.timeSinceStartup - _recordStartTime;
                EditorGUILayout.LabelField($"Status: Recording… ({elapsed:0.0}s)");
            }
            else if (_recording != null)
            {
                EditorGUILayout.LabelField($"Status: Ready to save (clip length: {_recording.length:0.00}s)");
                EditorGUILayout.ObjectField("Preview", _recording, typeof(AudioClip), false);
            }
            else
            {
                EditorGUILayout.LabelField("Status: Idle");
            }
        }

        private void DrawSaveSection()
        {
            EditorGUILayout.LabelField("Save", EditorStyles.boldLabel);

            _saveFolder = (DefaultAsset?)EditorGUILayout.ObjectField(
                new GUIContent("Folder", "Must be under Assets/"),
                _saveFolder,
                typeof(DefaultAsset),
                false);

            _fileName = EditorGUILayout.TextField(new GUIContent("File Name", "WAV file name (without extension)"), _fileName);
            _autoSelectNewClip = EditorGUILayout.Toggle("Auto-select new clip", _autoSelectNewClip);

            GUI.enabled = _recording != null && !_isRecording;
            if (GUILayout.Button("Save Recording as WAV", GUILayout.Height(28)))
            {
                SaveRecordingAsWav();
            }
            GUI.enabled = true;

            if (_lastImportedClip != null)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Last Imported", EditorStyles.miniBoldLabel);
                EditorGUILayout.ObjectField(_lastImportedClip, typeof(AudioClip), false);
            }
        }

        private void DrawMetadataSection()
        {
            EditorGUILayout.LabelField("Metadata (ScriptableObject)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "You can attach ANY ScriptableObject as metadata.\n" +
                "This tool creates/uses a companion asset (VoiceLineClipLink) to pair an AudioClip with metadata.",
                MessageType.Info);

            _createMetadataAsset = EditorGUILayout.Toggle(new GUIContent("Create new metadata asset"), _createMetadataAsset);

            using (new EditorGUI.DisabledScope(!_createMetadataAsset))
            {
                _metadataScript = (MonoScript?)EditorGUILayout.ObjectField(
                    new GUIContent("Metadata Script", "Drag a ScriptableObject script here (your custom type)."),
                    _metadataScript,
                    typeof(MonoScript),
                    false);
                _metadataAssetNameSuffix = EditorGUILayout.TextField("Metadata name suffix", _metadataAssetNameSuffix);
            }

            using (new EditorGUI.DisabledScope(_createMetadataAsset))
            {
                _existingMetadata = (ScriptableObject?)EditorGUILayout.ObjectField(
                    new GUIContent("Existing Metadata", "Use an existing metadata asset."),
                    _existingMetadata,
                    typeof(ScriptableObject),
                    false);
            }

            _linkAssetNameSuffix = EditorGUILayout.TextField("Link name suffix", _linkAssetNameSuffix);
        }

        private void DrawLinkToolsSection()
        {
            EditorGUILayout.LabelField("Attach / Find Links", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = _lastImportedClip != null;
                if (GUILayout.Button("Create/Update Link for Last Imported", GUILayout.Height(24)))
                {
                    if (_lastImportedClip != null)
                        CreateOrUpdateLinkForClip(_lastImportedClip);
                }
                GUI.enabled = true;

                if (GUILayout.Button("Select Link(s) For Selected Clip", GUILayout.Height(24)))
                {
                    var selectedClip = Selection.activeObject as AudioClip;
                    if (selectedClip == null)
                    {
                        EditorUtility.DisplayDialog("Select an AudioClip", "Please select an AudioClip in the Project window.", "OK");
                    }
                    else
                    {
                        SelectLinksForClip(selectedClip);
                    }
                }
            }
        }

        private void DrawFooter()
        {
            EditorGUILayout.LabelField("Tip", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "For best results: set your AudioClip import settings (Load Type/Compression) after import.\n" +
                "You can also extend VoiceLineClipLink to add fields like localization keys, subtitles, tags, etc.",
                EditorStyles.wordWrappedMiniLabel);
        }

        private string? CurrentDeviceOrNull()
        {
            if (_devices.Length == 0) return null;
            _deviceIndex = Mathf.Clamp(_deviceIndex, 0, _devices.Length - 1);
            return _devices[_deviceIndex];
        }

        private void StartRecording()
        {
            string? device = CurrentDeviceOrNull();
            if (string.IsNullOrEmpty(device))
            {
                EditorUtility.DisplayDialog("No device", "No microphone device selected or detected.", "OK");
                return;
            }

            StopRecordingIfNeeded(discard: true);

            // Microphone.Start creates an AudioClip with a fixed length buffer.
            _recording = Microphone.Start(device, _loop, _maxSeconds, _sampleRate);
            _recordStartTime = EditorApplication.timeSinceStartup;
            _isRecording = true;

            if (_recording == null)
            {
                _isRecording = false;
                EditorUtility.DisplayDialog("Microphone error", "Microphone.Start returned null.", "OK");
            }
        }

        private void StopRecordingIfNeeded(bool discard)
        {
            if (!_isRecording && _recording == null) return;

            string? device = CurrentDeviceOrNull();
            if (!string.IsNullOrEmpty(device) && Microphone.IsRecording(device))
            {
                int position = Microphone.GetPosition(device);
                Microphone.End(device);

                if (!discard && _recording != null)
                {
                    // Trim the recording to the actual mic position if loop is off.
                    // If loop is on, position may wrap; trimming is trickier. We'll still attempt to keep up to current position.
                    int samples = Mathf.Max(position, 0);
                    if (samples > 0)
                    {
                        _recording = AudioClipTrimmer.Trim(_recording, samples);
                    }
                }
            }

            if (discard)
            {
                _recording = null;
            }

            _isRecording = false;
        }

        private void SaveRecordingAsWav()
        {
            if (_recording == null)
            {
                EditorUtility.DisplayDialog("Nothing to save", "Record something first.", "OK");
                return;
            }

            // Guard: minimum length
            if (_recording.length < MinRecordSeconds)
            {
                if (!EditorUtility.DisplayDialog("Very short clip", "This recording is very short. Save anyway?", "Save", "Cancel"))
                    return;
            }

            string folderPath = AssetDatabase.GetAssetPath(_saveFolder);
            if (string.IsNullOrEmpty(folderPath) || !folderPath.StartsWith("Assets", StringComparison.Ordinal))
            {
                EditorUtility.DisplayDialog("Invalid folder", "Please choose a folder under Assets/", "OK");
                return;
            }

            string safeName = MakeFileNameSafe(_fileName);
            if (string.IsNullOrWhiteSpace(safeName)) safeName = "VoiceLine";

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{safeName}.wav");
            string fullPath = Path.GetFullPath(assetPath);

            try
            {
                WavUtility.SaveWav(fullPath, _recording);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Save failed", $"Failed to write WAV:\n{ex.Message}", "OK");
                return;
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.Refresh();

            var imported = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (imported == null)
            {
                EditorUtility.DisplayDialog("Import failed", "WAV was saved but Unity did not import it as an AudioClip.", "OK");
                return;
            }

            _lastImportedClip = imported;

            if (_autoSelectNewClip)
            {
                Selection.activeObject = imported;
                EditorGUIUtility.PingObject(imported);
            }

            // Optionally create/update a link asset immediately.
            CreateOrUpdateLinkForClip(imported);
        }

        private void CreateOrUpdateLinkForClip(AudioClip clip)
        {
            // Decide metadata instance
            ScriptableObject? metadata = null;
            if (_createMetadataAsset)
            {
                var soType = GetScriptableObjectTypeFromScript(_metadataScript);
                if (soType == null)
                {
                    // Not an error; user might only want a plain link
                    metadata = null;
                }
                else
                {
                    string clipPath = AssetDatabase.GetAssetPath(clip);
                    string dir = Path.GetDirectoryName(clipPath)?.Replace('\\', '/') ?? "Assets";
                    string metaName = Path.GetFileNameWithoutExtension(clipPath) + _metadataAssetNameSuffix + ".asset";
                    string metaPath = AssetDatabase.GenerateUniqueAssetPath($"{dir}/{metaName}");

                    metadata = ScriptableObject.CreateInstance(soType);
                    AssetDatabase.CreateAsset(metadata, metaPath);
                    AssetDatabase.SaveAssets();
                }
            }
            else
            {
                metadata = _existingMetadata;
            }

            // Create or find an existing link asset in the same folder.
            string audioPath = AssetDatabase.GetAssetPath(clip);
            string audioDir = Path.GetDirectoryName(audioPath)?.Replace('\\', '/') ?? "Assets";
            string linkName = Path.GetFileNameWithoutExtension(audioPath) + _linkAssetNameSuffix + ".asset";
            string desiredLinkPath = $"{audioDir}/{linkName}";

            NanoVoiceLineClipLink? link = AssetDatabase.LoadAssetAtPath<NanoVoiceLineClipLink>(desiredLinkPath);
            if (link == null)
            {
                // If not found at the desired name, see if any link already references this clip.
                link = FindLinkForClip(clip);
            }

            if (link == null)
            {
                string linkPath = AssetDatabase.GenerateUniqueAssetPath(desiredLinkPath);
                link = CreateInstance<NanoVoiceLineClipLink>();
                link.clip = clip;
                link.metadata = metadata;
                AssetDatabase.CreateAsset(link, linkPath);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Undo.RecordObject(link, "Update VoiceLineClipLink");
                link.clip = clip;
                link.metadata = metadata;
                EditorUtility.SetDirty(link);
                AssetDatabase.SaveAssets();
            }

            // Select created assets if any
            if (metadata != null)
            {
                Selection.objects = new UnityEngine.Object[] { clip, link, metadata };
            }
            else
            {
                Selection.objects = new UnityEngine.Object[] { clip, link };
            }

            EditorGUIUtility.PingObject(link);
        }

        private static NanoVoiceLineClipLink? FindLinkForClip(AudioClip clip)
        {
            string clipGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clip));
            string[] linkGuids = AssetDatabase.FindAssets("t:VoiceLineClipLink");

            foreach (string guid in linkGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var link = AssetDatabase.LoadAssetAtPath<NanoVoiceLineClipLink>(path);
                if (link == null || link.clip == null) continue;

                string linkedGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(link.clip));
                if (linkedGuid == clipGuid)
                    return link;
            }

            return null;
        }

        private static void SelectLinksForClip(AudioClip clip)
        {
            string clipGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clip));
            string[] linkGuids = AssetDatabase.FindAssets("t:VoiceLineClipLink");

            var matches = linkGuids
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Select(p => AssetDatabase.LoadAssetAtPath<NanoVoiceLineClipLink>(p))
                .Where(l => l != null && l.clip != null)
                .Where(l => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(l!.clip!)) == clipGuid)
                .Cast<UnityEngine.Object>()
                .ToArray();

            if (matches.Length == 0)
            {
                EditorUtility.DisplayDialog("No links found", "No VoiceLineClipLink assets reference the selected clip.", "OK");
                return;
            }

            Selection.objects = matches;
            EditorGUIUtility.PingObject(matches[0]);
        }

        private static Type? GetScriptableObjectTypeFromScript(MonoScript? script)
        {
            if (script == null) return null;

            var t = script.GetClass();
            if (t == null) return null;
            if (!typeof(ScriptableObject).IsAssignableFrom(t))
            {
                EditorUtility.DisplayDialog("Not a ScriptableObject", "The selected script must define a ScriptableObject type.", "OK");
                return null;
            }
            if (t.IsAbstract)
            {
                EditorUtility.DisplayDialog("Abstract type", "The selected ScriptableObject type is abstract and cannot be instantiated.", "OK");
                return null;
            }

            return t;
        }

        private static string MakeFileNameSafe(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c.ToString(), "_");
            return name.Trim();
        }
    }


    /// <summary>
    /// Companion asset that pairs an AudioClip with any ScriptableObject metadata.
    /// Extend this class if you want more explicit fields (subtitle text, localization key, tags, etc.).
    /// </summary>
    public sealed class NanoVoiceLineClipLink : ScriptableObject
    {
        public AudioClip? clip;
        public ScriptableObject? metadata;
    }

    /// <summary>
    /// Utility to trim an AudioClip to a specific sample length (per channel).
    /// </summary>
    internal static class AudioClipTrimmer
    {
        public static AudioClip Trim(AudioClip source, int sampleFrames)
        {
            sampleFrames = Mathf.Clamp(sampleFrames, 0, source.samples);
            int channels = source.channels;
            float[] data = new float[sampleFrames * channels];
            source.GetData(data, 0);

            AudioClip trimmed = AudioClip.Create(source.name + "_trim", sampleFrames, channels, source.frequency, false);
            trimmed.SetData(data, 0);
            return trimmed;
        }
    }

    /// <summary>
    /// Minimal WAV writer (16-bit PCM little-endian).
    /// </summary>
    internal static class WavUtility
    {
        // 16-bit PCM
        private const ushort BitsPerSample = 16;

        public static void SaveWav(string fullPath, AudioClip clip)
        {
            if (clip == null) throw new ArgumentNullException(nameof(clip));

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? ".");

            int channels = clip.channels;
            int frequency = clip.frequency;
            int sampleFrames = clip.samples;

            float[] samples = new float[sampleFrames * channels];
            clip.GetData(samples, 0);

            byte[] pcm = ConvertToPcm16(samples);

            using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            // RIFF header
            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + pcm.Length); // file size minus 8
            bw.Write(Encoding.ASCII.GetBytes("WAVE"));

            // fmt chunk
            bw.Write(Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16); // PCM chunk size
            bw.Write((ushort)1); // audio format (1 = PCM)
            bw.Write((ushort)channels);
            bw.Write(frequency);

            int byteRate = frequency * channels * (BitsPerSample / 8);
            ushort blockAlign = (ushort)(channels * (BitsPerSample / 8));

            bw.Write(byteRate);
            bw.Write(blockAlign);
            bw.Write(BitsPerSample);

            // data chunk
            bw.Write(Encoding.ASCII.GetBytes("data"));
            bw.Write(pcm.Length);
            bw.Write(pcm);

            bw.Flush();
        }

        private static byte[] ConvertToPcm16(float[] samples)
        {
            // Clamp [-1, 1] and scale to Int16
            byte[] bytes = new byte[samples.Length * 2];
            int offset = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                float s = Mathf.Clamp(samples[i], -1f, 1f);
                short v = (short)Mathf.RoundToInt(s * short.MaxValue);
                bytes[offset++] = (byte)(v & 0xFF);
                bytes[offset++] = (byte)((v >> 8) & 0xFF);
            }
            return bytes;
        }
    }

}