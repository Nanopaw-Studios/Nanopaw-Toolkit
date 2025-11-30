using Nanodogs.API.NanoMusic;
using System;
using System.Collections;
using UnityEngine;
using static PlasticPipe.Server.MonitorStats;

namespace Nanodogs.API.NanoMusic
{
    /// <summary>
    /// Manages ambiance music in the NanoMusic system.
    /// </summary>
    public class AmbianceManager : MonoBehaviourRequireNanoMusic
    {
        public NanoMusicAsset[] ambianceTracks;
        private int currentTrackIndex = -1;
        public int minimumRandomTime = 40;
        public int maximumRandomTime = 140;

        public bool isMainMenuAmbiance = false;
        public NanoMusicAsset mainMenuTrack;

        NanoMusicAsset lastTrack = null;

        private void Start()
        {
            if (isMainMenuAmbiance && mainMenuTrack != null)
            {
                NanoMusic.Instance.PlayMusic(mainMenuTrack);
            }
            else if (ambianceTracks.Length > 0)
            {
                StartCoroutine(AmbianceLoop());
            }
        }

        private IEnumerator AmbianceLoop()
        {
            while (true)
            {
                NanoMusicAsset track;

                do
                {
                    track = ambianceTracks[UnityEngine.Random.Range(0, ambianceTracks.Length)];
                }
                while (track == lastTrack);

                lastTrack = track;

                NanoMusic.Instance.PlayMusic(track);

                yield return new WaitForSecondsRealtime(
                    track.musicClip.length + UnityEngine.Random.Range(minimumRandomTime, maximumRandomTime)
                );
            }
        }
    }
}