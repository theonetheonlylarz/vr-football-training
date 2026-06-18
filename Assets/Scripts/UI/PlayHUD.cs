using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FootballTraining.Core;

namespace FootballTraining.UI
{
    /// <summary>
    /// Minimal overlay shown while the play is running.
    /// Shows the reaction timer and a subtle gap assignment reminder.
    /// Deliberately kept small to not obstruct the field view.
    /// </summary>
    public class PlayHUD : MonoBehaviour
    {
        [Header("Speed Display (Practice Mode)")]
        [SerializeField] private GameObject _speedPanel;
        [SerializeField] private TextMeshProUGUI _speedLabel;

        [Header("Rep Counter")]
        [SerializeField] private TextMeshProUGUI _repCounterLabel;

        [Header("Instruction")]
        [SerializeField] private TextMeshProUGUI _instructionText;

        private void Awake()
        {
            GameManager.OnStateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            GameManager.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.PlayComplete || state == GameState.MainMenu) Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);

            bool isPractice = GameManager.Instance?.ActiveMode == TrainingMode.Practice;
            if (_speedPanel != null) _speedPanel.SetActive(isPractice);

            if (_instructionText != null)
                _instructionText.text = "POINT to your gap — TRIGGER to confirm";

            UpdateRepCounter();
        }

        public void Hide() => gameObject.SetActive(false);

        public void UpdateSpeedDisplay(float speed)
        {
            if (_speedLabel != null)
                _speedLabel.text = $"{speed:F2}x";
        }

        private void UpdateRepCounter()
        {
            if (_repCounterLabel == null) return;
            int count = GameManager.Instance?.ActiveMode == TrainingMode.Test
                ? GameManager.Instance?.ActiveDifficultyConfig != null ? 15 : 15
                : 0;
            _repCounterLabel.gameObject.SetActive(GameManager.Instance?.ActiveMode == TrainingMode.Test);
        }
    }
}
