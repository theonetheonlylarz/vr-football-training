using UnityEngine;
using FootballTraining.Core;

namespace FootballTraining.Data
{
    // Offset from the center of the line of scrimmage (LOS).
    // x = lateral (positive = offense's right), y = height, z = depth (negative = toward own backfield)
    [System.Serializable]
    public class PlayerPositionConfig
    {
        public PlayerRole Role;
        // Position relative to ball placement. 1 Unity unit = 1 yard.
        public Vector3 OffsetFromBall;
        // Euler Y rotation: 0 = facing downfield toward defense (default for offense)
        public float YRotation;

        public PlayerPositionConfig() { }
        public PlayerPositionConfig(PlayerRole role, Vector3 offset, float yRot = 0f)
        {
            Role = role;
            OffsetFromBall = offset;
            YRotation = yRot;
        }
    }
}
