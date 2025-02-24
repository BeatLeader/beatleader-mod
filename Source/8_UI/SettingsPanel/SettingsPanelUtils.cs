namespace BeatLeader {
    internal partial class SettingsPanelUI {
        private const string ResourcePath = Plugin.ResourcesPath + ".BSML.SettingsPanelUI.bsml";
        private const string TabName = Plugin.FancyName;

        public static void AddTab() {
            BeatSaberMarkupLanguage.Settings.BSMLSettings.instance.AddSettingsMenu(
                TabName,
                ResourcePath,
                instance
            );
        }
    }
}