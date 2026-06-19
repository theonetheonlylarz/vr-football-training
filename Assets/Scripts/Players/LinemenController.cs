using UnityEngine;
using FootballTraining.Core;
using FootballTraining.Data;

namespace FootballTraining.Players
{
    /// <summary>
    /// Specialization for offensive linemen and tight ends.
    /// Handles block stances, drive blocks, double-teams, and pull routes.
    /// </summary>
    public class LinemenController : OffensivePlayerController
    {
        [Header("Lineman Settings")]
        [SerializeField] private float _driveBlockForce = 2f;

        private BlockAssignmentConfig _blockAssignment;

        private static readonly int HashDownBlock   = Animator.StringToHash("DownBlock");
        private static readonly int HashPullBlock   = Animator.StringToHash("PullBlock");
        private static readonly int HashDoubleTeam  = Animator.StringToHash("DoubleTeam");
        private static readonly int HashPassSet     = Animator.StringToHash("PassSet");
        private static readonly int HashKickOut     = Animator.StringToHash("KickOut");
        private static readonly int HashLeadBlock   = Animator.StringToHash("LeadBlock");

        public void AssignBlock(BlockAssignmentConfig assignment)
        {
            _blockAssignment = assignment;
        }

        public void ExecuteBlock(float speedMultiplier = 1f)
        {
            if (_blockAssignment == null)
            {
                PerformDefaultPostSnapBehavior();
                return;
            }

            SetPlaySpeed(speedMultiplier);

            switch (_blockAssignment.Target)
            {
                case BlockingTarget.PassPro:
                    TriggerAnimation("PassSet");
                    PerformPassSet();
                    break;

                case BlockingTarget.KickOut:
                    TriggerAnimation("KickOut");
                    PerformKickOut();
                    break;

                default:
                    if (_blockAssignment.IsPullBlock)
                        PerformPullBlock();
                    else
                        PerformDriveBlock();
                    break;
            }
        }

        private void PerformDriveBlock()
        {
            float angle = _blockAssignment.BlockAngleDegrees;
            Vector3 blockDir = Quaternion.Euler(0, angle, 0) * transform.forward;

            var moveConfig = new PlayerMovementConfig
            {
                Role = Role,
                RouteType = RouteType.None,
                AnimationTrigger = "DownBlock",
                MoveSpeed = _driveBlockForce,
                Waypoints = new System.Collections.Generic.List<RouteWaypoint>
                {
                    new RouteWaypoint { PositionOffset = blockDir * 2f, DurationSeconds = 0.6f },
                    new RouteWaypoint { PositionOffset = blockDir * 3f, DurationSeconds = 1.2f }
                }
            };
            SetMovementConfig(moveConfig, GetPlaySpeedMultiplier());
            ExecuteMovement();
        }

        private void PerformPullBlock()
        {
            float pullDist = _blockAssignment.PullDistanceYards;
            float angle = _blockAssignment.BlockAngleDegrees;
            Vector3 pullDir = (angle > 0) ? Vector3.right : Vector3.left;
            Vector3 engageDir = Quaternion.Euler(0, angle, 0) * transform.forward;

            var moveConfig = new PlayerMovementConfig
            {
                Role = Role,
                RouteType = RouteType.None,
                AnimationTrigger = "PullBlock",
                MoveSpeed = 5f,
                Waypoints = new System.Collections.Generic.List<RouteWaypoint>
                {
                    new RouteWaypoint { PositionOffset = pullDir * pullDist, DurationSeconds = 0.5f },
                    new RouteWaypoint { PositionOffset = pullDir * pullDist + engageDir * 1.5f, DurationSeconds = 0.4f },
                    new RouteWaypoint { PositionOffset = pullDir * pullDist + engageDir * 3.0f, DurationSeconds = 0.8f }
                }
            };
            SetMovementConfig(moveConfig, GetPlaySpeedMultiplier());
            ExecuteMovement();
        }

        private void PerformPassSet()
        {
            // Kick-step back, form pocket
            Vector3 kickDir = (Role == PlayerRole.LeftTackle || Role == PlayerRole.LeftGuard)
                ? new Vector3(-0.3f, 0, -0.8f)
                : new Vector3( 0.3f, 0, -0.8f);

            var moveConfig = new PlayerMovementConfig
            {
                Role = Role,
                AnimationTrigger = "PassSet",
                Waypoints = new System.Collections.Generic.List<RouteWaypoint>
                {
                    new RouteWaypoint { PositionOffset = kickDir, DurationSeconds = 0.3f },
                    new RouteWaypoint { PositionOffset = kickDir * 1.5f, DurationSeconds = 0.5f }
                }
            };
            SetMovementConfig(moveConfig, GetPlaySpeedMultiplier());
            ExecuteMovement();
        }

        private void PerformKickOut()
        {
            Vector3 kickDir = new Vector3(2f, 0, 0.5f);
            var moveConfig = new PlayerMovementConfig
            {
                Role = Role,
                AnimationTrigger = "KickOut",
                Waypoints = new System.Collections.Generic.List<RouteWaypoint>
                {
                    new RouteWaypoint { PositionOffset = kickDir, DurationSeconds = 0.5f }
                }
            };
            SetMovementConfig(moveConfig, GetPlaySpeedMultiplier());
            ExecuteMovement();
        }

        private float GetPlaySpeedMultiplier()
        {
            // Read the field directly — _animator.speed is 0 when paused, which would
            // produce Infinity when used as a divisor in waypoint duration calculations.
            return _speedMultiplier;
        }
    }
}
