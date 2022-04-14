using BeatLeader.API;
using BeatLeader.Utils;
using BeatLeader.UI;
using BeatLeader.Installers;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using JetBrains.Annotations;
using IPALogger = IPA.Logging.Logger;

namespace BeatLeader {
    [Plugin(RuntimeOptions.SingleStartInit)]
    [UsedImplicitly]
    public class Plugin 
    {
        #region Constants

        internal const string ResourcesPath = "BeatLeader.Resources";
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
            SubscribeInstallers();
            EventsHandler.Init();
            SubscribeEvents();
        }

        private static void InitializeAssets() 
        {
            BundleLoader.Initialize();
        }

        private static void InitializeConfig(Config config) 
        {
            ConfigFileData.Instance = config.Generated<ConfigFileData>();
        }

        private static void SubscribeEvents()
        {
            //EventsHandler.LevelViewUpdated += ReplayMenuUI.CheckIsReplayExists;
        }

        private static void SubscribeInstallers()
        {
            Zenjector.afterAppInstalled += OnAppInitInstaller.Install;
            Zenjector.afterGameplayCoreInstalled += OnGameplayCoreInstaller.Install;
            Zenjector.afterMenuInstalled += OnMenuInstaller.Install;
        }

        #endregion

        #region OnApplicationStart

        [OnStart]
        [UsedImplicitly]
        public void OnApplicationStart() 
        {
            ObserveEnabled();
            SettingsPanelUIHelper.AddTab();
        }

        private static void ObserveEnabled() 
        {
            PluginConfig.OnEnabledChangedEvent += OnEnabledChanged;
            OnEnabledChanged(PluginConfig.Enabled);
        }

        private static void OnEnabledChanged(bool enabled) 
        {
            if (enabled) {
                HarmonyHelper.ApplyPatches();
                ModPanelUIHelper.AddTab();
                EventsHandler.MenuSceneLoadedFresh += ReplayMenuUI.PatchUI;
            } else {
                HarmonyHelper.RemovePatches();
                ModPanelUIHelper.RemoveTab();
            }
        }

        #endregion

        #region OnApplicationQuit

        [OnExit]
        [UsedImplicitly]
        public void OnApplicationQuit() 
        { 
            
        }

        #endregion
    }
}