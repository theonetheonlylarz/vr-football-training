using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FootballTraining.Core;
using FootballTraining.Data;

namespace FootballTraining.UI
{
    /// <summary>
    /// Displayed during the pre-snap read phase. Shows formation name, down & distance,
    /// and the linebacker read cue (if difficulty allows).
    /// Positioned at the top of the VR field of view, not obstructing the formation.
    /// </summary>
    public class PreSnapHUD : MonoBehaviour
    {
        [Header("Info Labels")]
        [SerializeField] private TextMeshProUGUI _formationLabel;
        [SerializeField] private TextMeshProUGUI _playTypeLabel;
        [SerializeField] private TextMeshProUGUI _readCueLabel;
        [SerializeField] private TextMeshProUGUI _difficultyLabel;
        [SerializeField] private TextMeshProUGUI _downDistanceLabel;
        [SerializeField] private TextMeshProUGUI _instructionLabel;

        [Header("Panels")]
        [SerializeField] private GameObject _readCuePanel;
        [SerializeField] private GameObject _formationLabelPanel;

        [Header("Animation")]
        [SerializeField] private Animator _hudAnimator;

        private DifficultyConfig _difficulty;

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
            if (state == GameState.PlayRunning) Hide();
        }

        public void Show(PlayConfig play)
        {
            gameObject.SetActive(true);
            _difficulty = GameManager.Instance?.ActiveDifficultyConfig;

            if (play == null) return;

            bool showFormLabel = _difficulty?.ShowFormationLabel ?? true;
            bool showReadCue   = _difficulty?.ShowPreSnapCues    ?? true;

            if (_formationLabelPanel != null)
                _formationLabelPanel.SetActive(showFormLabel);

            if (_formationLabel != null && showFormLabel)
                _formationLabel.text = play.Formation?.FormationType.ToString().Replace("Formation", " Formation") ?? "";

            if (_playTypeLabel != null)
                _playTypeLabel.text = play.PlayType.ToString().ToUpper();

            if (_readCuePanel != null)
                _readCuePanel.SetActive(showReadCue);

            if (_readCueLabel != null && showReadCue)
                _readCueLabel.text = play.LinebackerReadCue;

            if (_difficultyLabel != null)
                _difficultyLabel.text = (_difficulty?.Level ?? Difficulty.Medium).ToString().ToUpper();

            if (_instructionLabel != null)
                _instructionLabel.text = "Read the offense. Point and confirm your gap assignment after the snap.";

            _hudAnimator?.SetTrigger("FadeIn");
        }

        public void Hide()
        {
            _hudAnimator?.SetTrigger("FadeOut");
            CancelInvoke(nameof(Deactivate));   // prevent accumulated calls from hiding a freshly re-shown HUD
            Invoke(nameof(Deactivate), 0.3f);
        }

        private void Deactivate() => gameObject.SetActive(false);
    }
}
