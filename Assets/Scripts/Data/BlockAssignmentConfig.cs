using UnityEngine;
using FootballTraining.Core;

namespace FootballTraining.Data
{
    [System.Serializable]
    public class BlockAssignmentConfig
    {
        public PlayerRole BlockerRole;
        public BlockingTarget Target;
        // Blocking move direction relative to the player (world-space angle degrees)
        public float BlockAngleDegrees;
        // Pull distance before engaging (for pulling guards)
        public float PullDistanceYards;
        public bool IsPullBlock;
        public string AnimationTrigger;
    }
}
