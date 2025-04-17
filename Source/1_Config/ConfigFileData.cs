using System;
using System.IO;
using System.Runtime.CompilerServices;
using BeatLeader.Models;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Version = Hive.Versioning.Version;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace BeatLeader {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal class ConfigFileData {
        #region Serialization

        private const string ConfigPath = "UserData\\BeatLeader.json";

        public static void Initialize() {
            if (File.Exists(ConfigPath)) {
                var text = File.ReadAllText(ConfigPath);
                try {
                    Instance = JsonConvert.DeserializeObject<ConfigFileData>(text);
                    Plugin.Log.Debug("BeatLeader config initialized");
                    return;
                } catch (Exception ex) {
                    Plugin.Log.Error($"Failed to load config (default will be used):\n{ex}");
                }
            }
            Instance = new();
        }

        public static void Save() {
            try {
                var text = JsonConvert.SerializeObject(
                    Instance, Formatting.Indented, new JsonSerializerSettings {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Converters = {
                            new StringEnumConverter()
                        }
                    }
                );
                File.WriteAllText(ConfigPath, text);
                Plugin.Log.Debug("BeatLeader config saved");
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to save configuration:\n{ex}");
            }
        }

        public static ConfigFileData Instance { get; private set; } = null!;

        #endregion

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
        public bool NoticeboardEnabled = true;

        #endregion
        
        #region MenuButtonEnabled

        public bool MenuButtonEnabled = ConfigDefaults.MenuButtonEnabled;

        #endregion

        #region BeatLeaderServer
        
        public BeatLeaderServer MainServer = ConfigDefaults.MainServer;

        #endregion

        #region ScoresContext
        
        public int ScoresContext = ConfigDefaults.ScoresContext;

        #endregion

        #region HubTheme

        public BeatLeaderHubTheme HubTheme = ConfigDefaults.HubTheme;

        #endregion

        #region LeaderboardTableMask
        
        public ScoreRowCellType LeaderboardTableMask = ConfigDefaults.LeaderboardTableMask;

        #endregion

        #region LeaderboardDisplaySettings

        public LeaderboardDisplaySettings LeaderboardDisplaySettings = ConfigDefaults.LeaderboardDisplaySettings;

        #endregion

        #region ReplayerSettings

        public ReplayerSettings ReplayerSettings { get; set; } = ConfigDefaults.ReplayerSettings;

        #endregion

        #region ReplaySavingSettings

        public bool OverrideOldReplays = ConfigDefaults.OverrideOldReplays;

        public bool SaveLocalReplays = ConfigDefaults.SaveLocalReplays;
        
        public ReplaySaveOption ReplaySavingOptions = ConfigDefaults.ReplaySavingOptions;

        #endregion

        #region Language
        
        public BLLanguage SelectedLanguage = ConfigDefaults.SelectedLanguage;

        #endregion

        #region OnReload

        [UsedImplicitly]
        public virtual void OnReload() {
            if (ConfigVersion != CurrentConfigVersion) ConfigVersion = CurrentConfigVersion;
        }

        #endregion
    }
}