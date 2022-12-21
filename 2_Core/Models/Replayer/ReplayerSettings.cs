using BeatLeader.Utils;

namespace BeatLeader.Models
{
    public class ReplayerSettings
    {
        public bool AutoHideUI { get; set; }
        public bool LoadPlayerEnvironment { get; set; }
        public bool ExitReplayAutomatically { get; set; }

        public bool ShowHead { get; set; }
        public bool ShowLeftSaber { get; set; }
        public bool ShowRightSaber { get; set; }
        public bool ShowWatermark { get; set; }

        public int MaxFOV { get; set; }
        public int MinFOV { get; set; }
        public int CameraFOV { get; set; }
        public string? FPFCCameraPose { get; set; }
        public string? VRCameraPose { get; set; }

        public ReplayerShortcuts Shortcuts { get; set; } = new();

        public string? ActualCameraPose => InputUtils.IsInFPFC ? FPFCCameraPose : VRCameraPose;
    }
}
