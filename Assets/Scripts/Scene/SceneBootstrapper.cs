using UnityEngine;
using FootballTraining.Core;
using FootballTraining.Formations;
using FootballTraining.Plays;
using FootballTraining.GapSystem;
using FootballTraining.Reactions;
using FootballTraining.Stats;
using FootballTraining.VR;
using FootballTraining.Audio;
using FootballTraining.Training;
using FootballTraining.UI;

namespace FootballTraining.Scene
{
    /// <summary>
    /// Master bootstrapper for the training scene.
    /// Wires all subsystem references together and sets up the play loop.
    /// All references should be assigned in the Inspector; this script handles
    /// runtime cross-wiring that can't be done statically.
    /// </summary>
    public class SceneBootstrapper : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private GameManager        _gameManager;
        [SerializeField] private SessionManager     _sessionManager;
        [SerializeField] private DifficultyManager  _difficultyManager;

        [Header("Formation & Play")]
        [SerializeField] private FormationManager   _formationManager;
        [SerializeField] private PlayDirector       _playDirector;

        [Header("Gap System")]
        [SerializeField] private GapAssignmentManager _gapManager;
        [SerializeField] private ReactionTimerController _reactionTimer;

        [Header("VR")]
        [SerializeField] private QuestInputHandler  _inputHandler;
        [SerializeField] private LinebackerCameraRig _cameraRig;
        [SerializeField] private GestureDetector    _gestureDetector;

        [Header("Audio")]
        [SerializeField] private AudioManager       _audioManager;
        [SerializeField] private CalloutSystem      _calloutSystem;

        [Header("Training Modes")]
        [SerializeField] private PracticeModeController _practiceModeCtrl;
        [SerializeField] private TestModeController     _testModeCtrl;

        [Header("Stats")]
        [SerializeField] private StatsManager       _statsManager;

        [Header("UI")]
        [SerializeField] private MainMenuUI         _mainMenuUI;
        [SerializeField] private PreSnapHUD         _preSnapHUD;
        [SerializeField] private PlayHUD            _playHUD;
        [SerializeField] private ResultUI           _resultUI;
        [SerializeField] private StatsUI            _statsUI;
        [SerializeField] private PlaySelectUI       _playSelectUI;

        private void Awake()
        {
            ValidateReferences();
            WireEvents();
        }

        private void WireEvents()
        {
            // Gap selection → GameManager (covers both confirmed selection AND the timer-expired
            // path — GapAssignmentManager itself fires OnGapSelected with GapLocation.None when
            // its internal window timeout fires, so no separate OnTimerExpired handler needed here).
            _gapManager.OnGapSelected += (gap, rt) =>
                GameManager.Instance?.SubmitRepResult(gap, rt);

            // State machine → subsystems
            GameManager.OnStateChanged += OnStateChanged;

            // Play selected → load into PlayDirector
            GameManager.OnPlaySelected += play =>
            {
                var diff = GameManager.Instance?.ActiveDifficultyConfig;
                _playDirector.LoadPlay(play, diff);
            };

            // Snap → actually start the play
            GameManager.OnSnapOccurred += () => _playDirector.StartPlay();

            // Rep completed → show result gap overlay in gap manager
            GameManager.OnRepCompleted += (correct, selected, rt) =>
            {
                var play = GameManager.Instance?.ActivePlay;
                if (play != null)
                    _gapManager.ShowResult(play.IntendedGap, selected);
            };
        }

        private void OnStateChanged(GameState state)
        {
            // Activate training mode controllers when session starts
            bool isPractice = GameManager.Instance?.ActiveMode == TrainingMode.Practice;
            bool isTest     = GameManager.Instance?.ActiveMode == TrainingMode.Test;

            if (state == GameState.PreSnap)
            {
                // Cancel any stale pending snap from a previous rep before scheduling a new one.
                CancelInvoke(nameof(TriggerSnap));
                float viewSeconds = GameManager.Instance?.ActiveDifficultyConfig?.PreSnapViewSeconds ?? 4f;
                Invoke(nameof(TriggerSnap), viewSeconds);
            }

            // BeginTest at PlaySelect so the RunTest coroutine's WaitUntil(PreSnap) fires
            // correctly for the first rep. Triggering at PlayRunning caused the first rep to be
            // skipped because PreSnap had already passed.
            if (state == GameState.PlaySelect && isTest && _testModeCtrl != null && !_testModeCtrl.IsRunning)
                _testModeCtrl.BeginTest();
        }

        private void TriggerSnap() => GameManager.Instance?.TriggerSnap();

        private void ValidateReferences()
        {
            if (_gameManager == null)
                Debug.LogError("[SceneBootstrapper] GameManager not assigned!");
            if (_formationManager == null)
                Debug.LogError("[SceneBootstrapper] FormationManager not assigned!");
            if (_playDirector == null)
                Debug.LogError("[SceneBootstrapper] PlayDirector not assigned!");
            if (_gapManager == null)
                Debug.LogError("[SceneBootstrapper] GapAssignmentManager not assigned!");
        }
    }
}
