using UnityEngine;
using System.Collections.Generic;

namespace FootballTraining.Audio
{
    /// <summary>
    /// Central audio manager. Handles SFX, ambient crowd noise, and voice callouts.
    /// Uses a pool of AudioSources to avoid GC from repeated GetComponent calls.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _ambientSource;
        [SerializeField] private AudioSource _voiceSource;
        [SerializeField] private int _sfxPoolSize = 8;

        [Header("Ambient")]
        [SerializeField] private AudioClip _stadiumCrowdClip;
        [SerializeField] private AudioClip _crowdReactionGoodClip;
        [SerializeField] private AudioClip _crowdReactionBadClip;

        [Header("SFX")]
        [SerializeField] private AudioClip _correctAnswerSFX;
        [SerializeField] private AudioClip _wrongAnswerSFX;
        [SerializeField] private AudioClip _timerTickSFX;
        [SerializeField] private AudioClip _repCompleteChime;

        private List<AudioSource> _sfxPool = new();
        private int _poolIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildSFXPool();
        }

        private void Start()
        {
            if (_ambientSource != null && _stadiumCrowdClip != null)
            {
                _ambientSource.clip = _stadiumCrowdClip;
                _ambientSource.loop = true;
                _ambientSource.Play();
            }
        }

        private void BuildSFXPool()
        {
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                var go = new GameObject($"SFX_Pool_{i}");
                go.transform.SetParent(transform);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                _sfxPool.Add(src);
            }
        }

        // ── Public API ────────────────────────────────────────────────────────────

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            var src = GetNextPooledSource();
            src.volume = volume;
            src.PlayOneShot(clip);
        }

        public void PlayVoice(AudioClip clip, float volume = 1f)
        {
            if (_voiceSource == null || clip == null) return;
            _voiceSource.Stop();
            _voiceSource.volume = volume;
            _voiceSource.clip = clip;
            _voiceSource.Play();
        }

        public void PlayCorrectFeedback()
        {
            PlaySFX(_correctAnswerSFX);
            if (_ambientSource != null && _crowdReactionGoodClip != null)
                _ambientSource.PlayOneShot(_crowdReactionGoodClip, 0.5f);
        }

        public void PlayWrongFeedback()
        {
            PlaySFX(_wrongAnswerSFX);
        }

        public void PlayTimerTick() => PlaySFX(_timerTickSFX, 0.4f);
        public void PlayRepComplete() => PlaySFX(_repCompleteChime);

        public void SetAmbientVolume(float vol)
        {
            if (_ambientSource != null) _ambientSource.volume = vol;
        }

        public void SetMusicVolume(float vol)
        {
            if (_musicSource != null) _musicSource.volume = vol;
        }

        private AudioSource GetNextPooledSource()
        {
            var src = _sfxPool[_poolIndex % _sfxPool.Count];
            _poolIndex++;
            return src;
        }
    }
}
