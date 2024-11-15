using BeatSaberMarkupLanguage.Util;

namespace BeatLeader {
    internal partial class SettingsPanelUI {
        private const string ResourcePath = Plugin.ResourcesPath + ".BSML.SettingsPanelUI.bsml";
        private const string TabName = Plugin.FancyName;

        public static void AddTab() {
            BeatSaberMarkupLanguage.Settings.BSMLSettings.Instance.AddSettingsMenu(
                TabName,
                ResourcePath,
                instance
            );
        }
    }
}