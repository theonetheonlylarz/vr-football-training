using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace FootballTraining.Reactions
{
    /// <summary>
    /// Displays a countdown timer that pressures the user to make a gap assignment.
    /// Changes color from green to yellow to red as time runs out.
    /// </summary>
    public class ReactionTimerController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _timerRoot;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Image _timerFillImage;
        [SerializeField] private Image _timerBackground;

        [Header("Colors")]
        [SerializeField] private Color _colorSafe    = new Color(0.2f, 0.9f, 0.2f);
        [SerializeField] private Color _colorWarning = new Color(1.0f, 0.8f, 0.0f);
        [SerializeField] private Color _colorDanger  = new Color(1.0f, 0.2f, 0.1f);

        [Header("Thresholds (fraction of total time)")]
        [SerializeField] private float _warningThreshold = 0.4f;
        [SerializeField] private float _dangerThreshold  = 0.2f;

        public event Action OnTimerExpired;

        private float _totalTime;
        private float _remaining;
        private bool _running;

        private void Awake()
        {
            if (_timerRoot != null) _timerRoot.SetActive(false);
        }

        public void StartTimer(float seconds)
        {
            _totalTime = seconds;
            _remaining = seconds;
            _running = true;
            if (_timerRoot != null) _timerRoot.SetActive(true);
            UpdateVisuals();
        }

        public void StopTimer()
        {
            _running = false;
            if (_timerRoot != null) _timerRoot.SetActive(false);
        }

        private void Update()
        {
            if (!_running) return;

            _remaining -= Time.deltaTime;
            if (_remaining <= 0f)
            {
                _remaining = 0f;
                _running = false;
                UpdateVisuals();
                if (_timerRoot != null) _timerRoot.SetActive(false);
                OnTimerExpired?.Invoke();
                return;
            }
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            float fraction = _totalTime > 0 ? _remaining / _totalTime : 0f;

            if (_timerText != null)
                _timerText.text = _remaining.ToString("F1");

            if (_timerFillImage != null)
                _timerFillImage.fillAmount = fraction;

            Color c = fraction > _warningThreshold ? _colorSafe :
                      fraction > _dangerThreshold  ? _colorWarning :
                                                     _colorDanger;

            if (_timerFillImage  != null) _timerFillImage.color  = c;
            if (_timerBackground != null) _timerBackground.color = c * 0.25f;
            if (_timerText       != null) _timerText.color        = c;
        }

        public float GetElapsed() => _totalTime - _remaining;
        public float GetRemaining() => _remaining;
    }
}
