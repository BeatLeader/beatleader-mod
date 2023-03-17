using BeatLeader.Utils;
using IPA.Config.Data;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    public class ReplayerSettings {
        public static ReplayerSettings UserSettings => ConfigDefaults.ReplayerSettings;

        public bool AutoHideUI { get; set; }
        public bool ExitReplayAutomatically { get; set; }
        public bool LoadPlayerEnvironment { get; set; }
        public bool LoadPlayerJumpDistance { get; set; }

        public bool ShowHead { get; set; }
        public bool ShowLeftSaber { get; set; }
        public bool ShowRightSaber { get; set; }
        public bool ShowWatermark { get; set; }

        public bool ShowTimelineMisses { get; set; }
        public bool ShowTimelineBombs { get; set; }
        public bool ShowTimelinePauses { get; set; }

        public int MaxCameraFOV { get; set; }
        public int MinCameraFOV { get; set; }
        public int CameraFOV { get; set; }
        public string? FPFCCameraView { get; set; }
        public string? VRCameraView { get; set; }

        public ReplayerShortcuts Shortcuts { get; set; } = new();

        public string? GetActualCameraView() { 
            return InputUtils.IsInFPFC ? FPFCCameraView : VRCameraView;
        }

        public void SetActualCameraView(string? view) {
            if (InputUtils.IsInFPFC) {
                FPFCCameraView = view;
            } else {
                VRCameraView = view;
            }
        }
    }
}
