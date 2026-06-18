using UnityEngine;
using FootballTraining.Core;
using FootballTraining.Plays;

namespace FootballTraining.Training
{
    /// <summary>
    /// Practice mode: unlimited time, slow-motion control, instant replay,
    /// and replay with full-speed comparison.
    /// </summary>
    public class PracticeModeController : MonoBehaviour
    {
        [SerializeField] private PlayDirector _playDirector;
        [SerializeField] private VR.QuestInputHandler _input;

        [Header("Speed Control")]
        [SerializeField] private float[] _speedPresets = { 0.25f, 0.5f, 0.75f, 1.0f };
        [SerializeField] private int _currentSpeedIndex = 3;    // Start at full speed

        private bool _isPaused;

        public float CurrentSpeed => _speedPresets[_currentSpeedIndex];

        private void Update()
        {
            if (GameManager.Instance?.CurrentState is not (GameState.PlayRunning or GameState.Replay))
                return;

            // Primary button: pause / resume
            if (_input.GetPrimaryButtonDown())
                TogglePause();

            // Thumbstick left/right: cycle speed
            var stick = _input.GetThumbstick();
            if (stick.x > 0.7f) SpeedUp();
            else if (stick.x < -0.7f) SlowDown();
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
            _playDirector.PausePlay(_isPaused);
        }

        public void SlowDown()
        {
            _currentSpeedIndex = Mathf.Max(0, _currentSpeedIndex - 1);
            ApplySpeed();
        }

        public void SpeedUp()
        {
            _currentSpeedIndex = Mathf.Min(_speedPresets.Length - 1, _currentSpeedIndex + 1);
            ApplySpeed();
        }

        public void SetSpeedPreset(int index)
        {
            _currentSpeedIndex = Mathf.Clamp(index, 0, _speedPresets.Length - 1);
            ApplySpeed();
        }

        public void ReplayCurrentPlay()
        {
            if (GameManager.Instance?.ActivePlay == null) return;
            _playDirector.ResetPlay();
            GameManager.Instance.RequestReplay();
        }

        private void ApplySpeed()
        {
            if (!_isPaused)
                _playDirector.SetSpeedMultiplier(CurrentSpeed);
        }
    }
}
