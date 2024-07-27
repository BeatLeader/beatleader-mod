using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    [PublicAPI]
    public class ReplayerSettings {
        public static ReplayerSettings DefaultSettings => ConfigDefaults.ReplayerSettings;
        public static ReplayerSettings UserSettings => ConfigFileData.Instance.ReplayerSettings;

        [Obsolete]
        public bool AutoHideUI {
            get => UISettings.AutoHideUI;
            set => UISettings.AutoHideUI = value;
        }
        public bool ExitReplayAutomatically { get; set; }
        public bool LoadPlayerEnvironment { get; set; }
        public bool LoadPlayerJumpDistance { get; set; }

        [Obsolete] public bool ShowHead { get; set; }
        [Obsolete] public bool ShowLeftSaber { get; set; }
        [Obsolete] public bool ShowRightSaber { get; set; }

        public bool ShowWatermark { get; set; }

        public bool ShowTimelineMisses { get; set; }
        public bool ShowTimelineBombs { get; set; }
        public bool ShowTimelinePauses { get; set; }

        public ReplayerShortcuts Shortcuts { get; set; } = new();
        public ReplayerUISettings UISettings { get; set; } = new();
        public BodySettings BodySettings { get; set; } = new();

        [Obsolete]
        public LayoutEditorSettings? LayoutEditorSettings => UISettings.LayoutEditorSettings;

        [JsonConverter(typeof(ImplicitTypeConverter<InternalReplayerCameraSettings>))]
        public ReplayerCameraSettings? CameraSettings { get; set; }
    }
}