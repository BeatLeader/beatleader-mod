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

        public ReplayerShortcuts Shortcuts { get; set; }
    }
}
