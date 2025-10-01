using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    [PublicAPI]
    public class ReplayerSettings {
        public static ReplayerSettings DefaultSettings => ConfigDefaults.ReplayerSettings;
        public static ReplayerSettings UserSettings => ConfigFileData.Instance.ReplayerSettings;

        public bool ExitReplayAutomatically { get; set; }
        public bool LoadPlayerEnvironment { get; set; }
        public bool LoadPlayerJumpDistance { get; set; }
        public bool IgnoreModifiers { get; set; }

        public bool ShowWatermark { get; set; }

        public ReplayerShortcuts Shortcuts { get; set; } = new();
        public ReplayerUISettings UISettings { get; set; } = new();
        public BodySettings BodySettings { get; set; } = new();
        
        [JsonConverter(typeof(ImplicitTypeConverter<InternalReplayerCameraSettings>))]
        public ReplayerCameraSettings? CameraSettings { get; set; }
    }
}