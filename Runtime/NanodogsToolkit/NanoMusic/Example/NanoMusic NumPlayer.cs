using UnityEngine;

namespace Nanodogs.API.NanoMusic.Example
{
    /// <summary>
    /// Example script for playing NanoMusic assets.
    /// </summary>
    public class NanoMusicNumPlayer : MonoBehaviour
    {
        public NanoMusic music;
        public NanoMusicAsset currentMusic;
        public void Start()
        {
            music.PlayMusic(currentMusic);
        }
    }
}
