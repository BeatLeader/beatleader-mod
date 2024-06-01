using System;
using BeatLeader.Models;

namespace BeatLeader {
    internal static class PluginConfig {
        #region Enabled

        public static event Action<bool> OnEnabledChangedEvent;

        public static bool Enabled {
            get => ConfigFileData.Instance.Enabled;
            set {
                if (ConfigFileData.Instance.Enabled == value) return;
                ConfigFileData.Instance.Enabled = value;
                OnEnabledChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region MainServer

        public static event Action<BeatLeaderServer> MainServerChangedEvent;

        public static BeatLeaderServer MainServer {
            get => ConfigFileData.Instance.MainServer;
            set {
                if (ConfigFileData.Instance.MainServer.Equals(value)) return;
                ConfigFileData.Instance.MainServer = value;
                MainServerChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region ScoresContext

        public static event Action<ScoresContext> ScoresContextChangedEvent;

        public static ScoresContext ScoresContext {
            get => ConfigFileData.Instance.ScoresContext;
            set {
                if (ConfigFileData.Instance.ScoresContext.Equals(value)) return;
                ConfigFileData.Instance.ScoresContext = value;
                ScoresContextChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region LeaderboardTableMask

        public static event Action<ScoreRowCellType> LeaderboardTableMaskChangedEvent;

        public static ScoreRowCellType LeaderboardTableMask {
            get => ConfigFileData.Instance.LeaderboardTableMask;
            set {
                if (ConfigFileData.Instance.LeaderboardTableMask.Equals(value)) return;
                ConfigFileData.Instance.LeaderboardTableMask = value;
                LeaderboardTableMaskChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region LeaderboardDisplaySettings

        public static event Action<LeaderboardDisplaySettings> LeaderboardDisplaySettingsChangedEvent;

        public static LeaderboardDisplaySettings LeaderboardDisplaySettings
        {
            get => ConfigFileData.Instance.LeaderboardDisplaySettings;
            set
            {
                ConfigFileData.Instance.LeaderboardDisplaySettings = value;
                LeaderboardDisplaySettingsChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region ReplayerSettings

        public static event Action<ReplayerSettings> ReplayerSettingsChangedEvent;

        public static ReplayerSettings ReplayerSettings {
            get => ConfigFileData.Instance.ReplayerSettings;
            set {
                ConfigFileData.Instance.ReplayerSettings = value;
                ReplayerSettingsChangedEvent?.Invoke(value);
            }
        }

        public static void NotifyReplayerSettingsChanged() {
            ReplayerSettingsChangedEvent?.Invoke(ReplayerSettings);
        }

        #endregion

        #region Language

        public static BLLanguage SelectedLanguage {
            get => ConfigFileData.Instance.SelectedLanguage;
            set => ConfigFileData.Instance.SelectedLanguage = value;
        }

        #endregion

        #region EnableReplayCaching

        public static bool EnableReplayCaching {
            get => ConfigFileData.Instance.EnableReplayCaching;
            set => ConfigFileData.Instance.EnableReplayCaching = value;
        }

        #endregion
    }
}