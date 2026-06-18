using UnityEngine;
using UnityEngine.XR;
using System;
using System.Collections.Generic;

namespace FootballTraining.VR
{
    /// <summary>
    /// Detects pointing gestures from the dominant hand controller.
    /// A "point" is detected when the trigger is not fully pulled and the
    /// primary button is not pressed (index finger extended).
    /// </summary>
    public class GestureDetector : MonoBehaviour
    {
        [SerializeField] private QuestInputHandler _input;
        [SerializeField] private bool _isRightHanded = true;
        [SerializeField] private float _pointHoldTime = 0.1f;  // seconds to confirm a point

        public event Action OnPointGestureStart;
        public event Action OnPointGestureEnd;
        public event Action OnPinchSelect;

        private bool _isPointing;
        private bool _wasPinching;
        private float _pointTimer;

        private InputDevice _dominantDevice;

        private void OnEnable()
        {
            InputDevices.deviceConnected += _ => RefreshDevice();
            RefreshDevice();
        }

        private void RefreshDevice()
        {
            var devices = new List<InputDevice>();
            var chars = _isRightHanded
                ? InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller
                : InputDeviceCharacteristics.Left  | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(chars, devices);
            if (devices.Count > 0) _dominantDevice = devices[0];
        }

        private void Update()
        {
            if (!_dominantDevice.isValid) return;

            _dominantDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerVal);
            _dominantDevice.TryGetFeatureValue(CommonUsages.grip,    out float gripVal);

            bool fingerExtended = triggerVal < 0.2f;
            bool handOpen       = gripVal    < 0.5f;
            bool currentlyPointing = fingerExtended && handOpen;

            if (currentlyPointing)
            {
                _pointTimer += Time.deltaTime;
                if (!_isPointing && _pointTimer >= _pointHoldTime)
                {
                    _isPointing = true;
                    OnPointGestureStart?.Invoke();
                }
            }
            else
            {
                _pointTimer = 0f;
                if (_isPointing)
                {
                    _isPointing = false;
                    OnPointGestureEnd?.Invoke();
                }
            }

            // Pinch = grip released quickly while trigger rises
            bool isPinching = triggerVal > 0.8f && gripVal < 0.3f;
            if (isPinching && !_wasPinching) OnPinchSelect?.Invoke();
            _wasPinching = isPinching;
        }

        public bool IsPointing => _isPointing;
    }
}
