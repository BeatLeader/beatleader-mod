﻿using System.Collections.Generic;
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
                case VRPlatformSDK.OpenXR:
                    ProcessOpenXRControllers(replay);
                    ProcessOpenXRHeadsetWithFallback(replay);
                    ProcessOpenVRSettings(replay);
                    
                    break;
                case VRPlatformSDK.Unknown:
                    ProcessUnknownDevices(replay);
                    break;
            }
        }

        #endregion

        #region ProcessOpenVR

        private static void ProcessOpenXRControllers(Replay replay) {
            var handDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, handDevices);
            replay.info.controller = handDevices.Count > 0 ? handDevices.First().name : "Unknown";
        }

        private static void ProcessOpenXRHeadsetWithFallback(Replay replay) {
            if (SteamVRSettings.GetString("LastKnown.HMDManufacturer") != null) {
                replay.info.hmd = (SteamVRSettings.GetString("LastKnown.HMDManufacturer") ?? "Unknown") + (SteamVRSettings.GetString("LastKnown.HMDModel") ?? "");
            } else if (OpenXRAcquirer.SystemName != null) {
                replay.info.hmd = OpenXRAcquirer.SystemName ?? "Unknown";
            } else {
                var headDevices = new List<InputDevice>();
                InputDevices.GetDevicesAtXRNode(XRNode.Head, headDevices);
                replay.info.hmd = headDevices.Count > 0 ? headDevices.First().name : "Unknown";
            }
        }

        private static void ProcessOpenVRSettings(Replay replay) {
            if (replay.frames.Count == 0 || !SteamVRSettings.IsAvailable()) return;
            var firstFrame = replay.frames[0];
            firstFrame.head.rotation.x = -1.0f;
            firstFrame.head.rotation.y = SteamVRSettings.GetFloatOrDefault("steam.app.620980.worldScale", -1.0f);
            firstFrame.head.rotation.z = SteamVRSettings.GetFloatOrDefault("steam.app.620980.additionalFramesToPredict", -1.0f);
            firstFrame.head.rotation.w = SteamVRSettings.GetFloatOrDefault("steam.app.620980.framesToThrottle", -1.0f);
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