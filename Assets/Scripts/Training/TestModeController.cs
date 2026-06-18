using UnityEngine;
using System.Collections;
using FootballTraining.Core;
using FootballTraining.Plays;
using FootballTraining.GapSystem;
using FootballTraining.Audio;

namespace FootballTraining.Training
{
    /// <summary>
    /// Test mode: timed reps with auto-advance, score tracking, and final grade.
    /// No pausing or slow-motion. Forces quick decision-making under pressure.
    /// </summary>
    public class TestModeController : MonoBehaviour
    {
        [SerializeField] private PlayDirector _playDirector;
        [SerializeField] private GapAssignmentManager _gapManager;
        [SerializeField] private AudioManager _audioManager;

        [Header("Test Settings")]
        [SerializeField] private float _betweenRepDelay = 1.5f;
        [SerializeField] private int   _repsPerTest = 15;

        private int _repCount;
        private bool _isRunning;
        private Coroutine _testRoutine;

        public int CurrentRep => _repCount;
        public bool IsRunning => _isRunning;

        public void BeginTest()
        {
            _repCount = 0;
            _isRunning = true;
            if (_testRoutine != null) StopCoroutine(_testRoutine);
            _testRoutine = StartCoroutine(RunTest());
        }

        public void AbortTest()
        {
            _isRunning = false;
            if (_testRoutine != null) { StopCoroutine(_testRoutine); _testRoutine = null; }
            GameManager.Instance?.EndSession();
        }

        private IEnumerator RunTest()
        {
            for (_repCount = 1; _repCount <= _repsPerTest; _repCount++)
            {
                // Hook into GameManager to get a play loaded
                yield return new WaitUntil(() => GameManager.Instance?.CurrentState == GameState.PreSnap);
                yield return new WaitForSeconds(GameManager.Instance.ActiveDifficultyConfig?.PreSnapViewSeconds ?? 3f);

                // Play will auto-start via PlayDirector triggered by state change
                yield return new WaitUntil(() => GameManager.Instance?.CurrentState == GameState.PlayComplete);

                _audioManager?.PlayRepComplete();
                yield return new WaitForSeconds(_betweenRepDelay);

                // Advance to next rep
                GameManager.Instance?.LoadNextRep();
            }

            _isRunning = false;
            GameManager.Instance?.EndSession();
        }
    }
}
