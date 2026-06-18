using UnityEngine;
using System.Collections.Generic;
using FootballTraining.Core;
using FootballTraining.Data;

namespace FootballTraining.Plays
{
    /// <summary>
    /// Defines post-snap player movements and blocking assignments for all built-in play types.
    /// Returns a list of PlayerMovementConfig used by PlayDirector to drive each player.
    /// </summary>
    public static class PlayLibrary
    {
        public delegate List<PlayerMovementConfig> PlayBuilder(PlayConfig play);

        // ── Inside Zone ──────────────────────────────────────────────────────────
        // All OL steps play-side and drive; RB reads first open gap behind the double-team
        public static List<PlayerMovementConfig> BuildInsideZoneMovements(PlayConfig play)
        {
            bool toRight = play.RunDirectionAngle >= 0;
            float dir = toRight ? 1f : -1f;

            return new List<PlayerMovementConfig>
            {
                OLZoneBlock(PlayerRole.Center,      dir * 0.3f, 2.0f),
                OLZoneBlock(PlayerRole.LeftGuard,   dir * 0.5f, 2.0f),
                OLZoneBlock(PlayerRole.RightGuard,  dir * 0.5f, 2.0f),
                OLZoneBlock(PlayerRole.LeftTackle,  dir * 0.6f, 2.0f),
                OLZoneBlock(PlayerRole.RightTackle, dir * 0.6f, 2.0f),
                OLZoneBlock(PlayerRole.TightEnd,    dir * 0.7f, 2.0f),
                RBInsideZone(play.BallCarrierRole, dir),
                QBHandoff(),
                ReceiverStaleLine(PlayerRole.WideReceiverLeft),
                ReceiverStaleLine(PlayerRole.WideReceiverRight),
                ReceiverStaleLine(PlayerRole.SlotReceiver),
            };
        }

        // ── Outside Zone ─────────────────────────────────────────────────────────
        // OL stretches laterally; TE kicks out; RB follows outside
        public static List<PlayerMovementConfig> BuildOutsideZoneMovements(PlayConfig play)
        {
            bool toRight = play.RunDirectionAngle >= 0;
            float dir = toRight ? 1f : -1f;

            return new List<PlayerMovementConfig>
            {
                OLZoneBlock(PlayerRole.Center,      dir * 0.6f, 1.8f),
                OLZoneBlock(PlayerRole.LeftGuard,   dir * 0.8f, 1.8f),
                OLZoneBlock(PlayerRole.RightGuard,  dir * 0.8f, 1.8f),
                OLZoneBlock(PlayerRole.LeftTackle,  dir * 1.0f, 1.8f),
                OLZoneBlock(PlayerRole.RightTackle, dir * 1.0f, 1.8f),
                OLZoneBlock(PlayerRole.TightEnd,    dir * 2.0f, 1.8f),  // kick out
                RBOutsideZone(play.BallCarrierRole, dir),
                QBHandoff(),
                ReceiverStaleLine(PlayerRole.WideReceiverLeft),
                ReceiverStaleLine(PlayerRole.WideReceiverRight),
                ReceiverStaleLine(PlayerRole.SlotReceiver),
            };
        }

        // ── Power ─────────────────────────────────────────────────────────────────
        // Back-side guard pulls; TE kicks out; FB leads; HB follows through the B-gap
        public static List<PlayerMovementConfig> BuildPowerMovements(PlayConfig play)
        {
            bool toRight = play.RunDirectionAngle >= 0;
            float dir = toRight ? 1f : -1f;

            return new List<PlayerMovementConfig>
            {
                OLDownBlock(PlayerRole.Center,       dir),
                OLDownBlock(PlayerRole.LeftGuard,    dir),
                OLDownBlock(PlayerRole.RightGuard,   dir),
                OLDownBlock(PlayerRole.LeftTackle,   dir),
                OLDownBlock(PlayerRole.RightTackle,  dir),
                OLPull(toRight ? PlayerRole.LeftGuard : PlayerRole.RightGuard, dir, 3.0f), // pulling guard
                OLDownBlock(PlayerRole.TightEnd,     dir),                                  // kick-out arc
                FBLeadBlock(play.BallCarrierRole, dir),
                HBPowerRun(play.BallCarrierRole, dir),
                QBHandoff(),
                ReceiverStaleLine(PlayerRole.WideReceiverLeft),
                ReceiverStaleLine(PlayerRole.WideReceiverRight),
            };
        }

        // ── Pass Plays ────────────────────────────────────────────────────────────
        // OL pass-sets; receivers run routes from RouteType on the play config
        public static List<PlayerMovementConfig> BuildPassMovements(PlayConfig play)
        {
            var list = new List<PlayerMovementConfig>
            {
                PassSet(PlayerRole.Center),
                PassSet(PlayerRole.LeftGuard),
                PassSet(PlayerRole.RightGuard),
                PassSet(PlayerRole.LeftTackle),
                PassSet(PlayerRole.RightTackle),
                PassSet(PlayerRole.TightEnd),
                QBDropback(),
                HBCheckRelease(),
            };

            // Merge in any specific route waypoints from play config
            foreach (var pm in play.PlayerMovements)
                list.Add(pm);

            // Fill default routes for receivers not already in play config
            bool hasLeft  = play.PlayerMovements.Exists(p => p.Role == PlayerRole.WideReceiverLeft);
            bool hasRight = play.PlayerMovements.Exists(p => p.Role == PlayerRole.WideReceiverRight);
            bool hasSlot  = play.PlayerMovements.Exists(p => p.Role == PlayerRole.SlotReceiver);

            if (!hasLeft)  list.Add(BuildRoute(PlayerRole.WideReceiverLeft,  RouteType.Curl,  -1f));
            if (!hasRight) list.Add(BuildRoute(PlayerRole.WideReceiverRight, RouteType.Out,    1f));
            if (!hasSlot)  list.Add(BuildRoute(PlayerRole.SlotReceiver,      RouteType.Slant,  1f));

            return list;
        }

        // ── Builder Helpers ──────────────────────────────────────────────────────

        private static PlayerMovementConfig OLZoneBlock(PlayerRole role, float lateralStep, float fwdDepth)
        {
            return new PlayerMovementConfig
            {
                Role = role,
                RouteType = RouteType.None,
                AnimationTrigger = "DownBlock",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(lateralStep, 0, 0.5f), DurationSeconds = 0.2f },
                    new() { PositionOffset = new Vector3(lateralStep, 0, fwdDepth), DurationSeconds = 0.9f }
                }
            };
        }

        private static PlayerMovementConfig OLDownBlock(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role,
                RouteType = RouteType.None,
                AnimationTrigger = "DownBlock",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(dir * 0.5f, 0, 0.3f), DurationSeconds = 0.15f },
                    new() { PositionOffset = new Vector3(dir * 0.5f, 0, 2.5f), DurationSeconds = 0.85f }
                }
            };
        }

        private static PlayerMovementConfig OLPull(PlayerRole role, float dir, float pullYards)
        {
            return new PlayerMovementConfig
            {
                Role = role,
                RouteType = RouteType.None,
                AnimationTrigger = "PullBlock",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(dir * pullYards, 0, -0.5f), DurationSeconds = 0.45f },
                    new() { PositionOffset = new Vector3(dir * pullYards, 0,  2.0f), DurationSeconds = 0.55f },
                    new() { PositionOffset = new Vector3(dir * pullYards, 0,  3.5f), DurationSeconds = 0.4f  }
                }
            };
        }

        private static PlayerMovementConfig RBInsideZone(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role,
                RouteType = RouteType.Run,
                AnimationTrigger = "CarryBall",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(0, 0, 2.0f),            DurationSeconds = 0.35f },
                    new() { PositionOffset = new Vector3(dir * 0.8f, 0, 4.5f),   DurationSeconds = 0.5f  },
                    new() { PositionOffset = new Vector3(dir * 1.5f, 0, 7.0f),   DurationSeconds = 0.6f  }
                }
            };
        }

        private static PlayerMovementConfig RBOutsideZone(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role,
                RouteType = RouteType.Run,
                AnimationTrigger = "CarryBall",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(dir * 0.5f, 0, 1.5f), DurationSeconds = 0.25f },
                    new() { PositionOffset = new Vector3(dir * 2.5f, 0, 3.0f), DurationSeconds = 0.4f  },
                    new() { PositionOffset = new Vector3(dir * 4.0f, 0, 5.0f), DurationSeconds = 0.55f }
                }
            };
        }

        private static PlayerMovementConfig FBLeadBlock(PlayerRole carrierRole, float dir)
        {
            // If carrier is FB, it leads; otherwise FB is separate lead blocker
            return new PlayerMovementConfig
            {
                Role = PlayerRole.FullBack,
                RouteType = RouteType.LeadBlock,
                AnimationTrigger = "LeadBlock",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(dir * 1.0f, 0, 2.0f), DurationSeconds = 0.4f },
                    new() { PositionOffset = new Vector3(dir * 1.5f, 0, 4.0f), DurationSeconds = 0.5f }
                }
            };
        }

        private static PlayerMovementConfig HBPowerRun(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = PlayerRole.HalfBack,
                RouteType = RouteType.Run,
                AnimationTrigger = "CarryBall",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(dir * 0.3f, 0, 2.0f), DurationSeconds = 0.35f },
                    new() { PositionOffset = new Vector3(dir * 1.2f, 0, 4.5f), DurationSeconds = 0.45f },
                    new() { PositionOffset = new Vector3(dir * 2.0f, 0, 7.0f), DurationSeconds = 0.55f }
                }
            };
        }

        private static PlayerMovementConfig QBHandoff()
        {
            return new PlayerMovementConfig
            {
                Role = PlayerRole.Quarterback,
                AnimationTrigger = "Snap",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(0, 0, -0.5f), DurationSeconds = 0.2f },
                    new() { PositionOffset = new Vector3(0, 0, -1.5f), DurationSeconds = 0.4f }
                }
            };
        }

        private static PlayerMovementConfig QBDropback()
        {
            return new PlayerMovementConfig
            {
                Role = PlayerRole.Quarterback,
                AnimationTrigger = "DropBack",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(0, 0, -2.0f), DurationSeconds = 0.3f },
                    new() { PositionOffset = new Vector3(0, 0, -5.0f), DurationSeconds = 0.6f }
                }
            };
        }

        private static PlayerMovementConfig HBCheckRelease()
        {
            return new PlayerMovementConfig
            {
                Role = PlayerRole.HalfBack,
                RouteType = RouteType.Flat,
                AnimationTrigger = "RunRoute",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(1.5f, 0, -0.5f), DurationSeconds = 0.5f },
                    new() { PositionOffset = new Vector3(3.0f, 0,  1.0f), DurationSeconds = 0.5f }
                }
            };
        }

        private static PlayerMovementConfig PassSet(PlayerRole role)
        {
            float lateralKick = (role == PlayerRole.LeftTackle || role == PlayerRole.LeftGuard) ? -0.3f : 0.3f;
            return new PlayerMovementConfig
            {
                Role = role,
                AnimationTrigger = "PassSet",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(lateralKick, 0, -0.8f), DurationSeconds = 0.3f },
                    new() { PositionOffset = new Vector3(lateralKick, 0, -1.3f), DurationSeconds = 0.5f }
                }
            };
        }

        private static PlayerMovementConfig ReceiverStaleLine(PlayerRole role)
        {
            return new PlayerMovementConfig
            {
                Role = role,
                AnimationTrigger = "RunRoute",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(0, 0, 2.5f), DurationSeconds = 0.5f }
                }
            };
        }

        public static PlayerMovementConfig BuildRoute(PlayerRole role, RouteType type, float dir)
        {
            return type switch
            {
                RouteType.Slant  => BuildSlant(role, dir),
                RouteType.Curl   => BuildCurl(role, dir),
                RouteType.Out    => BuildOut(role, dir),
                RouteType.Post   => BuildPost(role, dir),
                RouteType.Fly    => BuildFly(role, dir),
                RouteType.Flat   => BuildFlat(role, dir),
                RouteType.In     => BuildInRoute(role, dir),
                RouteType.Corner => BuildCorner(role, dir),
                RouteType.Seam   => BuildSeam(role, dir),
                RouteType.Drag   => BuildDrag(role, dir),
                RouteType.Wheel  => BuildWheel(role, dir),
                _                => ReceiverStaleLine(role)
            };
        }

        private static PlayerMovementConfig BuildSlant(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role, RouteType = RouteType.Slant, AnimationTrigger = "RunRoute",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(0, 0, 2f),                        DurationSeconds = 0.35f },
                    new() { PositionOffset = new Vector3(dir * -3f, 0, 5f), IsFinalCatchPoint = true, DurationSeconds = 0.55f }
                }
            };
        }
        private static PlayerMovementConfig BuildCurl(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role, RouteType = RouteType.Curl, AnimationTrigger = "RunRoute",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(0, 0, 5f),                        DurationSeconds = 0.65f },
                    new() { PositionOffset = new Vector3(0, 0, 3f), IsFinalCatchPoint = true, DurationSeconds = 0.35f }
                }
            };
        }
        private static PlayerMovementConfig BuildOut(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role, RouteType = RouteType.Out, AnimationTrigger = "RunRoute",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(0, 0, 5f),                              DurationSeconds = 0.65f },
                    new() { PositionOffset = new Vector3(dir * 3f, 0, 5f), IsFinalCatchPoint = true, DurationSeconds = 0.4f }
                }
            };
        }
        private static PlayerMovementConfig BuildPost(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role, RouteType = RouteType.Post, AnimationTrigger = "RunRoute",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(0, 0, 6f),                             DurationSeconds = 0.75f },
                    new() { PositionOffset = new Vector3(dir * -4f, 0, 12f), IsFinalCatchPoint = true, DurationSeconds = 0.7f }
                }
            };
        }
        private static PlayerMovementConfig BuildFly(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role, RouteType = RouteType.Fly, AnimationTrigger = "RunRoute",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(0, 0, 15f), IsFinalCatchPoint = true, DurationSeconds = 1.8f }
                }
            };
        }
        private static PlayerMovementConfig BuildFlat(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role, RouteType = RouteType.Flat, AnimationTrigger = "RunRoute",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(dir * 4f, 0, 1f), IsFinalCatchPoint = true, DurationSeconds = 0.55f }
                }
            };
        }
        private static PlayerMovementConfig BuildInRoute(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role, RouteType = RouteType.In, AnimationTrigger = "RunRoute",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(0, 0, 5f),                              DurationSeconds = 0.65f },
                    new() { PositionOffset = new Vector3(dir * -5f, 0, 5f), IsFinalCatchPoint = true, DurationSeconds = 0.55f }
                }
            };
        }
        private static PlayerMovementConfig BuildCorner(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role, RouteType = RouteType.Corner, AnimationTrigger = "RunRoute",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(0, 0, 6f),                              DurationSeconds = 0.75f },
                    new() { PositionOffset = new Vector3(dir * 4f, 0, 12f), IsFinalCatchPoint = true, DurationSeconds = 0.7f }
                }
            };
        }
        private static PlayerMovementConfig BuildSeam(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role, RouteType = RouteType.Seam, AnimationTrigger = "RunRoute",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(0, 0, 12f), IsFinalCatchPoint = true, DurationSeconds = 1.5f }
                }
            };
        }
        private static PlayerMovementConfig BuildDrag(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role, RouteType = RouteType.Drag, AnimationTrigger = "RunRoute",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(0, 0, 1f),                              DurationSeconds = 0.2f },
                    new() { PositionOffset = new Vector3(dir * -5f, 0, 2f), IsFinalCatchPoint = true, DurationSeconds = 0.65f }
                }
            };
        }
        private static PlayerMovementConfig BuildWheel(PlayerRole role, float dir)
        {
            return new PlayerMovementConfig
            {
                Role = role, RouteType = RouteType.Wheel, AnimationTrigger = "RunRoute",
                Waypoints = new List<RouteWaypoint>
                {
                    new() { PositionOffset = new Vector3(dir * 3f, 0, 0.5f), DurationSeconds = 0.4f },
                    new() { PositionOffset = new Vector3(dir * 3f, 0, 8.0f), IsFinalCatchPoint = true, DurationSeconds = 0.9f }
                }
            };
        }
    }
}
