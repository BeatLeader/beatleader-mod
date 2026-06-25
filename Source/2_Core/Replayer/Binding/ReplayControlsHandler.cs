using BeatLeader.Models;
using UnityEngine;
using UnityEngine.XR;
using Zenject;

namespace BeatLeader.Replayer.Binding {
    public class ReplayControlsHandler : MonoBehaviour {
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly IBeatmapTimeController _timeController = null!;

        #region Constants

        private const float ActivateThreshold = 0.8f;
        private const float ReleaseThreshold = 0.5f;
        private const float InitialRepeatDelay = 0.4f;
        private const float RepeatInterval = 0.18f;

        #endregion

        #region Input

        private enum AxisDirection {
            None,
            Positive,
            Negative
        }

        private InputDevice _leftDevice;
        private InputDevice _rightDevice;

        private AxisDirection _seekState;
        private float _seekNextActionTime;
        private AxisDirection _speedState;
        private float _speedNextActionTime;

        private void Update() {
            var settings = _launchData.Settings.UISettings.Controls;
            if (settings is not { Enabled: true }) return;

            var seekNode = settings.SeekOnRightHand ? XRNode.RightHand : XRNode.LeftHand;
            var speedNode = settings.SeekOnRightHand ? XRNode.LeftHand : XRNode.RightHand;

            if (TryGetThumbstick(seekNode, out var seekAxis)) {
                var direction = ProcessAxis(seekAxis.x, ref _seekState, ref _seekNextActionTime);
                if (direction != 0) ReplayControlsActions.Seek(_timeController, settings, direction);
            } else {
                _seekState = AxisDirection.None;
            }

            if (TryGetThumbstick(speedNode, out var speedAxis)) {
                var direction = ProcessAxis(speedAxis.y, ref _speedState, ref _speedNextActionTime);
                if (direction != 0) ReplayControlsActions.ChangeSpeed(_timeController, settings, direction);
            } else {
                _speedState = AxisDirection.None;
            }
        }

        private static int ProcessAxis(float value, ref AxisDirection state, ref float nextActionTime) {
            var now = Time.unscaledTime;
            switch (state) {
                case AxisDirection.None:
                    if (value >= ActivateThreshold) {
                        state = AxisDirection.Positive;
                        nextActionTime = now + InitialRepeatDelay;
                        return 1;
                    }
                    if (value <= -ActivateThreshold) {
                        state = AxisDirection.Negative;
                        nextActionTime = now + InitialRepeatDelay;
                        return -1;
                    }
                    return 0;

                case AxisDirection.Positive:
                    if (value < ReleaseThreshold) {
                        state = AxisDirection.None;
                        return 0;
                    }
                    if (now >= nextActionTime) {
                        nextActionTime = now + RepeatInterval;
                        return 1;
                    }
                    return 0;

                case AxisDirection.Negative:
                    if (value > -ReleaseThreshold) {
                        state = AxisDirection.None;
                        return 0;
                    }
                    if (now >= nextActionTime) {
                        nextActionTime = now + RepeatInterval;
                        return -1;
                    }
                    return 0;

                default:
                    return 0;
            }
        }

        private bool TryGetThumbstick(XRNode node, out Vector2 axis) {
            var isLeft = node == XRNode.LeftHand;
            var device = isLeft ? _leftDevice : _rightDevice;
            if (!device.isValid) {
                device = InputDevices.GetDeviceAtXRNode(node);
                if (isLeft) _leftDevice = device;
                else _rightDevice = device;
            }
            if (device.isValid && device.TryGetFeatureValue(CommonUsages.primary2DAxis, out axis)) {
                return true;
            }
            axis = Vector2.zero;
            return false;
        }

        #endregion
    }
}
