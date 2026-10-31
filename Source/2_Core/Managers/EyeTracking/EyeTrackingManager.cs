using System;
using UnityEngine.XR.OpenXR;

namespace BeatLeader.EyeTracking {
    internal static class EyeTrackingManager {
        public static bool IsSupported { get; private set; }

        public static bool IsStarted { get; private set; }

        public static string Runtime { get; private set; } = "";

        public static OscEyeTrackingReceiver? Receiver { get; private set; }

        public static bool ShouldRecord => IsStarted && ConfigFileData.Instance.RecordEyeTracking;

        public static void Initialize() {
            try {
                var runtimeName = OpenXRRuntime.name ?? "";
                IsSupported = runtimeName.Contains("SteamVR");
                IsStarted = IsSupported && ConfigFileData.Instance.RecordEyeTracking;
                if (!IsStarted) return;

                Runtime = $"{runtimeName} {OpenXRRuntime.version}";
                OpenXREyeGaze.UpdateIsActive();
                Receiver = new OscEyeTrackingReceiver();
            } catch (Exception ex) {
                Plugin.Log.Error($"[EyeTracking] Failed to start: {ex}");
            }
        }

        public static void Dispose() {
            Receiver?.Dispose();
            Receiver = null;
        }

        public static string GetStatusText() {
            if (!ConfigFileData.Instance.RecordEyeTracking) {
                return "For headsets with eye tracking (Steam Frame, Quest Pro over Steam Link). " +
                    "Eye data is saved in your replays and uploaded with them";
            }
            if (!IsSupported) return "Eye tracking is only available with SteamVR";
            if (!IsStarted || Receiver == null) return "Restart the game to start recording";
            return $"OpenXR gaze: {OpenXREyeGaze.GetStatusText()}\nSteam Link OSC: {Receiver.GetStatusText()}";
        }
    }
}
