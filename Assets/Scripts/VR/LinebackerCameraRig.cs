using UnityEngine;
using UnityEngine.XR;

namespace FootballTraining.VR
{
    /// <summary>
    /// Positions the VR camera rig at linebacker depth (~3-5 yards off the ball)
    /// and adjusts height for a realistic on-field perspective.
    /// </summary>
    public class LinebackerCameraRig : MonoBehaviour
    {
        [Header("Positioning")]
        [SerializeField] private Transform _ballPosition;        // Center of LOS
        [SerializeField] private float _depthFromBall = 4.0f;   // yards behind LOS
        [SerializeField] private float _eyeHeight = 1.75f;      // meters

        [Header("Comfort")]
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private bool _lockVerticalLook = false;

        private Transform _xrOrigin;
        private Vector3 _targetPosition;

        private void Awake()
        {
            _xrOrigin = transform;
        }

        private void Start()
        {
            SnapToPosition();
        }

        public void SetBallPosition(Transform ball)
        {
            _ballPosition = ball;
            SnapToPosition();
        }

        public void SnapToPosition()
        {
            if (_ballPosition == null) return;
            Vector3 target = _ballPosition.position - Vector3.forward * _depthFromBall;
            target.y = _eyeHeight;
            transform.position = target;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }

        private void Update()
        {
            if (_ballPosition == null) return;
            Vector3 desired = _ballPosition.position - Vector3.forward * _depthFromBall;
            desired.y = _eyeHeight;
            _targetPosition = desired;
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _smoothSpeed);
        }

        /// <summary>
        /// Adjusts depth so the linebacker can see the full offensive line.
        /// Called when a new formation is loaded.
        /// </summary>
        public void AdjustDepthForFormation(float formationWidth)
        {
            // Wider formations push the LB back slightly for a better view
            float adjustedDepth = Mathf.Lerp(3.5f, 5.5f, (formationWidth - 8f) / 4f);
            _depthFromBall = Mathf.Clamp(adjustedDepth, 3f, 6f);
        }
    }
}
