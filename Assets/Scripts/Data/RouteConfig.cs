using UnityEngine;
using System.Collections.Generic;
using FootballTraining.Core;

namespace FootballTraining.Data
{
    [System.Serializable]
    public class RouteWaypoint
    {
        // Offset from player's starting position
        public Vector3 PositionOffset;
        // How long to reach this waypoint from the previous one (seconds at normal speed)
        public float DurationSeconds;
        public bool IsFinalCatchPoint;
    }

    [System.Serializable]
    public class PlayerMovementConfig
    {
        public PlayerRole Role;
        public RouteType RouteType;
        public List<RouteWaypoint> Waypoints = new();
        public string AnimationTrigger;     // Animator trigger name
        public float MoveSpeed = 5f;        // yards per second
    }
}
