namespace BeatLeader {
    public static class ModPanelUIHelper {
        private const string ResourcePath = Plugin.ResourcesPath + ".BSML.ModPanelUI.bsml";
        private const string TabName = Plugin.FancyName;

        private static bool _tabActive;

        public static void AddTab() {
            if (_tabActive) return;
            PersistentSingleton<BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup>.instance.AddTab(
                TabName,
                ResourcePath,
                PersistentSingleton<ModPanelUI>.instance
            );
            _tabActive = true;
        }

        public static void RemoveTab() {
            if (!_tabActive) return;
            PersistentSingleton<BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup>.instance.RemoveTab(TabName);
            _tabActive = false;
        }
    }
}