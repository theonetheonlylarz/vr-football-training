using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FootballTraining.Core;
using FootballTraining.Data;
using FootballTraining.Stats;

namespace FootballTraining.UI
{
    /// <summary>
    /// Session and all-time stats review screen. Shown after a session ends.
    /// Displays accuracy, reaction time, per-play-type breakdown, and trend data.
    /// </summary>
    public class StatsUI : MonoBehaviour
    {
        [Header("Session Stats")]
        [SerializeField] private TextMeshProUGUI _sessionHeaderLabel;
        [SerializeField] private TextMeshProUGUI _accuracyLabel;
        [SerializeField] private TextMeshProUGUI _avgReactionLabel;
        [SerializeField] private TextMeshProUGUI _fastestReactionLabel;
        [SerializeField] private TextMeshProUGUI _totalRepsLabel;
        [SerializeField] private TextMeshProUGUI _correctRepsLabel;
        [SerializeField] private TextMeshProUGUI _modeLabel;

        [Header("Lifetime Stats")]
        [SerializeField] private TextMeshProUGUI _lifetimeAccuracyLabel;
        [SerializeField] private TextMeshProUGUI _lifetimeBestLabel;
        [SerializeField] private TextMeshProUGUI _lifetimeTotalRepsLabel;
        [SerializeField] private TextMeshProUGUI _weakFormationLabel;

        [Header("Per-Play-Type")]
        [SerializeField] private TextMeshProUGUI _izAccuracyLabel;
        [SerializeField] private TextMeshProUGUI _ozAccuracyLabel;
        [SerializeField] private TextMeshProUGUI _powerAccuracyLabel;
        [SerializeField] private TextMeshProUGUI _passAccuracyLabel;

        [Header("Buttons")]
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _playAgainButton;

        [Header("StatsManager")]
        [SerializeField] private StatsManager _statsManager;

        private void Awake()
        {
            _mainMenuButton ?.onClick.AddListener(() => { Hide(); GameManager.Instance?.ReturnToMainMenu(); });
            _playAgainButton?.onClick.AddListener(() => { Hide(); GameManager.Instance?.ReturnToMainMenu(); });
            gameObject.SetActive(false);
        }

        public void Show(SessionData session)
        {
            gameObject.SetActive(true);

            if (session != null)
            {
                string grade = GetGrade(session.AccuracyPercent);
                if (_sessionHeaderLabel != null)
                    _sessionHeaderLabel.text = $"Session {session.SessionId}  —  Grade: {grade}";

                if (_modeLabel != null)
                    _modeLabel.text = $"{session.Mode}  |  {session.Difficulty}";

                if (_accuracyLabel != null)
                    _accuracyLabel.text = $"Accuracy: {session.AccuracyPercent:F1}%";

                if (_avgReactionLabel != null)
                    _avgReactionLabel.text = $"Avg Reaction: {session.AverageReactionTime:F2}s";

                if (_fastestReactionLabel != null)
                    _fastestReactionLabel.text = $"Fastest: {session.FastestReactionTime:F2}s";

                if (_totalRepsLabel != null)
                    _totalRepsLabel.text = $"Reps: {session.TotalReps}";

                if (_correctRepsLabel != null)
                    _correctRepsLabel.text = $"Correct: {session.CorrectReps} / {session.TotalReps}";
            }

            if (_statsManager != null)
                PopulateLifetimeStats();
        }

        private void PopulateLifetimeStats()
        {
            if (_lifetimeAccuracyLabel != null)
                _lifetimeAccuracyLabel.text = $"All-Time Accuracy: {_statsManager.OverallAccuracy():F1}%";

            if (_lifetimeBestLabel != null)
                _lifetimeBestLabel.text = $"Best Reaction: {_statsManager.BestReactionTime():F2}s";

            if (_lifetimeTotalRepsLabel != null)
                _lifetimeTotalRepsLabel.text = $"Total Reps: {_statsManager.TotalReps()}";

            var (formation, pct) = _statsManager.MostMissedFormation();
            if (_weakFormationLabel != null)
                _weakFormationLabel.text = $"Work On: {formation} ({pct:F0}% correct)";

            if (_izAccuracyLabel    != null) _izAccuracyLabel.text    = $"IZ:    {_statsManager.AccuracyByPlayType(PlayType.InsideZone):F0}%";
            if (_ozAccuracyLabel    != null) _ozAccuracyLabel.text    = $"OZ:    {_statsManager.AccuracyByPlayType(PlayType.OutsideZone):F0}%";
            if (_powerAccuracyLabel != null) _powerAccuracyLabel.text = $"Power: {_statsManager.AccuracyByPlayType(PlayType.Power):F0}%";
            if (_passAccuracyLabel  != null) _passAccuracyLabel.text  = $"Pass:  {_statsManager.AccuracyByPlayType(PlayType.Pass):F0}%";
        }

        public void Hide() => gameObject.SetActive(false);

        private static string GetGrade(float accuracy) => accuracy switch
        {
            >= 95f => "A+",
            >= 90f => "A",
            >= 85f => "B+",
            >= 80f => "B",
            >= 75f => "C+",
            >= 70f => "C",
            >= 60f => "D",
            _      => "F"
        };
    }
}
