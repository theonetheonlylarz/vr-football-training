using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FootballTraining.Core;
using FootballTraining.Data;
using FootballTraining.Formations;
using FootballTraining.Players;
using FootballTraining.GapSystem;
using FootballTraining.Reactions;
using FootballTraining.Audio;

namespace FootballTraining.Plays
{
    /// <summary>
    /// Orchestrates a full rep: spawns formation, runs snap delay, executes play,
    /// and coordinates the gap assignment window.
    /// </summary>
    public class PlayDirector : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private FormationManager _formationManager;
        [SerializeField] private GapAssignmentManager _gapManager;
        [SerializeField] private ReactionTimerController _reactionTimer;
        [SerializeField] private CalloutSystem _calloutSystem;

        [Header("Snap Settings")]
        [SerializeField] private AudioSource _snapSoundSource;
        [SerializeField] private AudioClip _snapSound;

        private PlayConfig _activePlay;
        private DifficultyConfig _difficulty;
        private float _speedMult = 1f;
        private Coroutine _playCoroutine;
        private bool _isPaused;
        private bool _isReplayMode;

        public bool IsPlaying => _playCoroutine != null;

        private static readonly Dictionary<PlayType, PlayLibrary.PlayBuilder> _playBuilders
            = new Dictionary<PlayType, PlayLibrary.PlayBuilder>
            {
                { PlayType.InsideZone,  PlayLibrary.BuildInsideZoneMovements },
                { PlayType.OutsideZone, PlayLibrary.BuildOutsideZoneMovements },
                { PlayType.Power,       PlayLibrary.BuildPowerMovements },
                { PlayType.Pass,        PlayLibrary.BuildPassMovements },
            };

        public void LoadPlay(PlayConfig play, DifficultyConfig difficulty, bool replay = false)
        {
            _activePlay = play;
            _difficulty = difficulty;
            _speedMult = difficulty != null ? difficulty.PlaySpeedMultiplier : 1f;
            _isReplayMode = replay;

            _formationManager.SpawnFormation(play.Formation);
            ApplyMovementConfigs(play);
        }

        private void ApplyMovementConfigs(PlayConfig play)
        {
            if (!_playBuilders.TryGetValue(play.PlayType, out var builder)) return;

            var configs = builder(play);
            foreach (var cfg in configs)
            {
                var player = _formationManager.GetPlayerByRole(cfg.Role);
                if (player == null) continue;
                player.SetMovementConfig(cfg, _speedMult);

                if (player is LinemenController lc)
                {
                    foreach (var ba in play.BlockAssignments)
                        if (ba.BlockerRole == cfg.Role) { lc.AssignBlock(ba); break; }
                }
                else if (player is SkillPlayerController sc)
                {
                    sc.AssignRoute(cfg);
                }
            }
        }

        public void StartPlay()
        {
            if (_playCoroutine != null) StopCoroutine(_playCoroutine);
            _playCoroutine = StartCoroutine(RunPlay());
        }

        private IEnumerator RunPlay()
        {
            // Pre-snap delay (cadence simulation)
            float snapDelay = Random.Range(_activePlay.SnapDelayMin, _activePlay.SnapDelayMax);
            if (_difficulty != null && _difficulty.UseVariableSnapCount)
                snapDelay = Random.Range(0.3f, _activePlay.SnapDelayMax * 1.5f);

            yield return new WaitForSeconds(snapDelay / _speedMult);

            // Hard count jump fake
            if (_difficulty != null && Random.value < _difficulty.SnapCountJumpFakeChance)
            {
                _calloutSystem?.PlayHardCount();
                yield return new WaitForSeconds(1.0f);
            }

            // --- SNAP ---
            PlaySnapSound();
            _calloutSystem?.PlaySnapCall(_activePlay.HuddleBreakCallout);
            GameManager.Instance?.TriggerSnap();

            // Execute all player movements simultaneously
            foreach (var player in _formationManager.ActivePlayers)
            {
                if (player is LinemenController lc) lc.ExecuteBlock(_speedMult);
                else if (player is SkillPlayerController sc) sc.ExecuteRoute();
                else if (player is QuarterbackController qc) qc.ExecuteSnap(
                    _activePlay.PlayType != PlayType.Pass);
                else player.ExecuteMovement();
            }

            // Open gap assignment window
            if (!_isReplayMode)
            {
                float window = _difficulty?.ReactionWindowSeconds ?? 3f;
                _gapManager.OpenSelectionWindow(window);
                _reactionTimer.StartTimer(window);
            }

            yield return new WaitForSeconds(_activePlay.PlayDurationSeconds / _speedMult);
            _playCoroutine = null;
        }

        public void PausePlay(bool paused)
        {
            _isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        public void SetSpeedMultiplier(float mult)
        {
            _speedMult = mult;
            foreach (var p in _formationManager.ActivePlayers)
                p.SetPlaySpeed(mult);
        }

        public void ResetPlay()
        {
            if (_playCoroutine != null) { StopCoroutine(_playCoroutine); _playCoroutine = null; }
            Time.timeScale = 1f;
            _formationManager.ResetAllToPreSnapPositions();
            _reactionTimer.StopTimer();
            _gapManager.CloseSelectionWindow();
        }

        private void PlaySnapSound()
        {
            if (_snapSoundSource != null && _snapSound != null)
                _snapSoundSource.PlayOneShot(_snapSound);
        }
    }
}
