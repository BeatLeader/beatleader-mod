using System.Runtime.CompilerServices;
using BeatLeader.Components;
using BeatLeader.Models;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using JetBrains.Annotations;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace BeatLeader
{
    [UsedImplicitly]
    internal class ConfigFileData {
        public static ConfigFileData Instance { get; set; } = null!;

        #region ConfigVersion
        
        public const string CurrentConfigVersion = "1.0";

        [UsedImplicitly]
        public virtual string ConfigVersion { get; set; } = CurrentConfigVersion;

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

        public ReplayerSettings ReplayerSettings = ConfigDefaults.ReplayerSettings;

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