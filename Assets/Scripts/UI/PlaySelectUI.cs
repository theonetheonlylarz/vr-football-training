using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using FootballTraining.Core;
using FootballTraining.Data;
using FootballTraining.Plays;

namespace FootballTraining.UI
{
    /// <summary>
    /// Play selection screen (Practice Mode only — Test Mode auto-selects plays).
    /// Lets the user filter by play type and pick a specific play, or choose random.
    /// </summary>
    public class PlaySelectUI : MonoBehaviour
    {
        [Header("Filter Buttons")]
        [SerializeField] private Button _allButton;
        [SerializeField] private Button _insideZoneButton;
        [SerializeField] private Button _outsideZoneButton;
        [SerializeField] private Button _powerButton;
        [SerializeField] private Button _passButton;

        [Header("Play List")]
        [SerializeField] private Transform _playListContainer;
        [SerializeField] private GameObject _playEntryPrefab;

        [Header("Random Button")]
        [SerializeField] private Button _randomButton;

        private List<PlayConfig> _allPlays;
        private PlayType? _activeFilter;

        private void Awake()
        {
            _allButton          ?.onClick.AddListener(() => FilterPlays(null));
            _insideZoneButton   ?.onClick.AddListener(() => FilterPlays(PlayType.InsideZone));
            _outsideZoneButton  ?.onClick.AddListener(() => FilterPlays(PlayType.OutsideZone));
            _powerButton        ?.onClick.AddListener(() => FilterPlays(PlayType.Power));
            _passButton         ?.onClick.AddListener(() => FilterPlays(PlayType.Pass));
            _randomButton       ?.onClick.AddListener(SelectRandom);

            GameManager.OnStateChanged += OnStateChanged;
        }

        private void OnDestroy() => GameManager.OnStateChanged -= OnStateChanged;

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.PlaySelect)
            {
                // Test mode: skip UI and auto-pick
                if (GameManager.Instance?.ActiveMode == TrainingMode.Test)
                {
                    SelectRandom();
                    return;
                }
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _allPlays ??= PlayCatalog.BuildAll();
            FilterPlays(_activeFilter);
        }

        public void Hide() => gameObject.SetActive(false);

        private void FilterPlays(PlayType? filter)
        {
            _activeFilter = filter;
            RefreshList(filter.HasValue ? PlayCatalog.GetPlaysByType(filter.Value) : _allPlays);
        }

        private void RefreshList(List<PlayConfig> plays)
        {
            foreach (Transform child in _playListContainer) Destroy(child.gameObject);

            foreach (var play in plays)
            {
                var entry = Instantiate(_playEntryPrefab, _playListContainer);
                var label = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = play.PlayName;
                var btn = entry.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    var captured = play;
                    btn.onClick.AddListener(() => SelectPlay(captured));
                }
            }
        }

        private void SelectPlay(PlayConfig play)
        {
            Hide();
            GameManager.Instance?.LoadPlay(play);
        }

        private void SelectRandom()
        {
            _allPlays ??= PlayCatalog.BuildAll();
            if (_allPlays.Count == 0) return;

            // Weight toward run plays slightly more often in early sessions
            var pool = _activeFilter.HasValue
                ? PlayCatalog.GetPlaysByType(_activeFilter.Value)
                : _allPlays;

            if (pool.Count == 0) pool = _allPlays;
            SelectPlay(pool[Random.Range(0, pool.Count)]);
        }
    }
}
