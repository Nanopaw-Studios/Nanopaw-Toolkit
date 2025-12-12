using UnityEngine;

namespace NanodogsToolkit.NanoVoice
{
    /// <summary>
    /// Represents a single line of voice data in the NanoVoice system.
    /// </summary>
    public class NanoVoiceLine : ScriptableObject
    {
        public string lineID;               // Unique identifier for the voice line
        public AudioClip voiceLine;        // The audio clip associated with this voice line

        [Header("Other")]
        public string performerName;     // Name of the performer
        public string captions;         // Captions or subtitles for the voice line
    }
}
