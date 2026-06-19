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
            AlignGapZonesToFormation();   // must happen after SpawnFormation so transforms exist
            ApplyMovementConfigs(play);
        }

        // Repositions gap zone colliders to match the actual spawned linemen positions.
        // Without this, gap zones sit at their editor-placed positions and won't match
        // non-standard formations (Trips, Shotgun, Pistol, etc.).
        private void AlignGapZonesToFormation()
        {
            if (_gapManager == null) return;

            var lt = _formationManager.GetPlayerByRole(PlayerRole.LeftTackle)?.transform;
            var lg = _formationManager.GetPlayerByRole(PlayerRole.LeftGuard)?.transform;
            var c  = _formationManager.GetPlayerByRole(PlayerRole.Center)?.transform;
            var rg = _formationManager.GetPlayerByRole(PlayerRole.RightGuard)?.transform;
            var rt = _formationManager.GetPlayerByRole(PlayerRole.RightTackle)?.transform;
            var te = _formationManager.GetPlayerByRole(PlayerRole.TightEnd)?.transform;

            if (lt == null || lg == null || c == null || rg == null || rt == null)
            {
                Debug.LogWarning("[PlayDirector] Missing linemen transforms — gap zones not realigned.");
                return;
            }
            _gapManager.AlignGapsToFormation(lt, lg, c, rg, rt, te);
        }

        private void ApplyMovementConfigs(PlayConfig play)
        {
            if (!_playBuilders.TryGetValue(play.PlayType, out var builder))
            {
                Debug.LogWarning($"[PlayDirector] No movement builder for PlayType '{play.PlayType}'. " +
                                 "Register it in _playBuilders or add a PlayLibrary builder. Players will stand still.");
                return;
            }

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
            _playCoroutine = StartCoroutine(RunPlayAndClear());
        }

        // Wrapper so _playCoroutine is nulled after the coroutine exits naturally,
        // not on the last yield inside RunPlay — which would race with an immediate
        // ResetPlay() call turning StopCoroutine into a no-op.
        private IEnumerator RunPlayAndClear()
        {
            yield return RunPlay();
            _playCoroutine = null;
        }

        private IEnumerator RunPlay()
        {
            // Pre-snap delay (cadence simulation)
            float snapDelay = Random.Range(_activePlay.SnapDelayMin, _activePlay.SnapDelayMax);
            if (_difficulty != null && _difficulty.UseVariableSnapCount)
                snapDelay = Random.Range(0.3f, _activePlay.SnapDelayMax * 1.5f);

            yield return new WaitForSeconds(snapDelay / _speedMult);

            // Hard count jump fake — scale duration with speed multiplier like everything else
            if (_difficulty != null && Random.value < _difficulty.SnapCountJumpFakeChance)
            {
                _calloutSystem?.PlayHardCount();
                yield return new WaitForSeconds(1.0f / _speedMult);
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
        }

        public void PausePlay(bool paused)
        {
            _isPaused = paused;
            // Never touch Time.timeScale in a VR app. Freezing it decouples head tracking
            // from rendering and causes immediate motion sickness on-headset.
            // Instead, each player's coroutine checks _paused and skips elapsed-time
            // advancement, while the animator speed is set to 0 to freeze playback.
            foreach (var p in _formationManager.ActivePlayers)
                p.SetPaused(paused);
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
            _isPaused = false;
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
