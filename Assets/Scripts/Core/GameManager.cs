using UnityEngine;
using System;
using FootballTraining.Data;
using FootballTraining.Training;
using FootballTraining.UI;
using FootballTraining.Audio;

namespace FootballTraining.Core
{
    /// <summary>
    /// Central state machine that owns the active game state and coordinates all subsystems.
    /// Other managers register themselves here rather than using static singletons.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Current State")]
        [SerializeField] private GameState _currentState = GameState.MainMenu;
        public GameState CurrentState => _currentState;

        [Header("Settings")]
        public TrainingMode ActiveMode { get; private set; } = TrainingMode.Practice;
        public Difficulty ActiveDifficulty { get; private set; } = Difficulty.Medium;

        [Header("Active Play")]
        public PlayConfig ActivePlay { get; private set; }
        public DifficultyConfig ActiveDifficultyConfig { get; private set; }

        // Events broadcast to all interested systems
        public static event Action<GameState> OnStateChanged;
        public static event Action<PlayConfig> OnPlaySelected;
        public static event Action OnSnapOccurred;
        public static event Action<bool, GapLocation, float> OnRepCompleted; // correct, selectedGap, reactionTime

        [Header("Subsystem References")]
        [SerializeField] private SessionManager _sessionManager;
        [SerializeField] private DifficultyManager _difficultyManager;
        [SerializeField] private MainMenuUI _mainMenuUI;
        [SerializeField] private PreSnapHUD _preSnapHUD;
        [SerializeField] private PlayHUD _playHUD;
        [SerializeField] private ResultUI _resultUI;
        [SerializeField] private StatsUI _statsUI;
        [SerializeField] private CalloutSystem _calloutSystem;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // _currentState is already GameState.MainMenu from field initializer.
            // TransitionTo would no-op on the same-state guard, so drive the first
            // entry directly instead.
            OnEnterState(_currentState);
            OnStateChanged?.Invoke(_currentState);
        }

        public void TransitionTo(GameState newState)
        {
            if (_currentState == newState) return;

            OnExitState(_currentState);
            _currentState = newState;
            OnEnterState(_currentState);
            OnStateChanged?.Invoke(_currentState);
        }

        private void OnExitState(GameState state) { }

        private void OnEnterState(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    _mainMenuUI?.Show();
                    break;

                case GameState.PreSnap:
                    _preSnapHUD?.Show(ActivePlay);
                    if (ActivePlay != null)
                        _calloutSystem?.PlayFormationCallout(ActivePlay.Formation);
                    break;

                case GameState.PlayRunning:
                    _playHUD?.Show();
                    OnSnapOccurred?.Invoke();
                    break;

                case GameState.PlayComplete:
                    break;

                case GameState.StatsReview:
                    _statsUI?.Show(_sessionManager?.CurrentSession);
                    break;
            }
        }

        // --- Public API called by UI and subsystems ---

        public void StartSession(TrainingMode mode, Difficulty difficulty)
        {
            ActiveMode = mode;
            ActiveDifficulty = difficulty;
            ActiveDifficultyConfig = _difficultyManager?.GetConfig(difficulty);
            _sessionManager?.BeginSession(mode, difficulty);
            TransitionTo(GameState.PlaySelect);
        }

        public void LoadPlay(PlayConfig play)
        {
            ActivePlay = play;
            OnPlaySelected?.Invoke(play);
            TransitionTo(GameState.PreSnap);
        }

        public void TriggerSnap()
        {
            if (_currentState != GameState.PreSnap) return;
            TransitionTo(GameState.PlayRunning);
        }

        public void SubmitRepResult(GapLocation selectedGap, float reactionTime)
        {
            if (ActivePlay == null) return;

            bool correct = (selectedGap == ActivePlay.IntendedGap);
            _sessionManager?.RecordRep(ActivePlay, selectedGap, reactionTime);
            OnRepCompleted?.Invoke(correct, selectedGap, reactionTime);

            _resultUI?.Show(correct, selectedGap, ActivePlay.IntendedGap, reactionTime);
            TransitionTo(GameState.PlayComplete);
        }

        public void RequestReplay()
        {
            TransitionTo(GameState.Replay);
        }

        public void LoadNextRep()
        {
            if (ActiveMode == TrainingMode.Test && _sessionManager != null &&
                _sessionManager.CurrentRepCount >= GetTestModeRepTarget())
            {
                EndSession();
                return;
            }
            TransitionTo(GameState.PlaySelect);
        }

        public void EndSession()
        {
            _sessionManager?.EndSession();
            TransitionTo(GameState.StatsReview);
        }

        public void ReturnToMainMenu()
        {
            TransitionTo(GameState.MainMenu);
        }

        public int GetTestModeRepTarget() => ActiveDifficulty switch
        {
            Difficulty.Easy   => 10,
            Difficulty.Medium => 15,
            Difficulty.Hard   => 20,
            _                 => 15
        };
    }
}
