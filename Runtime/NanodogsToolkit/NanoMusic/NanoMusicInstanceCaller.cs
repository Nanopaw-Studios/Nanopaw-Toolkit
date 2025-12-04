using Nanodogs.API.NanoMusic;
using UnityEngine;

namespace Nanodogs.API.NanoMusic
{
    /// <summary>
    /// Provides static helper methods for interacting with the NanoMusic singleton instance, including checking its
    /// availability and controlling music playback.
    /// </summary>
    /// <remarks>This class is intended for use in Unity projects that utilize the NanoMusic system. All
    /// methods are static and operate on the current NanoMusic singleton instance, if available. Before calling
    /// playback control methods, ensure that the NanoMusic instance exists to avoid warnings.</remarks>
    public class NanoMusicInstanceCaller : MonoBehaviour
    {
        public static bool InstanceExists()
        {
            var instance = NanoMusic.Instance;
            if (instance != null)
            {
                Debug.Log("NanoMusic Instance is available.");
                return true;
            }
            else
            {
                Debug.LogWarning("NanoMusic Instance is not available.");
                return false;
            }
        }

        public static void PlayFromInstance(NanoMusicAsset musicAsset)
        {
            if (InstanceExists())
            {
                NanoMusic.Instance.PlayMusic(musicAsset);
                Debug.Log($"Requested to play music: {musicAsset.songData.songName}");
            }
            else
            {
                Debug.LogWarning("Cannot play music. NanoMusic Instance is not available.");
            }
        }

        public static void StopFromInstance()
        {
            if (InstanceExists())
            {
                NanoMusic.Instance.StopCurrentMusic();
                Debug.Log("Requested to stop music.");
            }
            else
            {
                Debug.LogWarning("Cannot stop music. NanoMusic Instance is not available.");
            }
        }

        public static void PauseFromInstance()
        {
            if (InstanceExists())
            {
                NanoMusic.Instance.PauseCurrentMusic();
                Debug.Log("Requested to pause music.");
            }
            else
            {
                Debug.LogWarning("Cannot pause music. NanoMusic Instance is not available.");
            }
        }
    }
}
