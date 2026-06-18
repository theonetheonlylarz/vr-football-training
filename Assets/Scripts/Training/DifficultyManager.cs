using UnityEngine;
using System.Collections.Generic;
using FootballTraining.Core;
using FootballTraining.Data;

namespace FootballTraining.Training
{
    /// <summary>
    /// Holds DifficultyConfig assets and provides the active config.
    /// If no ScriptableObject assets are assigned, builds defaults in code.
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {
        [SerializeField] private DifficultyConfig _easyConfig;
        [SerializeField] private DifficultyConfig _mediumConfig;
        [SerializeField] private DifficultyConfig _hardConfig;

        private void Awake()
        {
            // Build defaults if assets weren't assigned in Inspector
            _easyConfig   ??= BuildDefault(Difficulty.Easy);
            _mediumConfig ??= BuildDefault(Difficulty.Medium);
            _hardConfig   ??= BuildDefault(Difficulty.Hard);
        }

        public DifficultyConfig GetConfig(Difficulty level) => level switch
        {
            Difficulty.Easy   => _easyConfig,
            Difficulty.Medium => _mediumConfig,
            Difficulty.Hard   => _hardConfig,
            _                 => _mediumConfig
        };

        private static DifficultyConfig BuildDefault(Difficulty level)
        {
            var cfg = ScriptableObject.CreateInstance<DifficultyConfig>();
            cfg.Level = level;
            switch (level)
            {
                case Difficulty.Easy:
                    cfg.PlaySpeedMultiplier       = 0.6f;
                    cfg.ReactionWindowSeconds     = 5.0f;
                    cfg.PreSnapViewSeconds        = 6.0f;
                    cfg.ShowGapHighlights         = true;
                    cfg.ShowPreSnapCues           = true;
                    cfg.ShowFormationLabel        = true;
                    cfg.UseVariableSnapCount      = false;
                    cfg.SnapCountJumpFakeChance   = 0f;
                    cfg.OffsidesGracePeriod       = 0.5f;
                    break;

                case Difficulty.Medium:
                    cfg.PlaySpeedMultiplier       = 1.0f;
                    cfg.ReactionWindowSeconds     = 3.0f;
                    cfg.PreSnapViewSeconds        = 4.0f;
                    cfg.ShowGapHighlights         = true;
                    cfg.ShowPreSnapCues           = true;
                    cfg.ShowFormationLabel        = false;
                    cfg.UseVariableSnapCount      = false;
                    cfg.SnapCountJumpFakeChance   = 0.1f;
                    cfg.OffsidesGracePeriod       = 0.3f;
                    break;

                case Difficulty.Hard:
                    cfg.PlaySpeedMultiplier       = 1.3f;
                    cfg.ReactionWindowSeconds     = 1.5f;
                    cfg.PreSnapViewSeconds        = 2.5f;
                    cfg.ShowGapHighlights         = false;
                    cfg.ShowPreSnapCues           = false;
                    cfg.ShowFormationLabel        = false;
                    cfg.UseVariableSnapCount      = true;
                    cfg.SnapCountJumpFakeChance   = 0.25f;
                    cfg.OffsidesGracePeriod       = 0.2f;
                    break;
            }
            return cfg;
        }
    }
}
