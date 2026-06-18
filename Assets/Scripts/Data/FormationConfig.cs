using UnityEngine;
using System.Collections.Generic;
using FootballTraining.Core;

namespace FootballTraining.Data
{
    [CreateAssetMenu(fileName = "Formation_", menuName = "Football Training/Formation Config")]
    public class FormationConfig : ScriptableObject
    {
        public FormationType FormationType;
        [TextArea(2, 4)] public string CalloutText;   // e.g. "I-Formation, twins right"
        public AudioClip CalloutClip;
        public List<PlayerPositionConfig> Positions = new();

        // Prefab overrides per role (null = use default)
        public GameObject LinemenPrefab;
        public GameObject SkillPlayerPrefab;
        public GameObject QuarterbackPrefab;
    }
}
