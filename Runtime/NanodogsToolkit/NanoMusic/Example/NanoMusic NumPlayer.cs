using UnityEngine;

namespace Nanodogs.API.NanoMusic.Example
{
    /// <summary>
    /// Example script for playing NanoMusic assets.
    /// </summary>
    public class NanoMusicNumPlayer : MonoBehaviourRequireNanoMusic
    {
        public NanoMusicAsset currentMusic;
        public void Start()
        {
            music.PlayMusic(currentMusic);
        }
    }
}
