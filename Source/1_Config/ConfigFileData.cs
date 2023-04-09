using System.Runtime.CompilerServices;
using BeatLeader.Components;
using BeatLeader.Models;
using Hive.Versioning;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using JetBrains.Annotations;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace BeatLeader {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal class ConfigFileData {
        public static ConfigFileData Instance { get; set; } = null!;

        #region ConfigVersion

        public const string CurrentConfigVersion = "1.0";

        [UsedImplicitly]
        public virtual string ConfigVersion { get; set; } = CurrentConfigVersion;

        #endregion

        #region ModVersion

        [UsedImplicitly]
        public string LastSessionModVersion { get; set; } = Version.Zero.ToString();

        #endregion
        
        #region Enabled

        public bool Enabled = ConfigDefaults.Enabled;

        #endregion

        #region ScoresContext

        [UseConverter]
        public ScoresContext ScoresContext = ConfigDefaults.ScoresContext;

        #endregion

        #region LeaderboardTableMask

        [UseConverter]
        public ScoreRowCellType LeaderboardTableMask = ConfigDefaults.LeaderboardTableMask;

        #endregion

        #region LeaderboardDisplaySettings

        public LeaderboardDisplaySettings LeaderboardDisplaySettings = ConfigDefaults.LeaderboardDisplaySettings;

        #endregion

        #region ReplayerSettings

        public InternalReplayerCameraSettings InternalReplayerCameraSettings { get; set; } = ConfigDefaults.InternalReplayerCameraSettings;

        public ReplayerSettings ReplayerSettings {
            get {
                _replayerSettings.CameraSettings = InternalReplayerCameraSettings;
                return _replayerSettings;
            }
            set => _replayerSettings = value;
        }
        
        private ReplayerSettings _replayerSettings = ConfigDefaults.ReplayerSettings;

        #endregion

        #region EnableReplayCaching

        public bool EnableReplayCaching = ConfigDefaults.EnableReplayCaching;

        #endregion

        #region OnReload

        [UsedImplicitly]
        public virtual void OnReload() {
            if (ConfigVersion != CurrentConfigVersion) ConfigVersion = CurrentConfigVersion;
        }

        #endregion
    }
}
