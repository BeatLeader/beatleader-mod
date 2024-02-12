using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
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

        #region Language

        [UIValue("language-choices"), UsedImplicitly]
        private List<object> _languageOptions = BLLocalization.SupportedLanguagesSorted().Cast<object>().ToList();

        [UIValue("language-choice"), UsedImplicitly]
        private object _languageValue = PluginConfig.SelectedLanguage;

        [UIAction("language-on-change"), UsedImplicitly]
        private void LanguageOnChange(object selectedValue) {
            PluginConfig.SelectedLanguage = (BLLanguage)selectedValue;
        }

        [UIAction("language-formatter"), UsedImplicitly]
        private string LanguageFormatter(object selectedValue) {
            return BLLocalization.GetLanguageName((BLLanguage)selectedValue);
        }

        #endregion
    }
}