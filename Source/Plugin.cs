using BeatLeader.DataManager;
using BeatLeader.UI.Reactive;
using BeatLeader.Utils;
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

        internal static Version Version { get; private set; }

        #endregion

        #region Init

        internal static IPALogger Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger, PluginMetadata metadata) {
            Log = logger;
            Version = metadata.HVersion;
            // important to call on init because p/invoke need some
            // time to start working after the manual LoadLibrary call
            DynamicLibLoader.Load();
            InitializeConfig();
            InitializeAssets();
        }

        private static void InitializeAssets() {
            BundleLoader.Initialize();
        }

        private static void InitializeConfig() {
            ConfigFileData.Initialize();
        }

        #endregion

        #region OnApplicationStart

        [OnStart]
        [UsedImplicitly]
        public void OnApplicationStart() {
            ObserveEnabled();
            SettingsPanelUI.AddTab();
            ReplayManager.LoadCache();
            InteropLoader.Init();
            ReactivePlatform.Init();
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
        public void OnApplicationQuit() {
            SerializableSingletons.SaveAll();
            LeaderboardsCache.Save();
            ReplayManager.SaveCache();
            ConfigFileData.Instance.LastSessionModVersion = Version.ToString();
            ConfigFileData.Save();
            //important to call LAST!!
            DynamicLibLoader.Unload();
        }

        #endregion
    }
}