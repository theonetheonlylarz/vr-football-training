using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FootballTraining.Data;
using FootballTraining.Core;

namespace FootballTraining.Audio
{
    /// <summary>
    /// Plays pre-snap formation callouts and snap-count cadence.
    /// Each FormationConfig can supply its own AudioClip; if none, fallback
    /// text is played via TextToSpeech stub or on-screen text.
    /// </summary>
    public class CalloutSystem : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioManager _audioManager;

        [Header("Snap Cadence Clips")]
        [SerializeField] private AudioClip _hardCountClip;     // "Red 80! Set! HUT!"
        [SerializeField] private AudioClip _normalSnapClip;    // "Hut!"

        [Header("Formation Callout Clips (fallback pool)")]
        [SerializeField] private List<FormationCalloutEntry> _calloutEntries = new();

        [Header("On-Screen Callout")]
        [SerializeField] private TMPro.TextMeshProUGUI _calloutTextLabel;
        [SerializeField] private float _calloutDisplaySeconds = 3f;

        private Coroutine _calloutCoroutine;

        [System.Serializable]
        public class FormationCalloutEntry
        {
            public FormationType Formation;
            public AudioClip Clip;
            public string FallbackText;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        public void PlayFormationCallout(FormationConfig formation)
        {
            if (formation == null) return;

            AudioClip clip = formation.CalloutClip;
            string text    = formation.CalloutText;

            // Try to find a fallback entry
            if (clip == null)
            {
                foreach (var entry in _calloutEntries)
                    if (entry.Formation == formation.FormationType)
                    {
                        clip = entry.Clip;
                        if (string.IsNullOrEmpty(text)) text = entry.FallbackText;
                        break;
                    }
            }

            if (clip != null) _audioManager?.PlayVoice(clip);
            ShowCalloutText(text ?? formation.FormationType.ToString());
        }

        public void PlaySnapCall(string cadenceText)
        {
            _audioManager?.PlaySFX(_normalSnapClip);
            if (!string.IsNullOrEmpty(cadenceText))
                ShowCalloutText(cadenceText, 1.2f);
        }

        public void PlayHardCount()
        {
            _audioManager?.PlayVoice(_hardCountClip);
            ShowCalloutText("SET! HUT!", 1.5f);
        }

        private void ShowCalloutText(string text, float? duration = null)
        {
            if (_calloutTextLabel == null) return;
            if (_calloutCoroutine != null) StopCoroutine(_calloutCoroutine);
            _calloutCoroutine = StartCoroutine(DisplayText(text, duration ?? _calloutDisplaySeconds));
        }

        private IEnumerator DisplayText(string text, float seconds)
        {
            _calloutTextLabel.text = text;
            _calloutTextLabel.gameObject.SetActive(true);
            yield return new WaitForSeconds(seconds);
            _calloutTextLabel.gameObject.SetActive(false);
        }
    }
}
