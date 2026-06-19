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

        private int _currentRep;

        private void Awake()
        {
            GameManager.OnStateChanged += OnStateChanged;
            GameManager.OnRepCompleted += OnRepCompleted;
        }

        private void OnDestroy()
        {
            GameManager.OnStateChanged -= OnStateChanged;
            GameManager.OnRepCompleted -= OnRepCompleted;
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.PlayComplete || state == GameState.MainMenu) Hide();
            if (state == GameState.PlaySelect) _currentRep = 0;
        }

        private void OnRepCompleted(bool correct, GapLocation gap, float rt)
        {
            _currentRep++;
            UpdateRepCounter();
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
            bool isTest = GameManager.Instance?.ActiveMode == TrainingMode.Test;
            _repCounterLabel.gameObject.SetActive(isTest);
            if (isTest)
            {
                int target = GameManager.Instance?.GetTestModeRepTarget() ?? 15;
                _repCounterLabel.text = $"Rep {_currentRep} / {target}";
            }
        }
    }
}
