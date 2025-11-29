using UnityEngine;

namespace Nanodogs.API.NanoMusic.Example
{
    /// <summary>
    /// Example script for playing NanoMusic assets.
    /// </summary>
    public class NanoMusicNumPlayer : MonoBehaviour
    {
        public NanoMusicAsset currentMusic;
        public void PlayMusic()
        {
            NanoMusic.Instance.PlayMusic(currentMusic);
        }
    }
}
