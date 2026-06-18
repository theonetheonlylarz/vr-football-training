using UnityEngine;

namespace FootballTraining.Utility
{
    /// <summary>
    /// Keeps a world-space UI canvas billboard-facing the main camera (or XR HMD).
    /// Used for floating HUD elements and the result panel.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class WorldSpaceCanvas : MonoBehaviour
    {
        [SerializeField] private bool _billboard = true;
        [SerializeField] private bool _fixedDistance = true;
        [SerializeField] private float _distance = 2.0f;
        [SerializeField] private float _heightOffset = 0f;
        [SerializeField] private float _followSmoothing = 5f;

        private Transform _camera;
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
        }

        private void Start()
        {
            _camera = Camera.main?.transform;
        }

        private void LateUpdate()
        {
            if (_camera == null) return;

            if (_fixedDistance)
            {
                Vector3 forward = _camera.forward;
                forward.y = 0;
                forward.Normalize();

                Vector3 target = _camera.position + forward * _distance + Vector3.up * _heightOffset;
                transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * _followSmoothing);
            }

            if (_billboard)
            {
                Vector3 lookDir = transform.position - _camera.position;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
    }
}
