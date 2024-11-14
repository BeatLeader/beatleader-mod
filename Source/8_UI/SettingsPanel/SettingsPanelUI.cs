using System.Collections.Generic;
using BeatLeader.Models;
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

        
        [UIValue("noticeboard-enabled"), UsedImplicitly]
        private bool NoticeboardEnabled {
            get => PluginConfig.NoticeboardEnabled;
            set => PluginConfig.NoticeboardEnabled = value;
        }

        #endregion

        #region Language

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

        #region Server

        [UIValue("server-choices"), UsedImplicitly]
        private List<BeatLeaderServer> _serverOptions = BeatLeaderServerUtils.ServerOptions;

        [UIValue("server-choice"), UsedImplicitly]
        private BeatLeaderServer _serverValue = PluginConfig.MainServer;

        [UIAction("server-on-change"), UsedImplicitly]
        private void ServerOnChange(BeatLeaderServer selectedValue) {
            PluginConfig.MainServer = selectedValue;
        }

        [UIAction("server-formatter"), UsedImplicitly]
        private string ServerFormatter(BeatLeaderServer selectedValue) {
            return selectedValue.GetName();
        }

        #endregion
    }
}