using UnityEngine;
using System.Collections.Generic;
using System;
using FootballTraining.Core;
using FootballTraining.VR;

namespace FootballTraining.GapSystem
{
    /// <summary>
    /// Manages the gap selection UI and input. Opens/closes the selection window,
    /// tracks the user's pointing gesture, and fires the selection event.
    /// </summary>
    public class GapAssignmentManager : MonoBehaviour
    {
        [Header("Gap Zones")]
        [SerializeField] private List<GapZone> _gapZones = new();

        [Header("Input")]
        [SerializeField] private QuestInputHandler _inputHandler;

        [Header("Pointer")]
        [SerializeField] private LineRenderer _pointerLine;
        [SerializeField] private float _pointerLength = 10f;
        [SerializeField] private LayerMask _gapLayerMask;

        private bool _windowOpen;
        private GapZone _hoveredZone;
        private float _windowCloseTime;

        public event Action<GapLocation, float> OnGapSelected;  // gap, reaction time
        public bool IsWindowOpen => _windowOpen;

        private float _windowOpenTime;

        private void Awake()
        {
            if (_inputHandler == null) _inputHandler = FindObjectOfType<QuestInputHandler>();
            foreach (var z in _gapZones) z.OnSelected.AddListener(HandleZoneSelected);
            SetPointerVisible(false);
        }

        public void OpenSelectionWindow(float durationSeconds)
        {
            _windowOpen = true;
            _windowOpenTime = Time.time;
            _windowCloseTime = Time.time + durationSeconds;

            foreach (var z in _gapZones) z.SetActive(true);
            SetPointerVisible(true);
        }

        public void CloseSelectionWindow()
        {
            _windowOpen = false;
            foreach (var z in _gapZones)
            {
                z.SetActive(false);
                z.ResetVisual();
            }
            SetPointerVisible(false);
            _hoveredZone = null;
        }

        public void ShowResult(GapLocation correctGap, GapLocation selectedGap)
        {
            foreach (var z in _gapZones)
            {
                if (z.Gap == correctGap) z.ShowResult(true);
                else if (z.Gap == selectedGap && selectedGap != correctGap) z.ShowResult(false);
            }
        }

        private void Update()
        {
            if (!_windowOpen) return;

            if (Time.time >= _windowCloseTime)
            {
                // Time's up — submit no selection
                CloseSelectionWindow();
                OnGapSelected?.Invoke(GapLocation.None, _windowCloseTime - _windowOpenTime);
                return;
            }

            UpdatePointerAndHover();

            if (_inputHandler != null && _inputHandler.IsConfirmPressed())
            {
                _hoveredZone?.OnConfirm();
            }
        }

        private void UpdatePointerAndHover()
        {
            if (_inputHandler == null) return;

            Ray ray = _inputHandler.GetDominantHandRay();
            GapZone newHover = null;

            if (Physics.Raycast(ray, out RaycastHit hit, _pointerLength, _gapLayerMask))
            {
                newHover = hit.collider.GetComponentInParent<GapZone>();
                UpdatePointerLine(ray.origin, hit.point);
            }
            else
            {
                UpdatePointerLine(ray.origin, ray.origin + ray.direction * _pointerLength);
            }

            if (newHover != _hoveredZone)
            {
                _hoveredZone?.OnHoverExit();
                _hoveredZone = newHover;
                _hoveredZone?.OnHoverEnter();
            }
        }

        private void HandleZoneSelected(GapZone zone)
        {
            if (!_windowOpen) return;
            float reactionTime = Time.time - _windowOpenTime;
            CloseSelectionWindow();
            OnGapSelected?.Invoke(zone.Gap, reactionTime);
        }

        private void UpdatePointerLine(Vector3 start, Vector3 end)
        {
            if (_pointerLine == null) return;
            _pointerLine.SetPosition(0, start);
            _pointerLine.SetPosition(1, end);
        }

        private void SetPointerVisible(bool visible)
        {
            if (_pointerLine != null) _pointerLine.enabled = visible;
        }

        /// <summary>
        /// Positions gap zones relative to a set of player transforms to match
        /// the actual gaps between linemen.
        /// </summary>
        public void AlignGapsToFormation(
            Transform lLT, Transform lLG, Transform center,
            Transform lRG, Transform lRT, Transform te)
        {
            SetGapPosition(GapLocation.ALeft,  MidPoint(lLG.position,  center.position));
            SetGapPosition(GapLocation.ARight, MidPoint(center.position, lRG.position));
            SetGapPosition(GapLocation.BLeft,  MidPoint(lLT.position,  lLG.position));
            SetGapPosition(GapLocation.BRight, MidPoint(lRG.position,  lRT.position));
            SetGapPosition(GapLocation.CLeft,  lLT.position + Vector3.left * 0.8f);
            SetGapPosition(GapLocation.CRight, lRT.position + Vector3.right * 0.8f);
            if (te != null)
            {
                SetGapPosition(GapLocation.DLeft,  te.position + Vector3.left  * 0.8f);
                SetGapPosition(GapLocation.DRight, te.position + Vector3.right * 0.8f);
            }
        }

        private void SetGapPosition(GapLocation gap, Vector3 worldPos)
        {
            foreach (var z in _gapZones)
                if (z.Gap == gap) { z.transform.position = worldPos; return; }
        }

        private static Vector3 MidPoint(Vector3 a, Vector3 b) => (a + b) * 0.5f;
    }
}
