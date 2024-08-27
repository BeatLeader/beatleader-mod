using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models.Replay;
using BeatLeader.SteamVR;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine.XR;
using Zenject;

namespace BeatLeader.Core.Managers.ReplayEnhancer {
    internal class TrackingDeviceEnhancer {
        #region Enhance

        [Inject, UsedImplicitly]
        private IVRPlatformHelper _vrPlatformHelper;

        public void Enhance(Replay replay) {
            var trackingSystem = _vrPlatformHelper.vrPlatformSDK;
            replay.info.trackingSytem = trackingSystem.ToString();

            switch (trackingSystem) {
                case VRPlatformSDK.OpenVR:
                    ProcessOpenVRDevices(replay);
                    ProcessOpenVRSettings(replay);
                    break;
                case VRPlatformSDK.Oculus:
                    ProcessOculusDevices(replay);
                    break;
                case VRPlatformSDK.Unknown:
                    ProcessUnknownDevices(replay);
                    break;
            }
        }

        #endregion

        #region ProcessOpenVR

        private static void ProcessOpenVRDevices(Replay replay) {
            var headDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.Head, headDevices);
            replay.info.hmd = headDevices.Count > 0 ? headDevices.First().name : "Unknown";

            var handDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, handDevices);
            replay.info.controller = handDevices.Count > 0 ? handDevices.First().name : "Unknown";
        }

        private static void ProcessOpenVRSettings(Replay replay) {
            if (replay.info.hmd == "Unknown") {
                replay.info.hmd = (SteamVRSettings.GetString("LastKnown.HMDManufacturer") ?? "Unknown") + (SteamVRSettings.GetString("LastKnown.HMDModel") ?? "");
            }

            if (replay.frames.Count == 0) return;
            var firstFrame = replay.frames[0];
            firstFrame.head.rotation.x = -1.0f;
            firstFrame.head.rotation.y = SteamVRSettings.GetFloatOrDefault("steam.app.620980.worldScale", -1.0f);
            firstFrame.head.rotation.z = SteamVRSettings.GetFloatOrDefault("steam.app.620980.additionalFramesToPredict", -1.0f);
            firstFrame.head.rotation.w = SteamVRSettings.GetFloatOrDefault("steam.app.620980.framesToThrottle", -1.0f);
        }

        #endregion

        #region ProcessOculus

        private static void ProcessOculusDevices(Replay replay) {
            replay.info.hmd = OVRPlugin.GetSystemHeadsetType().ToString();
            if (OVRPlugin.productName.Contains("Quest 3")) {
                replay.info.hmd = "Meta Quest 3";
            }
            if (OVRPlugin.GetActiveController() != null) {
                replay.info.controller = OVRPlugin.GetActiveController().ToString();
            } else {
                replay.info.controller = "Unknown";
            }
        }

        #endregion

        #region ProcessUnknown

        private static void ProcessUnknownDevices(Replay replay) {
            replay.info.hmd = "Unknown";
            replay.info.controller = "Unknown";
        }

        #endregion
    }
}