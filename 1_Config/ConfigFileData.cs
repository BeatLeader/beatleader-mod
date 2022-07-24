using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BeatLeader.Models;
using IPA.Config.Stores;
using JetBrains.Annotations;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace BeatLeader
{
    [UsedImplicitly]
    internal class ConfigFileData {
        public static ConfigFileData Instance { get; set; }

        #region ConfigVersion
        
        public const string CurrentConfigVersion = "1.0";

        [UsedImplicitly]
        public virtual string ConfigVersion { get; set; } = CurrentConfigVersion;

        #endregion

        #region Enabled

        public bool Enabled = ConfigDefaults.Enabled;

        #endregion

        #region ScoresContext

        public ScoresContext ScoresContext = ConfigDefaults.ScoresContext;

        #endregion

        #region LeaderboardTableMask

        public ScoreRowCellType LeaderboardTableMask = ConfigDefaults.LeaderboardTableMask;

        #endregion
        
        #region OnReload

        [UsedImplicitly]
        public virtual void OnReload() {
            if (ConfigVersion != CurrentConfigVersion) ConfigVersion = CurrentConfigVersion;
        }

        #endregion
    }
}