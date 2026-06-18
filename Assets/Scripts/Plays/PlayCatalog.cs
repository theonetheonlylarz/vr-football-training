using UnityEngine;
using System.Collections.Generic;
using FootballTraining.Core;
using FootballTraining.Data;
using FootballTraining.Formations;

namespace FootballTraining.Plays
{
    /// <summary>
    /// Builds the complete set of built-in PlayConfig objects at runtime.
    /// Each play references a FormationConfig from FormationLibrary.
    /// </summary>
    public static class PlayCatalog
    {
        private static Dictionary<FormationType, FormationConfig> _formations;

        private static Dictionary<FormationType, FormationConfig> Formations
        {
            get
            {
                _formations ??= FormationLibrary.BuildAll();
                return _formations;
            }
        }

        public static List<PlayConfig> BuildAll()
        {
            var plays = new List<PlayConfig>();
            plays.AddRange(BuildInsideZonePlays());
            plays.AddRange(BuildOutsideZonePlays());
            plays.AddRange(BuildPowerPlays());
            plays.AddRange(BuildPassPlays());
            return plays;
        }

        public static List<PlayConfig> GetPlaysByType(PlayType type)
        {
            return BuildAll().FindAll(p => p.PlayType == type);
        }

        // ── Inside Zone ──────────────────────────────────────────────────────────

        private static List<PlayConfig> BuildInsideZonePlays()
        {
            return new List<PlayConfig>
            {
                MakePlay("IZ Strong Right", PlayType.InsideZone, FormationType.IFormation,
                    PlayerRole.HalfBack, 5f, GapLocation.BRight,
                    "Heavy strong-side alignment: expect inside zone through the B-gap right",
                    BlocksForIZ(toRight: true)),

                MakePlay("IZ Weak Left", PlayType.InsideZone, FormationType.Singleback,
                    PlayerRole.HalfBack, -5f, GapLocation.BLeft,
                    "Singleback set, one tight end: inside zone to the weak side",
                    BlocksForIZ(toRight: false)),

                MakePlay("IZ Pistol A-Gap", PlayType.InsideZone, FormationType.Pistol,
                    PlayerRole.HalfBack, 0f, GapLocation.ARight,
                    "Pistol with tight split: look for A-gap cutback",
                    BlocksForIZ(toRight: true)),
            };
        }

        // ── Outside Zone ─────────────────────────────────────────────────────────

        private static List<PlayConfig> BuildOutsideZonePlays()
        {
            return new List<PlayConfig>
            {
                MakePlay("OZ Stretch Right", PlayType.OutsideZone, FormationType.IFormation,
                    PlayerRole.HalfBack, 25f, GapLocation.CRight,
                    "Full stretch right: TE releases outside, honor the C-gap and spill contain",
                    BlocksForOZ(toRight: true)),

                MakePlay("OZ Stretch Left", PlayType.OutsideZone, FormationType.ProSet,
                    PlayerRole.HalfBack, -25f, GapLocation.CLeft,
                    "Pro set stretch left: stay in your gap, don't over-pursue",
                    BlocksForOZ(toRight: false)),
            };
        }

        // ── Power ────────────────────────────────────────────────────────────────

        private static List<PlayConfig> BuildPowerPlays()
        {
            return new List<PlayConfig>
            {
                MakePlay("Power Right", PlayType.Power, FormationType.IFormation,
                    PlayerRole.HalfBack, 5f, GapLocation.BRight,
                    "I-Form power: pulling guard from left, FB leads through B-gap right",
                    BlocksForPower(toRight: true)),

                MakePlay("Power Left", PlayType.Power, FormationType.ProSet,
                    PlayerRole.HalfBack, -5f, GapLocation.BLeft,
                    "Pro Set power: guard pulls from right side, read the kick-out block",
                    BlocksForPower(toRight: false)),

                MakePlay("Counter H", PlayType.Power, FormationType.Singleback,
                    PlayerRole.HalfBack, -5f, GapLocation.BLeft,
                    "Counter: initial motion right, ball cuts back left — don't false step",
                    BlocksForCounter()),
            };
        }

        // ── Pass ──────────────────────────────────────────────────────────────────

        private static List<PlayConfig> BuildPassPlays()
        {
            return new List<PlayConfig>
            {
                MakePassPlay("Curl Flat Combo", FormationType.Shotgun,
                    "Shotgun empty: curl-flat concept — cover 2 read, honor the flat",
                    new[] {
                        (PlayerRole.WideReceiverLeft,  RouteType.Curl,  -1f),
                        (PlayerRole.WideReceiverRight, RouteType.Flat,   1f),
                        (PlayerRole.SlotReceiver,      RouteType.Seam,   1f),
                    }),

                MakePassPlay("Four Verts", FormationType.Shotgun,
                    "Four verticals: identify #1 and #2 threats to your zone, no easy seams",
                    new[] {
                        (PlayerRole.WideReceiverLeft,  RouteType.Fly,  -1f),
                        (PlayerRole.WideReceiverRight, RouteType.Fly,   1f),
                        (PlayerRole.SlotReceiver,      RouteType.Seam,  1f),
                        (PlayerRole.TightEnd,          RouteType.Seam,  1f),
                    }),

                MakePassPlay("Mesh Crossers", FormationType.Singleback,
                    "Crossing routes: two drags will cross at 3-4 yards — read levels",
                    new[] {
                        (PlayerRole.WideReceiverLeft,  RouteType.Drag,  1f),
                        (PlayerRole.WideReceiverRight, RouteType.Drag, -1f),
                        (PlayerRole.SlotReceiver,      RouteType.Curl, -1f),
                    }),

                MakePassPlay("Boot Waggle", FormationType.IFormation,
                    "Bootleg action: QB fakes run, rolls away — stay disciplined, no QB scramble",
                    new[] {
                        (PlayerRole.TightEnd,          RouteType.Flat,  1f),
                        (PlayerRole.WideReceiverRight, RouteType.Corner, 1f),
                        (PlayerRole.WideReceiverLeft,  RouteType.Post,  -1f),
                    }),

                MakePassPlay("Slant Shoot", FormationType.TripsRight,
                    "Trips right slant-shoot: quick three-man surface, cover 2/cover 3 indicator",
                    new[] {
                        (PlayerRole.WideReceiverRight, RouteType.Slant, -1f),
                        (PlayerRole.SlotReceiver,      RouteType.Flat,   1f),
                        (PlayerRole.TightEnd,          RouteType.In,    -1f),
                    }),

                MakePassPlay("Post Corner", FormationType.ProSet,
                    "Post-corner hi-lo: TE seam, WR post-corner — track the two-level stretch",
                    new[] {
                        (PlayerRole.WideReceiverLeft,  RouteType.Post,  -1f),
                        (PlayerRole.WideReceiverRight, RouteType.Corner, 1f),
                        (PlayerRole.TightEnd,          RouteType.Seam,   1f),
                    }),

                MakePassPlay("Wheel Route", FormationType.Singleback,
                    "HB wheel: back leaks out of the backfield into a vertical — don't lose him",
                    new[] {
                        (PlayerRole.HalfBack,          RouteType.Wheel,  1f),
                        (PlayerRole.WideReceiverLeft,  RouteType.Curl,  -1f),
                        (PlayerRole.WideReceiverRight, RouteType.Out,    1f),
                    }),
            };
        }

        // ── Blocking scheme helpers ───────────────────────────────────────────────

        private static List<BlockAssignmentConfig> BlocksForIZ(bool toRight)
        {
            float dir = toRight ? 1f : -1f;
            return new List<BlockAssignmentConfig>
            {
                new() { BlockerRole = PlayerRole.Center,      Target = BlockingTarget.ALeftGap,  BlockAngleDegrees = dir * 5f  },
                new() { BlockerRole = PlayerRole.LeftGuard,   Target = BlockingTarget.BLeftGap,  BlockAngleDegrees = dir * 15f },
                new() { BlockerRole = PlayerRole.RightGuard,  Target = BlockingTarget.BRightGap, BlockAngleDegrees = dir * 15f },
                new() { BlockerRole = PlayerRole.LeftTackle,  Target = BlockingTarget.CLeftGap,  BlockAngleDegrees = dir * 20f },
                new() { BlockerRole = PlayerRole.RightTackle, Target = BlockingTarget.CRightGap, BlockAngleDegrees = dir * 20f },
                new() { BlockerRole = PlayerRole.TightEnd,    Target = BlockingTarget.Linebacker, BlockAngleDegrees = dir * 10f },
            };
        }

        private static List<BlockAssignmentConfig> BlocksForOZ(bool toRight)
        {
            float dir = toRight ? 1f : -1f;
            return new List<BlockAssignmentConfig>
            {
                new() { BlockerRole = PlayerRole.Center,      Target = BlockingTarget.ALeftGap,  BlockAngleDegrees = dir * 30f },
                new() { BlockerRole = PlayerRole.LeftGuard,   Target = BlockingTarget.BLeftGap,  BlockAngleDegrees = dir * 30f },
                new() { BlockerRole = PlayerRole.RightGuard,  Target = BlockingTarget.BRightGap, BlockAngleDegrees = dir * 30f },
                new() { BlockerRole = PlayerRole.LeftTackle,  Target = BlockingTarget.CLeftGap,  BlockAngleDegrees = dir * 35f },
                new() { BlockerRole = PlayerRole.RightTackle, Target = BlockingTarget.CRightGap, BlockAngleDegrees = dir * 35f },
                new() { BlockerRole = PlayerRole.TightEnd,    Target = BlockingTarget.KickOut,   BlockAngleDegrees = dir * 45f },
            };
        }

        private static List<BlockAssignmentConfig> BlocksForPower(bool toRight)
        {
            float dir = toRight ? 1f : -1f;
            return new List<BlockAssignmentConfig>
            {
                new() { BlockerRole = PlayerRole.Center,      Target = BlockingTarget.ALeftGap,   BlockAngleDegrees = dir * 10f },
                new() { BlockerRole = PlayerRole.LeftGuard,   Target = BlockingTarget.BLeftGap,   BlockAngleDegrees = dir * 10f,
                        IsPullBlock = !toRight, PullDistanceYards = toRight ? 0 : 4f },
                new() { BlockerRole = PlayerRole.RightGuard,  Target = BlockingTarget.BRightGap,  BlockAngleDegrees = dir * 10f,
                        IsPullBlock = toRight,  PullDistanceYards = toRight ? 4f : 0 },
                new() { BlockerRole = PlayerRole.LeftTackle,  Target = BlockingTarget.Cutoff,     BlockAngleDegrees = dir * -20f },
                new() { BlockerRole = PlayerRole.RightTackle, Target = BlockingTarget.CRightGap,  BlockAngleDegrees = dir * 15f },
                new() { BlockerRole = PlayerRole.TightEnd,    Target = BlockingTarget.KickOut,    BlockAngleDegrees = dir * 40f },
                new() { BlockerRole = PlayerRole.FullBack,    Target = BlockingTarget.LeadThrough,BlockAngleDegrees = dir * 5f  },
            };
        }

        private static List<BlockAssignmentConfig> BlocksForCounter()
        {
            return new List<BlockAssignmentConfig>
            {
                new() { BlockerRole = PlayerRole.Center,      Target = BlockingTarget.ALeftGap,   BlockAngleDegrees = -10f },
                new() { BlockerRole = PlayerRole.LeftGuard,   Target = BlockingTarget.BLeftGap,   IsPullBlock = true, PullDistanceYards = 5f, BlockAngleDegrees = -15f },
                new() { BlockerRole = PlayerRole.RightGuard,  Target = BlockingTarget.BRightGap,  IsPullBlock = true, PullDistanceYards = 3f, BlockAngleDegrees = -15f },
                new() { BlockerRole = PlayerRole.LeftTackle,  Target = BlockingTarget.Cutoff,     BlockAngleDegrees = 20f },
                new() { BlockerRole = PlayerRole.RightTackle, Target = BlockingTarget.CRightGap,  BlockAngleDegrees = -10f },
                new() { BlockerRole = PlayerRole.TightEnd,    Target = BlockingTarget.KickOut,    BlockAngleDegrees = -30f },
            };
        }

        // ── Factory helpers ───────────────────────────────────────────────────────

        private static PlayConfig MakePlay(string name, PlayType type, FormationType formation,
            PlayerRole ballCarrier, float runAngle, GapLocation gap, string readCue,
            List<BlockAssignmentConfig> blocks)
        {
            var cfg = ScriptableObject.CreateInstance<PlayConfig>();
            cfg.name = $"Play_{name.Replace(" ", "_")}";
            cfg.PlayName = name;
            cfg.PlayType = type;
            cfg.Formation = Formations[formation];
            cfg.BallCarrierRole = ballCarrier;
            cfg.RunDirectionAngle = runAngle;
            cfg.IntendedGap = gap;
            cfg.LinebackerReadCue = readCue;
            cfg.BlockAssignments = blocks;
            cfg.SnapDelayMin = 0.8f;
            cfg.SnapDelayMax = 2.5f;
            cfg.PlayDurationSeconds = 3.5f;
            return cfg;
        }

        private static PlayConfig MakePassPlay(string name, FormationType formation, string readCue,
            (PlayerRole role, RouteType route, float dir)[] routes)
        {
            var cfg = ScriptableObject.CreateInstance<PlayConfig>();
            cfg.name = $"Play_{name.Replace(" ", "_")}";
            cfg.PlayName = name;
            cfg.PlayType = PlayType.Pass;
            cfg.Formation = Formations[formation];
            cfg.IntendedGap = GapLocation.None;
            cfg.LinebackerReadCue = readCue;
            cfg.BlockAssignments = new List<BlockAssignmentConfig>
            {
                new() { BlockerRole = PlayerRole.Center,      Target = BlockingTarget.PassPro },
                new() { BlockerRole = PlayerRole.LeftGuard,   Target = BlockingTarget.PassPro },
                new() { BlockerRole = PlayerRole.RightGuard,  Target = BlockingTarget.PassPro },
                new() { BlockerRole = PlayerRole.LeftTackle,  Target = BlockingTarget.PassPro },
                new() { BlockerRole = PlayerRole.RightTackle, Target = BlockingTarget.PassPro },
            };

            cfg.PlayerMovements = new List<PlayerMovementConfig>();
            foreach (var (role, route, dir) in routes)
                cfg.PlayerMovements.Add(PlayLibrary.BuildRoute(role, route, dir));

            cfg.SnapDelayMin = 1.0f;
            cfg.SnapDelayMax = 3.0f;
            cfg.PlayDurationSeconds = 4.5f;
            return cfg;
        }
    }
}
