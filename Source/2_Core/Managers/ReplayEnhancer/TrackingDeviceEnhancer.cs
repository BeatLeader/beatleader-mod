using BeatLeader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models.Replay;
using UnityEngine.XR;

namespace BeatLeader.Core.Managers.ReplayEnhancer
{
    class TrackingDeviceEnhancer
    {
        private readonly IVRPlatformHelper _vRPlatformHelper;
        private readonly Dictionary<VRPlatformSDK, Action<Replay>> _trackingDataProcessors = new Dictionary<VRPlatformSDK, Action<Replay>>();

        public TrackingDeviceEnhancer(IVRPlatformHelper vRPlatformHelper)
        {
            _vRPlatformHelper = vRPlatformHelper;

            _trackingDataProcessors.Add(VRPlatformSDK.OpenVR, ProcessOpenVR);
            _trackingDataProcessors.Add(VRPlatformSDK.Oculus, ProcessOculus);
            _trackingDataProcessors.Add(VRPlatformSDK.Unknown, ProcessUnknown);
        }
        
        public void Enhance(Replay replay)
        {
            VRPlatformSDK trackingSystem = _vRPlatformHelper.vrPlatformSDK;
            replay.info.trackingSytem = trackingSystem.ToString();
            _trackingDataProcessors[trackingSystem].Invoke(replay);
        }

        private void ProcessOpenVR(Replay replay)
        {
            List<InputDevice> headDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.Head, headDevices);
            replay.info.hmd = headDevices.Count > 0 ? headDevices.First().name : "Unknown";

            List<InputDevice> handDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, handDevices);
            replay.info.controller = handDevices.Count > 0 ? handDevices.First().name : "Unknown";
        }

        private void ProcessOculus(Replay replay) 
        {
            switch (OVRPlugin.GetSystemHeadsetType())
            {
                case OVRPlugin.SystemHeadset.None:
                case OVRPlugin.SystemHeadset.Oculus_Quest:
                case OVRPlugin.SystemHeadset.Oculus_Link_Quest:
                case OVRPlugin.SystemHeadset.Oculus_Quest_2:
                case OVRPlugin.SystemHeadset.Oculus_Link_Quest_2:
                case OVRPlugin.SystemHeadset.Rift_CV1:
                case OVRPlugin.SystemHeadset.Rift_S:
                    replay.info.hmd = OVRPlugin.GetSystemHeadsetType().ToString();
                    break;

                default:
                    replay.info.hmd = "Unknown";
                    break;
            }
            replay.info.controller = "Unknown";
        }

        private void ProcessUnknown(Replay replay)
        {
            replay.info.hmd = "Unknown";
            replay.info.controller = "Unknown";
        }
    }
}
