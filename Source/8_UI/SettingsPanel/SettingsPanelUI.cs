using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
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

        
        [UIValue("noticeboard-enabled"), UsedImplicitly]
        private bool NoticeboardEnabled {
            get => PluginConfig.NoticeboardEnabled;
            set => PluginConfig.NoticeboardEnabled = value;
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

        #region Server

        [UIValue("server-choices"), UsedImplicitly]
        private List<object> _serverOptions = BeatLeaderServerUtils.ServerOptions.Cast<object>().ToList();

        [UIValue("server-choice"), UsedImplicitly]
        private object _serverValue = PluginConfig.MainServer;

        [UIAction("server-on-change"), UsedImplicitly]
        private void ServerOnChange(object selectedValue) {
            PluginConfig.MainServer = (BeatLeaderServer)selectedValue;
        }

        [UIAction("server-formatter"), UsedImplicitly]
        private string ServerFormatter(object selectedValue) {
            return ((BeatLeaderServer)selectedValue).GetName();
        }

        #endregion
    }
}