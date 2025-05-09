using BeatLeader.DataManager;
using BeatLeader.UI.BSML_Addons;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Util;
using Hive.Versioning;
using IPA;
using IPA.Config;
using IPA.Loader;
using JetBrains.Annotations;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace BeatLeader {
    [Plugin(RuntimeOptions.SingleStartInit)]
    [UsedImplicitly]
    public class Plugin {
        #region Constants

        internal const string ResourcesPath = "BeatLeader._9_Resources";
        internal const string PluginId = "BeatLeader";
        internal const string HarmonyId = "BeatLeader";
        internal const string FancyName = "BeatLeader";

        internal static string UserAgent = "PC mod";

        internal static Version Version { get; private set; }

        #endregion

        #region Init

        internal static IPALogger Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger, PluginMetadata metadata) {
            Log = logger;
            Version = metadata.HVersion;
            
            ConfigFileData.Initialize();
            BundleLoader.Initialize();
        }

        #endregion

        #region OnApplicationStart

        [OnStart]
        [UsedImplicitly]
        public void OnApplicationStart() {
            OnEnabledChanged(PluginConfig.Enabled);
            MainMenuAwaiter.MainMenuInitializing += MainMenuInit;
            InteropLoader.Init();

            UserAgent = $"PC mod {Plugin.Version} / {Application.version}";
        }

        public static void MainMenuInit() {
            SettingsPanelUI.AddTab();
            BSMLAddonsLoader.LoadAddons();
            ReplayManager.LoadCache();
        }

        private static void OnEnabledChanged(bool enabled) {
            if (enabled) {
                HarmonyHelper.ApplyPatches();
                // ModPanelUI.AddTab(); -- Template with "HelloWorld!" button
            } else {
                HarmonyHelper.RemovePatches();
                // ModPanelUI.RemoveTab();
            }
        }

        #endregion

        #region OnApplicationQuit

        [OnExit]
        [UsedImplicitly]
        public void OnApplicationQuit() {
            LeaderboardsCache.Save();
            ReplayManager.SaveCache();
            ConfigFileData.Instance.LastSessionModVersion = Version.ToString();
            ConfigFileData.Save();
        }

        #endregion
    }
}