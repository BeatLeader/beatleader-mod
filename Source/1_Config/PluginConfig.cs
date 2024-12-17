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

        #region Noticeboard

        public static event Action<bool> OnNoticeboardEnabledChangedEvent;

        public static bool NoticeboardEnabled {
            get => ConfigFileData.Instance.NoticeboardEnabled;
            set {
                if (ConfigFileData.Instance.NoticeboardEnabled == value) return;
                ConfigFileData.Instance.NoticeboardEnabled = value;
                OnNoticeboardEnabledChangedEvent?.Invoke(value);
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

        public static event Action<int> ScoresContextChangedEvent;

        public static int ScoresContext {
            get => ConfigFileData.Instance.ScoresContext;
            set {
                if (ConfigFileData.Instance.ScoresContext.Equals(value)) return;
                ConfigFileData.Instance.ScoresContext = value;
                ScoresContextChangedEvent?.Invoke(value);
            }
        }

        public static event Action ScoresContextListChangedEvent;

        public static void NotifyScoresContextListWasChanged() {
            ScoresContextListChangedEvent?.Invoke();
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

        #region Christmas

        public static event Action<ChristmasSettings>? ChristmasSettingsUpdatedEvent;

        public static ChristmasSettings ChristmasSettings => ConfigFileData.Instance.ChristmasSettings;

        public static void NotifyChristmasSettingsUpdated() {
            ChristmasSettingsUpdatedEvent?.Invoke(ChristmasSettings);
        }

        #endregion
    }
}