using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Util;
using JetBrains.Annotations;

namespace BeatLeader {
    internal partial class SettingsPanelUI : NotifiableSingleton<SettingsPanelUI> {
        #region Toggles

        [UIValue("enabled"), UsedImplicitly]
        private bool Enabled {
            get => PluginConfig.Enabled;
            set => PluginConfig.Enabled = value;
        }

        [UIValue("menu-button-enabled"), UsedImplicitly]
        private bool MenuButtonEnabled {
            get => BeatLeaderMenuButtonManager.MenuButtonEnabled;
            set => BeatLeaderMenuButtonManager.MenuButtonEnabled = value;
        }

        #endregion
    }
}