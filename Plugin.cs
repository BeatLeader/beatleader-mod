using BeatLeader.DataManager;
using BeatLeader.Interop;
using BeatLeader.Interop.BetterSongList;
using BeatLeader.UI.BSML_Addons;
using Hive.Versioning;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
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

        internal static Version Version => PluginManager.GetPlugin(PluginId).HVersion;

        #endregion

        #region Init

        internal static IPALogger Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Config config) {
            Log = logger;
            InitializeConfig(config);
            InitializeAssets();
        }

        private static void InitializeAssets() {
            BundleLoader.Initialize();
        }

        private static void InitializeConfig(Config config) {
            ConfigFileData.Instance = config.Generated<ConfigFileData>();
            AutomaticConfigTool.Load();
        }

        #endregion

        #region OnApplicationStart

        [OnStart]
        [UsedImplicitly]
        public void OnApplicationStart() {
            ObserveEnabled();
            SettingsPanelUI.AddTab();
            BSMLAddonsLoader.LoadAddons();
            BetterSongListInterop.TryRegister();
            Cam2Interop.Init();
            NoodleExtensionsInterop.Init();
            RaycastBlocker.BlockerMask = 1 << 5;
        }

        private static void ObserveEnabled() {
            PluginConfig.OnEnabledChangedEvent += OnEnabledChanged;
            OnEnabledChanged(PluginConfig.Enabled);
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
        public void OnApplicationQuit()
        {
            AutomaticConfigTool.Save();
            LeaderboardsCache.Save();
        }

        #endregion
    }
}