using UnityEngine;
using UnityEngine.Events;

namespace Nanodogs.API.NanoMusic
{
    /// <summary>
    /// NanoMusic Main Script with AudioSource-based playback.
    /// </summary>
    public class NanoMusic : MonoBehaviour
    {
        public static NanoMusic Instance { get; private set; }

        [Header("Audio Source Settings")]
        [SerializeField] private AudioSource musicSource;

        public NanoMusicAsset CurrentPlaying { get; private set; }

        public UnityEvent<NanoMusicAsset> OnMusicStarted = new();
        public UnityEvent<NanoMusicAsset> OnMusicChanged = new();
        public UnityEvent<NanoMusicAsset> OnMusicStopped = new();

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create AudioSource if none is assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
        }

        /// <summary>
        /// Plays a given music asset.
        /// </summary>
        public void PlayMusic(NanoMusicAsset musicAsset)
        {
            if (musicAsset == null || musicAsset.musicClip == null)
            {
                Debug.LogWarning("Invalid music asset or clip.");
                return;
            }

            if (CurrentPlaying == musicAsset && musicSource.isPlaying)
                return;

            musicSource.clip = musicAsset.musicClip;
            musicSource.volume = musicAsset.volume;
            musicSource.pitch = musicAsset.pitch;
            musicSource.loop = musicAsset.loop;
            musicSource.Play();

            CurrentPlaying = musicAsset;
            Debug.Log($"Playing music: {musicAsset.songData.songName}");

            OnMusicStarted.Invoke(musicAsset);
        }

        public void ChangeMusic(NanoMusicAsset newMusicAsset)
        {
            if (newMusicAsset == null || newMusicAsset.musicClip == null)
            {
                Debug.LogWarning("Invalid music asset or clip.");
                return;
            }
            if (CurrentPlaying == newMusicAsset)
                return;

            musicSource.clip = newMusicAsset.musicClip;
            musicSource.volume = newMusicAsset.volume;
            musicSource.pitch = newMusicAsset.pitch;
            musicSource.loop = newMusicAsset.loop;
            musicSource.Play();

            CurrentPlaying = newMusicAsset;
            Debug.Log($"Changed music to: {newMusicAsset.songData.songName}");

            OnMusicChanged.Invoke(newMusicAsset);
        }

        /// <summary>
        /// Stops the currently playing music.
        /// </summary>
        public void StopCurrentMusic()
        {
            if (CurrentPlaying == null || musicSource.clip == null)
            {
                Debug.LogWarning("No music is currently playing.");
                return;
            }

            musicSource.Stop();
            Debug.Log($"Stopped music: {CurrentPlaying.musicClip.name}");
            OnMusicStopped.Invoke(CurrentPlaying);
            CurrentPlaying = null;
        }

        /// <summary>
        /// Pauses the currently playing music.
        /// </summary>
        public void PauseCurrentMusic()
        {
            if (CurrentPlaying == null || !musicSource.isPlaying)
            {
                Debug.LogWarning("No music is currently playing.");
                return;
            }

            musicSource.Pause();
            Debug.Log($"Paused music: {CurrentPlaying.musicClip.name}");
        }

        /// <summary>
        /// Resumes the currently paused music.
        /// </summary>
        public void ResumeCurrentMusic()
        {
            if (CurrentPlaying == null || musicSource.isPlaying)
            {
                Debug.LogWarning("No music to resume.");
                return;
            }

            musicSource.UnPause();
            Debug.Log($"Resumed music: {CurrentPlaying.musicClip.name}");
        }

        /// <summary>
        /// Fades out the current music over time.
        /// </summary>
        public void FadeOutCurrent(float duration)
        {
            if (CurrentPlaying == null || !musicSource.isPlaying)
                return;

            StartCoroutine(FadeOutRoutine(duration));
        }

        private System.Collections.IEnumerator FadeOutRoutine(float duration)
        {
            float startVolume = musicSource.volume;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume;
            CurrentPlaying = null;
        }

        /// <summary>
        /// Fades in new music.
        /// </summary>
        public void FadeInMusic(NanoMusicAsset musicAsset, float duration)
        {
            PlayMusic(musicAsset);
            StartCoroutine(FadeInRoutine(duration));
        }

        private System.Collections.IEnumerator FadeInRoutine(float duration)
        {
            musicSource.volume = 0f;
            float targetVolume = CurrentPlaying != null ? CurrentPlaying.volume : 1f;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, targetVolume, time / duration);
                yield return null;
            }

            musicSource.volume = targetVolume;
        }
    }
}