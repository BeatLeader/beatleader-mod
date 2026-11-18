using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using InputDevice = UnityEngine.XR.InputDevice;

namespace BeatLeader.EyeTracking {
    internal static class OpenXREyeGaze {
        private static readonly InputFeatureUsage<InputTrackingState> gazeTrackingState = new("gazeTrackingState");
        private static readonly List<InputDevice> devices = new();
        private static readonly AngularVelocityYawEstimator angularVelocityYaw = new();
        private static InputDevice _eyeDevice;
        private static InputDevice _headDevice;

        public static bool IsActive { get; private set; }

        public static GazePredictionEstimator GazePrediction { get; } = new();

        public static void UpdateIsActive() {
            IsActive = OpenXRRuntime.IsExtensionEnabled(EyeGazeInteraction.extensionString);
        }

        public static void EnableFeatureBeforeLoaderInitialize() {
            if (!EyeTrackingManager.IsStarted) return;
            var feature = OpenXRSettings.Instance?.GetFeature<EyeGazeInteraction>();
            if (feature != null) feature.enabled = true;
        }

        public static bool TryGetGaze(out Vector3 direction) {
            direction = default;
            if (!IsActive || !TryGetDevices(out var eye, out var head)) return false;
            if (!eye.TryGetFeatureValue(gazeTrackingState, out var trackingState) || (trackingState & InputTrackingState.Rotation) == 0) return false;

            if (!eye.TryGetFeatureValue(EyeTrackingUsages.gazePosition, out var gazePosition) ||
                !eye.TryGetFeatureValue(EyeTrackingUsages.gazeRotation, out var gazeRotation) ||
                !head.TryGetFeatureValue(CommonUsages.centerEyePosition, out var headPosition) ||
                !head.TryGetFeatureValue(CommonUsages.centerEyeRotation, out var headRotation) ||
                !head.TryGetFeatureValue(CommonUsages.deviceVelocity, out var headVelocity) ||
                !head.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out var headAngularVelocity)) return false;

            var time = Time.unscaledTime;
            var inverseHead = Quaternion.Inverse(headRotation);
            GazePrediction.Add(inverseHead * (gazePosition - headPosition), inverseHead * headVelocity, time);
            angularVelocityYaw.Add(headRotation, headAngularVelocity, time);

            // The runtime places the gaze on a head pose predicted further ahead than the one it reports
            var predictedHead = Extrapolate(headRotation, angularVelocityYaw.Apply(headAngularVelocity), GazePrediction.Seconds);
            direction = (Quaternion.Inverse(predictedHead) * gazeRotation * Vector3.forward).normalized;
            return direction.sqrMagnitude > 0.5f;
        }

        private static Quaternion Extrapolate(Quaternion rotation, Vector3 angularVelocity, float seconds) {
            var degrees = angularVelocity.magnitude * seconds * Mathf.Rad2Deg;
            return degrees > 0f ? Quaternion.AngleAxis(degrees, angularVelocity.normalized) * rotation : rotation;
        }

        private static bool TryGetDevices(out InputDevice eye, out InputDevice head) {
            if (!_eyeDevice.isValid || !_headDevice.isValid) {
                InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, devices);
                _eyeDevice = devices.FirstOrDefault(x => (x.characteristics & InputDeviceCharacteristics.EyeTracking) != 0);
                _headDevice = devices.FirstOrDefault(x => (x.characteristics & InputDeviceCharacteristics.EyeTracking) == 0);
            }
            eye = _eyeDevice;
            head = _headDevice;
            return eye.isValid && head.isValid;
        }

        public static string GetStatusText() {
            if (!IsActive) return "<color=#FF8888>unavailable</color>";
            return TryGetGaze(out _) ? "<color=#88FF88>tracking</color>" : "eyes not tracked";
        }
    }
}
