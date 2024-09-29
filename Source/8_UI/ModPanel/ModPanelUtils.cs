using BeatSaberMarkupLanguage.Util;

namespace BeatLeader {
    internal partial class ModPanelUI {
        private const string ResourcePath = Plugin.ResourcesPath + ".BSML.Leaderboard.ModPanelUI.bsml";
        private const string TabName = Plugin.FancyName;

        private static bool _tabActive;

        public static void AddTab() {
            if (_tabActive) return;
            BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup.Instance.AddTab(
                TabName,
                ResourcePath,
                instance
            );
            _tabActive = true;
        }

        public static void RemoveTab() {
            if (!_tabActive) return;
            BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup.Instance.RemoveTab(TabName);
            _tabActive = false;
        }
    }
}