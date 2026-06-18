using UnityEngine;
using System.Collections.Generic;
using FootballTraining.Core;

namespace FootballTraining.Data
{
    [CreateAssetMenu(fileName = "Play_", menuName = "Football Training/Play Config")]
    public class PlayConfig : ScriptableObject
    {
        [Header("Identity")]
        public string PlayName;
        public PlayType PlayType;

        [Header("Formation")]
        public FormationConfig Formation;

        [Header("Ball Carrier / Target")]
        public PlayerRole BallCarrierRole;
        // Initial direction of the run (angle from straight ahead, positive = right)
        public float RunDirectionAngle;
        // Intended gap for run plays
        public GapLocation IntendedGap;

        [Header("Blocking Scheme")]
        public List<BlockAssignmentConfig> BlockAssignments = new();

        [Header("Player Movements After Snap")]
        public List<PlayerMovementConfig> PlayerMovements = new();

        [Header("Snap")]
        public float SnapDelayMin = 0.5f;
        public float SnapDelayMax = 2.5f;
        public string HuddleBreakCallout;   // "Red 80, set, hut!"

        [Header("Timing")]
        public float PlayDurationSeconds = 4f;

        [Header("Pre-Snap Keys for Linebacker")]
        [TextArea(2, 5)]
        public string LinebackerReadCue;    // Shown in HUD pre-snap
    }
}
