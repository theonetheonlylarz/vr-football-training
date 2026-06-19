using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

namespace FootballTraining.VR
{
    /// <summary>
    /// Abstracts Meta Quest controller input using Unity XR APIs.
    /// Provides raycasting from the dominant hand and button polling.
    /// </summary>
    public class QuestInputHandler : MonoBehaviour
    {
        [Header("Controller References")]
        [SerializeField] private Transform _rightHandTransform;
        [SerializeField] private Transform _leftHandTransform;
        [SerializeField] private bool _isRightHanded = true;

        [Header("Input Thresholds")]
        [SerializeField] private float _triggerThreshold    = 0.7f;
        [SerializeField] private float _gripThreshold       = 0.7f;

        private InputDevice _rightController;
        private InputDevice _leftController;
        private bool _wasConfirmPressed;
        private bool _primaryButtonDown;
        private bool _prevConfirm;
        private bool _prevPrimary;

        private void OnEnable()
        {
            InputDevices.deviceConnected    += OnDeviceConnected;
            InputDevices.deviceDisconnected += OnDeviceDisconnected;
            InitDevices();
        }

        private void OnDisable()
        {
            InputDevices.deviceConnected    -= OnDeviceConnected;
            InputDevices.deviceDisconnected -= OnDeviceDisconnected;
        }

        private void InitDevices()
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, devices);
            if (devices.Count > 0) _rightController = devices[0];

            devices.Clear();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, devices);
            if (devices.Count > 0) _leftController = devices[0];
        }

        private void OnDeviceConnected(InputDevice device) => InitDevices();
        private void OnDeviceDisconnected(InputDevice device) => InitDevices();

        private void Update()
        {
            // Rising-edge detection: fire only on the frame the button is first pressed.
            bool confirm  = ReadConfirmThisFrame();
            bool primary  = ReadPrimaryThisFrame();

            _wasConfirmPressed  = confirm && !_prevConfirm;
            _primaryButtonDown  = primary && !_prevPrimary;

            _prevConfirm = confirm;
            _prevPrimary = primary;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        public Ray GetDominantHandRay()
        {
            Transform handTf = _isRightHanded ? _rightHandTransform : _leftHandTransform;
            if (handTf == null) return new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            return new Ray(handTf.position, handTf.forward);
        }

        public bool IsConfirmPressed() => _wasConfirmPressed;

        public bool GetTriggerDown()
        {
            InputDevice dominant = _isRightHanded ? _rightController : _leftController;
            if (dominant.TryGetFeatureValue(CommonUsages.trigger, out float val))
                return val >= _triggerThreshold;
            return false;
        }

        public bool GetGripDown()
        {
            InputDevice dominant = _isRightHanded ? _rightController : _leftController;
            if (dominant.TryGetFeatureValue(CommonUsages.grip, out float val))
                return val >= _gripThreshold;
            return false;
        }

        public bool GetPrimaryButtonDown() => _primaryButtonDown;

        public bool GetSecondaryButtonDown()
        {
            InputDevice dominant = _isRightHanded ? _rightController : _leftController;
            if (dominant.TryGetFeatureValue(CommonUsages.secondaryButton, out bool val))
                return val;
            return false;
        }

        public Vector2 GetThumbstick()
        {
            InputDevice dominant = _isRightHanded ? _rightController : _leftController;
            if (dominant.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 val))
                return val;
            return Vector2.zero;
        }

        public bool GetMenuButton()
        {
            if (_leftController.TryGetFeatureValue(CommonUsages.menuButton, out bool val))
                return val;
            return false;
        }

        public void TriggerHapticPulse(bool rightHand, float amplitude = 0.5f, float duration = 0.1f)
        {
            InputDevice device = rightHand ? _rightController : _leftController;
            HapticCapabilities caps;
            if (device.TryGetHapticCapabilities(out caps) && caps.supportsImpulse)
                device.SendHapticImpulse(0, amplitude, duration);
        }

        public void SetHandedness(bool rightHanded) => _isRightHanded = rightHanded;

        private bool ReadPrimaryThisFrame()
        {
            InputDevice dominant = _isRightHanded ? _rightController : _leftController;
            if (dominant.TryGetFeatureValue(CommonUsages.primaryButton, out bool val))
                return val;
            return false;
        }

        private bool ReadConfirmThisFrame()
        {
            // Trigger OR primary button confirms a gap selection
            return GetTriggerDown() || ReadPrimaryThisFrame();
        }
    }
}
