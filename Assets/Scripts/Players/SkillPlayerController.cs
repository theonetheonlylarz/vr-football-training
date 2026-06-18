using UnityEngine;
using System.Collections;
using FootballTraining.Core;
using FootballTraining.Data;

namespace FootballTraining.Players
{
    /// <summary>
    /// Handles wide receivers, running backs, and fullbacks — route running, cuts, and lead blocks.
    /// </summary>
    public class SkillPlayerController : OffensivePlayerController
    {
        [Header("Skill Player")]
        [SerializeField] private float _baseRouteSpeed = 7f;
        [SerializeField] private TrailRenderer _routeTrail;

        private RouteType _assignedRoute;

        private static readonly int HashRunRoute  = Animator.StringToHash("RunRoute");
        private static readonly int HashCarryBall = Animator.StringToHash("CarryBall");
        private static readonly int HashCatch     = Animator.StringToHash("Catch");

        protected override void Awake()
        {
            base.Awake();
            if (_routeTrail != null) _routeTrail.emitting = false;
        }

        public void AssignRoute(PlayerMovementConfig routeConfig)
        {
            _assignedRoute = routeConfig.RouteType;
            SetMovementConfig(routeConfig);
        }

        public void ExecuteRoute()
        {
            if (_routeTrail != null) _routeTrail.emitting = true;
            ExecuteMovement();
        }

        public override void Initialize(PlayerRole role, Vector3 startOffset)
        {
            base.Initialize(role, startOffset);
        }

        protected override void PerformDefaultPostSnapBehavior()
        {
            // Skill players that aren't assigned a route still step in motion
            StartCoroutine(DefaultMotion());
        }

        private IEnumerator DefaultMotion()
        {
            TriggerAnimation("Run");
            Vector3 forwardDir = Role switch
            {
                PlayerRole.HalfBack  => Vector3.forward * 4f,
                PlayerRole.FullBack  => Vector3.forward * 3f,
                _                    => Vector3.forward * 5f
            };

            float duration = 1.5f;
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Vector3 target = startPos + forwardDir;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, target, elapsed / duration);
                yield return null;
            }
        }

        public void ShowBallCarry()
        {
            TriggerAnimation("CarryBall");
        }
    }
}
