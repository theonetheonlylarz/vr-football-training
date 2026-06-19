using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using FootballTraining.Core;
using FootballTraining.Audio;

namespace FootballTraining.UI
{
    /// <summary>
    /// Post-play result panel. Shows correct/incorrect, selected gap vs actual gap,
    /// reaction time, and provides next-rep / replay buttons.
    /// </summary>
    public class ResultUI : MonoBehaviour
    {
        [Header("Result Text")]
        [SerializeField] private TextMeshProUGUI _resultHeader;        // "CORRECT!" / "WRONG"
        [SerializeField] private TextMeshProUGUI _selectedGapLabel;
        [SerializeField] private TextMeshProUGUI _correctGapLabel;
        [SerializeField] private TextMeshProUGUI _reactionTimeLabel;
        [SerializeField] private TextMeshProUGUI _ratingLabel;         // "Great", "Fast", etc.

        [Header("Colors")]
        [SerializeField] private Color _correctColor = new Color(0.2f, 0.9f, 0.3f);
        [SerializeField] private Color _wrongColor   = new Color(0.95f, 0.2f, 0.2f);

        [Header("Buttons")]
        [SerializeField] private Button _nextRepButton;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _endSessionButton;

        [Header("Animation")]
        [SerializeField] private Animator _panelAnimator;

        private void Awake()
        {
            _nextRepButton    ?.onClick.AddListener(OnNextRep);
            _replayButton     ?.onClick.AddListener(OnReplay);
            _endSessionButton ?.onClick.AddListener(OnEndSession);

            gameObject.SetActive(false);
        }

        public void Show(bool correct, GapLocation selected, GapLocation correctGap, float reactionTime)
        {
            gameObject.SetActive(true);

            if (_resultHeader != null)
            {
                _resultHeader.text  = correct ? "CORRECT!" : "WRONG GAP";
                _resultHeader.color = correct ? _correctColor : _wrongColor;
            }

            if (_selectedGapLabel != null)
                _selectedGapLabel.text = $"Your gap: {FormatGap(selected)}";

            if (_correctGapLabel != null)
                _correctGapLabel.text = correct ? "" : $"Correct gap: {FormatGap(correctGap)}";

            if (_reactionTimeLabel != null)
                _reactionTimeLabel.text = reactionTime > 0
                    ? $"Reaction: {reactionTime:F2}s"
                    : "Reaction: — (time expired)";

            if (_ratingLabel != null)
                _ratingLabel.text = RatingText(correct, reactionTime);

            bool isPractice = GameManager.Instance?.ActiveMode == TrainingMode.Practice;
            _replayButton?.gameObject.SetActive(isPractice);

            if (correct) AudioManager.Instance?.PlayCorrectFeedback();
            else         AudioManager.Instance?.PlayWrongFeedback();

            _panelAnimator?.SetTrigger("SlideIn");
        }

        private void OnNextRep()
        {
            _panelAnimator?.SetTrigger("SlideOut");
            StartCoroutine(DeferredAction(0.25f, () => {
                gameObject.SetActive(false);
                GameManager.Instance?.LoadNextRep();
            }));
        }

        private void OnReplay()
        {
            gameObject.SetActive(false);
            GameManager.Instance?.RequestReplay();
        }

        private void OnEndSession()
        {
            gameObject.SetActive(false);
            GameManager.Instance?.EndSession();
        }

        private IEnumerator DeferredAction(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        private static string FormatGap(GapLocation gap) => gap switch
        {
            GapLocation.ALeft  => "A-Left",
            GapLocation.ARight => "A-Right",
            GapLocation.BLeft  => "B-Left",
            GapLocation.BRight => "B-Right",
            GapLocation.CLeft  => "C-Left",
            GapLocation.CRight => "C-Right",
            GapLocation.DLeft  => "D-Left",
            GapLocation.DRight => "D-Right",
            GapLocation.None   => "None (expired)",
            _                  => gap.ToString()
        };

        private static string RatingText(bool correct, float rt)
        {
            if (!correct)   return "Study the blocking scheme and try again.";
            if (rt < 0.8f)  return "Lightning fast!";
            if (rt < 1.5f)  return "Great read!";
            if (rt < 2.5f)  return "Good — work on speed.";
            return "Correct, but too slow for the game level.";
        }
    }
}
