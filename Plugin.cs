using IPA;
using IPA.Config;
using IPA.Config.Stores;
using JetBrains.Annotations;
using IPALogger = IPA.Logging.Logger;
using BeatLeader.UI.BSML_Addons;

namespace BeatLeader
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    [UsedImplicitly]
    public class Plugin
    {
        #region Constants

        internal const string ResourcesPath = "BeatLeader._9_Resources";
        internal const string HarmonyId = "BeatLeader";
        internal const string FancyName = "BeatLeader";

        #endregion

        #region Init

        internal static IPALogger Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Config config)
        {
            Log = logger;
            InitializeConfig(config);
            InitializeAssets();
        }

        private static void InitializeAssets()
        {
            BundleLoader.Initialize();
        }

        private static void InitializeConfig(Config config)
        {
            ConfigFileData.Instance = config.Generated<ConfigFileData>();
            ReplayerConfig.Load();
        }

        #endregion

        #region OnApplicationStart

        [OnStart]
        [UsedImplicitly]
        public void OnApplicationStart()
        {
            ObserveEnabled();
            SettingsPanelUI.AddTab();
            BSMLAddonsLoader.LoadAddons();
        }

        private static void ObserveEnabled()
        {
            PluginConfig.OnEnabledChangedEvent += OnEnabledChanged;
            OnEnabledChanged(PluginConfig.Enabled);
        }

        private static void OnEnabledChanged(bool enabled)
        {
            if (enabled)
            {
                HarmonyHelper.ApplyPatches();
                // ModPanelUI.AddTab(); -- Template with "HelloWorld!" button
            }
            else
            {
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
            ReplayerConfig.Save();
        }

        #endregion
    }
}