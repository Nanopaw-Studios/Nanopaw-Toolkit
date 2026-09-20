using UnityEngine;

namespace Nanodogs.API.NanoMusic
{
    [CreateAssetMenu(fileName = "New Nano Music Asset", menuName = "Nanodogs/Nano Music Asset", order = 1)]
    public class NanoMusicAsset : ScriptableObject
    {
        public NanoSong songData;
        public AudioClip musicClip;
        public bool loop = true;
        public float volume = 1.0f;
        public float pitch = 1.0f;
        public bool playOnAwake = false;
    }

    [System.Serializable]
    public class NanoSong
    {
        public string songName;
        public string artistName;
    }
}