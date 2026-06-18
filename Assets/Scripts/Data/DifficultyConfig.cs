using UnityEngine;
using FootballTraining.Core;

namespace FootballTraining.Data
{
    [CreateAssetMenu(fileName = "Difficulty_", menuName = "Football Training/Difficulty Config")]
    public class DifficultyConfig : ScriptableObject
    {
        public Difficulty Level;

        [Header("Timing")]
        public float PlaySpeedMultiplier = 1f;      // 0.5 = half speed (easy), 1.0 = normal, 1.3 = hard
        public float ReactionWindowSeconds = 3f;    // How long user has to assign gap
        public float PreSnapViewSeconds = 5f;       // Time to read pre-snap before snap delay starts

        [Header("Visual Aids")]
        public bool ShowGapHighlights = true;       // Show gap zone indicators
        public bool ShowPreSnapCues = true;         // Show linebacker read text
        public bool ShowFormationLabel = true;      // Show formation name on HUD

        [Header("Snap Count")]
        public bool UseVariableSnapCount = false;   // Hard mode: don't react on cadence
        public float SnapCountJumpFakeChance = 0f;  // 0-1 probability of a hard count

        [Header("Penalties")]
        public float OffsidesGracePeriod = 0.3f;   // Seconds before snap to lock out early input
    }
}
