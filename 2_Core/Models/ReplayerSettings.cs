using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace BeatLeader.Models
{
    public class ReplayerSettings
    {
        public bool ShowUI { get; set; }
        public bool LoadPlayerEnvironment { get; set; }
        public bool SyncUIRotationWithHead { get; set; }
        public bool ForceUseReplayerCamera { get; set; }

        public bool ShowHead { get; set; }
        public bool ShowLeftSaber { get; set; }
        public bool ShowRightSaber { get; set; }

        public int MaxFOV { get; set; }
        public int MinFOV { get; set; }
        public int CameraFOV { get; set; }
        public string FPFCCameraPose { get; set; }
        public string VRCameraPose { get; set; }

        public void CopyWith(bool? showUI = null, bool? loadPlayerEnvironment = null, bool? syncUIRotation = null,
            bool? useReplayerCamera = null, bool? showHead = null, bool? showLeftSaber = null, bool? showRightSaber = null,
            int? maxFov = null, int? minFov = null, int? cameraFov = null, string fpfcCameraPose = null, string vrCameraPose = null)
        {
            ShowUI = (bool)(showUI != null ? showUI : ShowUI);
            LoadPlayerEnvironment = (bool)(loadPlayerEnvironment != null ? loadPlayerEnvironment : LoadPlayerEnvironment);
            SyncUIRotationWithHead = (bool)(syncUIRotation != null ? syncUIRotation : SyncUIRotationWithHead);
            ForceUseReplayerCamera = (bool)(useReplayerCamera != null ? useReplayerCamera : ForceUseReplayerCamera);

            ShowHead = (bool)(showHead != null ? showHead : ShowHead);
            ShowLeftSaber = (bool)(showLeftSaber != null ? showLeftSaber : ShowLeftSaber);
            ShowRightSaber = (bool)(showRightSaber != null ? showRightSaber : ShowRightSaber);

            MaxFOV = (int)(maxFov != null ? maxFov : MaxFOV);
            MinFOV = (int)(minFov != null ? minFov : MinFOV);
            CameraFOV = (int)(cameraFov != null ? cameraFov : CameraFOV);
            FPFCCameraPose = string.IsNullOrEmpty(fpfcCameraPose) ? fpfcCameraPose : FPFCCameraPose;
            VRCameraPose = string.IsNullOrEmpty(vrCameraPose) ? vrCameraPose : VRCameraPose;
        }
    }
}
