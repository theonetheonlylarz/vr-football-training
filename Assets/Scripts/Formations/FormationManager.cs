using UnityEngine;
using System.Collections.Generic;
using FootballTraining.Data;
using FootballTraining.Players;

namespace FootballTraining.Formations
{
    /// <summary>
    /// Instantiates and manages the active offensive formation.
    /// All player GameObjects are parented under this transform.
    /// </summary>
    public class FormationManager : MonoBehaviour
    {
        [SerializeField] private Transform _formationRoot;
        [SerializeField] private GameObject _defaultLinemenPrefab;
        [SerializeField] private GameObject _defaultSkillPlayerPrefab;
        [SerializeField] private GameObject _defaultQuarterbackPrefab;

        private readonly List<OffensivePlayerController> _activePlayers = new();
        private FormationConfig _currentFormation;

        public IReadOnlyList<OffensivePlayerController> ActivePlayers => _activePlayers;

        public void SpawnFormation(FormationConfig config)
        {
            ClearFormation();
            _currentFormation = config;

            foreach (var posConfig in config.Positions)
            {
                GameObject prefab = SelectPrefab(posConfig.Role, config);
                if (prefab == null)
                {
                    Debug.LogWarning($"[FormationManager] No prefab for role {posConfig.Role}");
                    continue;
                }

                Vector3 worldPos = _formationRoot.position + posConfig.OffsetFromBall;
                Quaternion worldRot = Quaternion.Euler(0f, posConfig.YRotation, 0f);
                GameObject playerObj = Instantiate(prefab, worldPos, worldRot, _formationRoot);
                playerObj.name = $"Player_{posConfig.Role}";

                var controller = playerObj.GetComponent<OffensivePlayerController>();
                if (controller != null)
                {
                    controller.Initialize(posConfig.Role, posConfig.OffsetFromBall);
                    _activePlayers.Add(controller);
                }
            }

            Debug.Log($"[FormationManager] Spawned {_activePlayers.Count} players for {config.FormationType}");
        }

        public void ClearFormation()
        {
            foreach (var player in _activePlayers)
            {
                if (player != null) Destroy(player.gameObject);
            }
            _activePlayers.Clear();
            _currentFormation = null;
        }

        public OffensivePlayerController GetPlayerByRole(Core.PlayerRole role)
        {
            foreach (var p in _activePlayers)
                if (p.Role == role) return p;
            return null;
        }

        private GameObject SelectPrefab(Core.PlayerRole role, FormationConfig config)
        {
            bool isOL = role is Core.PlayerRole.Center
                            or Core.PlayerRole.LeftGuard or Core.PlayerRole.RightGuard
                            or Core.PlayerRole.LeftTackle or Core.PlayerRole.RightTackle
                            or Core.PlayerRole.TightEnd;

            bool isQB = role == Core.PlayerRole.Quarterback;

            if (isQB)
                return config.QuarterbackPrefab != null ? config.QuarterbackPrefab : _defaultQuarterbackPrefab;
            if (isOL)
                return config.LinemenPrefab != null ? config.LinemenPrefab : _defaultLinemenPrefab;
            return config.SkillPlayerPrefab != null ? config.SkillPlayerPrefab : _defaultSkillPlayerPrefab;
        }

        public void ResetAllToPreSnapPositions()
        {
            foreach (var player in _activePlayers)
                player.ResetToStartPosition();
        }
    }
}
