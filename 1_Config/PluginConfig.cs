using System;
using BeatLeader.Models;

namespace BeatLeader
{
    internal static class PluginConfig
    {
        #region Enabled

        public static event Action<bool> OnEnabledChangedEvent;

        public static bool Enabled
        {
            get => ConfigFileData.Instance.Enabled;
            set
            {
                if (ConfigFileData.Instance.Enabled == value) return;
                ConfigFileData.Instance.Enabled = value;
                OnEnabledChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region ScoresContext

        public static event Action<ScoresContext> ScoresContextChangedEvent;

        public static ScoresContext ScoresContext
        {
            get => ConfigFileData.Instance.ScoresContext;
            set
            {
                if (ConfigFileData.Instance.ScoresContext.Equals(value)) return;
                ConfigFileData.Instance.ScoresContext = value;
                ScoresContextChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region LeaderboardTableMask

        public static event Action<ScoreRowCellType> LeaderboardTableMaskChangedEvent;

        public static ScoreRowCellType LeaderboardTableMask
        {
            get => ConfigFileData.Instance.LeaderboardTableMask;
            set
            {
                if (ConfigFileData.Instance.LeaderboardTableMask.Equals(value)) return;
                ConfigFileData.Instance.LeaderboardTableMask = value;
                LeaderboardTableMaskChangedEvent?.Invoke(value);
            }
        }

        public static ScoreRowCellType GetLeaderboardTableMask(bool includePP)
        {
            return includePP ? LeaderboardTableMask : LeaderboardTableMask & ~ScoreRowCellType.PerformancePoints;
        }

        #endregion
    }
}