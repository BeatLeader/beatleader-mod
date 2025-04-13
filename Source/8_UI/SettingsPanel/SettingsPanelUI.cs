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

        BLLanguage languageToSelect = PluginConfig.SelectedLanguage;

        [UIAction("language-on-change"), UsedImplicitly]
        private void LanguageOnChange(BLLanguage selectedValue) {
            languageToSelect = selectedValue;
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

        BeatLeaderServer serverToSelect = PluginConfig.MainServer;

        [UIAction("server-on-change"), UsedImplicitly]
        private void ServerOnChange(BeatLeaderServer selectedValue) {
            serverToSelect = selectedValue;
        }

        [UIAction("server-formatter"), UsedImplicitly]
        private string ServerFormatter(BeatLeaderServer selectedValue) {
            return selectedValue.GetName();
        }

        #endregion

        [UIAction("#cancel")]
        protected void HandleCancel() {
            _languageValue = PluginConfig.SelectedLanguage;
            NotifyPropertyChanged(nameof(_languageValue));

            _serverValue = PluginConfig.MainServer;
            NotifyPropertyChanged(nameof(_serverValue));
        }

        [UIAction("#apply")]
        protected void HandleApply() {
            PluginConfig.SelectedLanguage = languageToSelect;
            PluginConfig.MainServer = serverToSelect;
        }
    }
}