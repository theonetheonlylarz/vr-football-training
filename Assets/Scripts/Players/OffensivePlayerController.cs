using UnityEngine;
using System.Collections;
using FootballTraining.Core;
using FootballTraining.Data;

namespace FootballTraining.Players
{
    /// <summary>
    /// Base controller for all offensive players. Handles positioning, pre-snap stance,
    /// and post-snap movement along a defined waypoint path.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class OffensivePlayerController : MonoBehaviour
    {
        public PlayerRole Role { get; private set; }
        public Vector3 StartOffset { get; private set; }

        protected Animator _animator;
        protected Vector3 _startWorldPosition;
        protected Quaternion _startRotation;

        private PlayerMovementConfig _movementConfig;
        private Coroutine _moveCoroutine;
        protected float _speedMultiplier = 1f;
        private bool _paused;

        // Animator parameter names
        private static readonly int HashPreSnap  = Animator.StringToHash("PreSnap");
        private static readonly int HashRun      = Animator.StringToHash("Run");
        private static readonly int HashBlock    = Animator.StringToHash("Block");
        private static readonly int HashRoute    = Animator.StringToHash("RunRoute");
        private static readonly int HashIdle     = Animator.StringToHash("Idle");

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public virtual void Initialize(PlayerRole role, Vector3 startOffset)
        {
            Role = role;
            StartOffset = startOffset;
            _startWorldPosition = transform.position;
            _startRotation = transform.rotation;
            EnterPreSnapStance();
        }

        public void SetMovementConfig(PlayerMovementConfig config, float speedMultiplier = 1f)
        {
            _movementConfig = config;
            _speedMultiplier = speedMultiplier;
        }

        public void ExecuteMovement()
        {
            if (_movementConfig == null || _movementConfig.Waypoints.Count == 0)
            {
                PerformDefaultPostSnapBehavior();
                return;
            }

            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(FollowWaypoints(_movementConfig));
        }

        private IEnumerator FollowWaypoints(PlayerMovementConfig config)
        {
            TriggerAnimation(config.AnimationTrigger);

            Vector3 originPos = transform.position;
            for (int i = 0; i < config.Waypoints.Count; i++)
            {
                var wp = config.Waypoints[i];
                Vector3 target = _startWorldPosition + wp.PositionOffset;
                float duration = wp.DurationSeconds / _speedMultiplier;
                float elapsed = 0f;
                Vector3 fromPos = transform.position;

                while (elapsed < duration)
                {
                    // Do not advance time while paused — keeps Time.timeScale = 1 for VR
                    // tracking so the headset never decouples from rendering.
                    if (!_paused)
                    {
                        elapsed += Time.deltaTime;
                        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                        transform.position = Vector3.Lerp(fromPos, target, t);

                        if ((target - transform.position).sqrMagnitude > 0.01f)
                        {
                            Vector3 dir2 = (target - transform.position).normalized;
                            dir2.y = 0;
                            if (dir2.sqrMagnitude > 0.001f)
                                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir2), Time.deltaTime * 8f);
                        }
                    }
                    yield return null;
                }
                transform.position = target;
            }
        }

        public void ResetToStartPosition()
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }
            transform.position = _startWorldPosition;
            transform.rotation = _startRotation;
            EnterPreSnapStance();
        }

        protected virtual void EnterPreSnapStance()
        {
            _animator?.SetTrigger(HashPreSnap);
        }

        protected virtual void PerformDefaultPostSnapBehavior()
        {
            _animator?.SetTrigger(HashIdle);
        }

        protected void TriggerAnimation(string triggerName)
        {
            if (string.IsNullOrEmpty(triggerName) || _animator == null) return;
            int hash = Animator.StringToHash(triggerName);
            _animator.SetTrigger(hash);
        }

        public void SetPlaySpeed(float multiplier)
        {
            _speedMultiplier = multiplier;
            if (_animator != null) _animator.speed = _paused ? 0f : multiplier;
        }

        /// <summary>
        /// Freeze/unfreeze this player without touching Time.timeScale.
        /// VR-safe: the coroutine keeps yielding so the engine keeps rendering.
        /// </summary>
        public void SetPaused(bool paused)
        {
            _paused = paused;
            if (_animator != null) _animator.speed = paused ? 0f : _speedMultiplier;
        }
    }
}
