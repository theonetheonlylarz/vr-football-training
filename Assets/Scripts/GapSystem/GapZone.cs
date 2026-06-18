using UnityEngine;
using UnityEngine.Events;
using FootballTraining.Core;

namespace FootballTraining.GapSystem
{
    /// <summary>
    /// Represents a single gap on the field (A-Left, B-Right, etc.).
    /// Provides a collider for controller raycasting and a visual indicator.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GapZone : MonoBehaviour
    {
        [Header("Gap Identity")]
        public GapLocation Gap;
        public string GapLabel;     // "A", "B", "C", "D"

        [Header("Visuals")]
        [SerializeField] private Renderer _indicatorRenderer;
        [SerializeField] private Color _idleColor      = new Color(0.2f, 0.8f, 1f, 0.3f);
        [SerializeField] private Color _hoveredColor   = new Color(0.2f, 0.8f, 1f, 0.7f);
        [SerializeField] private Color _confirmedColor = new Color(0.1f, 1.0f, 0.2f, 0.8f);
        [SerializeField] private Color _wrongColor     = new Color(1.0f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private GameObject _labelCanvas;

        private static readonly int ColorPropId = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock _propBlock;
        private bool _isActive;

        public UnityEvent<GapZone> OnSelected = new();

        private void Awake()
        {
            _propBlock = new MaterialPropertyBlock();
            SetVisible(false);
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            SetVisible(active);
            if (active) SetColor(_idleColor);
        }

        public void OnHoverEnter()
        {
            if (!_isActive) return;
            SetColor(_hoveredColor);
            transform.localScale = Vector3.one * 1.1f;
        }

        public void OnHoverExit()
        {
            if (!_isActive) return;
            SetColor(_idleColor);
            transform.localScale = Vector3.one;
        }

        public void OnConfirm()
        {
            if (!_isActive) return;
            SetColor(_confirmedColor);
            transform.localScale = Vector3.one * 1.2f;
            OnSelected.Invoke(this);
        }

        public void ShowResult(bool correct)
        {
            SetColor(correct ? _confirmedColor : _wrongColor);
        }

        public void ResetVisual()
        {
            SetColor(_idleColor);
            transform.localScale = Vector3.one;
        }

        private void SetVisible(bool visible)
        {
            if (_indicatorRenderer != null) _indicatorRenderer.enabled = visible;
            if (_labelCanvas != null) _labelCanvas.SetActive(visible);
            GetComponent<Collider>().enabled = visible;
        }

        private void SetColor(Color color)
        {
            if (_indicatorRenderer == null) return;
            _indicatorRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(ColorPropId, color);
            _indicatorRenderer.SetPropertyBlock(_propBlock);
        }
    }
}
