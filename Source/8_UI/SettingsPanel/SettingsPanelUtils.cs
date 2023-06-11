namespace BeatLeader {
    internal partial class SettingsPanelUI {
        private const string ResourcePath = Plugin.ResourcesPath + ".BSML.SettingsPanelUI.bsml";
        private const string TabName = Plugin.FancyName;

        private static bool _tabActive;

        public static void AddTab() {
            if (_tabActive) return;
            PersistentSingleton<BeatSaberMarkupLanguage.Settings.BSMLSettings>.instance.AddSettingsMenu(
                TabName,
                ResourcePath,
                instance
            );
            _tabActive = true;
        }

        public static void RemoveTab() {
            if (!_tabActive) return;
            PersistentSingleton<BeatSaberMarkupLanguage.Settings.BSMLSettings>.instance.RemoveSettingsMenu(TabName);
            _tabActive = false;
        }
    }
}