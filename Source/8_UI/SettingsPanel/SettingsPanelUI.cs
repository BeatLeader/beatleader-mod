using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
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

        #region Language

        [UIValue("override-language"), UsedImplicitly]
        private bool OverrideLanguage {
            get => PluginConfig.OverrideLanguage;
            set {
                PluginConfig.OverrideLanguage = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("language-choices"), UsedImplicitly]
        private List<BLLanguage> _languageOptions = BLLocalization.SupportedLanguagesSorted();

        [UIValue("language-choice"), UsedImplicitly]
        private BLLanguage _languageValue = PluginConfig.SelectedLanguage;

        [UIAction("language-on-change"), UsedImplicitly]
        private void LanguageOnChange(BLLanguage selectedValue) {
            PluginConfig.SelectedLanguage = selectedValue;
        }

        [UIAction("language-formatter"), UsedImplicitly]
        private string LanguageFormatter(BLLanguage selectedValue) {
            return BLLocalization.GetLanguageName(selectedValue);
        }

        #endregion
    }
}