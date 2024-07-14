using IPA.Config.Stores.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public class ReplayerSettings {
        public static ReplayerSettings DefaultSettings => ConfigDefaults.ReplayerSettings;
        public static ReplayerSettings UserSettings => ConfigFileData.Instance.ReplayerSettings;

        public bool AutoHideUI { get; set; }
        public bool ExitReplayAutomatically { get; set; }
        public bool LoadPlayerEnvironment { get; set; }
        public bool LoadPlayerJumpDistance { get; set; }
        public bool IgnoreModifiers { get; set; }

        public bool ShowHead { get; set; }
        public bool ShowLeftSaber { get; set; }
        public bool ShowRightSaber { get; set; }
        public bool ShowWatermark { get; set; }

        public bool ShowTimelineMisses { get; set; }
        public bool ShowTimelineBombs { get; set; }
        public bool ShowTimelinePauses { get; set; }
        
        [Ignore]
        public ReplayerCameraSettings? CameraSettings { get; set; }
        public ReplayerShortcuts? Shortcuts { get; set; }
    }
}
