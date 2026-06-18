using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FootballTraining.Core;

namespace FootballTraining.UI
{
    /// <summary>
    /// Main menu panel: mode selection (Practice / Test) and difficulty selection.
    /// Floats as a world-space canvas ~2m in front of the player.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _modeSelectPanel;
        [SerializeField] private GameObject _difficultyPanel;
        [SerializeField] private GameObject _creditsPanel;

        [Header("Mode Buttons")]
        [SerializeField] private Button _practiceButton;
        [SerializeField] private Button _testButton;

        [Header("Difficulty Buttons")]
        [SerializeField] private Button _easyButton;
        [SerializeField] private Button _mediumButton;
        [SerializeField] private Button _hardButton;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _selectedModeLabel;

        private TrainingMode _selectedMode = TrainingMode.Practice;

        private void Awake()
        {
            _practiceButton?.onClick.AddListener(() => SelectMode(TrainingMode.Practice));
            _testButton    ?.onClick.AddListener(() => SelectMode(TrainingMode.Test));
            _easyButton    ?.onClick.AddListener(() => ConfirmDifficulty(Difficulty.Easy));
            _mediumButton  ?.onClick.AddListener(() => ConfirmDifficulty(Difficulty.Medium));
            _hardButton    ?.onClick.AddListener(() => ConfirmDifficulty(Difficulty.Hard));
        }

        public void Show()
        {
            gameObject.SetActive(true);
            ShowPanel(_modeSelectPanel);
        }

        public void Hide() => gameObject.SetActive(false);

        private void SelectMode(TrainingMode mode)
        {
            _selectedMode = mode;
            if (_selectedModeLabel != null)
                _selectedModeLabel.text = mode == TrainingMode.Practice ? "Practice Mode" : "Test Mode";
            ShowPanel(_difficultyPanel);
        }

        private void ConfirmDifficulty(Difficulty diff)
        {
            Hide();
            GameManager.Instance?.StartSession(_selectedMode, diff);
        }

        private void ShowPanel(GameObject panel)
        {
            _modeSelectPanel?.SetActive(panel == _modeSelectPanel);
            _difficultyPanel?.SetActive(panel == _difficultyPanel);
            _creditsPanel   ?.SetActive(panel == _creditsPanel);
        }
    }
}
