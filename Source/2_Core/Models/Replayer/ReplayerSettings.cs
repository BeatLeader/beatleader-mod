using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
        
        // This property exists as a fix for previous versions that were broken because of the camera settings being null
        public ReplayerCameraSettings CameraSettings {
            get => _originalCameraSettings ??= ConfigDefaults.ReplayerSettings.CameraSettings;
            set => _originalCameraSettings = value;
        }

        // TODO: remove in the next release
        // ReSharper disable once ReplaceWithFieldKeyword
        [JsonProperty("CameraSettings"), UsedImplicitly]
        private ReplayerCameraSettings? _originalCameraSettings = new();
    }
}