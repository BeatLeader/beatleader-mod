using System;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class GlobalSettingsView : ReactiveComponent {
        #region Setup

        public void Setup(MenuTransitionsHelper? transitionsHelper) {
            if (transitionsHelper != null) {
                _reloadNotice.Setup(transitionsHelper);
            }
        }

        #endregion

        #region Event

        public static event Action<bool> ExperienceBarConfigEvent;

        #endregion

        #region Notice

        private bool _initialMenuButtonEnabled;
        private bool _initialNoticeboardEnabled;
        private BLLanguage _initialLanguage;
        private BeatLeaderServer _initialServer;

        public void CancelSelection() {
            BeatLeaderMenuButtonManager.MenuButtonEnabled = _initialMenuButtonEnabled;
            PluginConfig.NoticeboardEnabled = _initialNoticeboardEnabled;
            PluginConfig.SelectedLanguage = _initialLanguage;
            PluginConfig.MainServer = _initialServer;

            _menuButtonToggle.SetActive(_initialMenuButtonEnabled);
            _noticeboardToggle.SetActive(_initialNoticeboardEnabled);
            _languageDropdown.Select(_initialLanguage);
            _serverDropdown.Select(_initialServer);
        }

        private void SaveInitialValues() {
            _initialMenuButtonEnabled = BeatLeaderMenuButtonManager.MenuButtonEnabled;
            _initialNoticeboardEnabled = PluginConfig.NoticeboardEnabled;
            _initialLanguage = PluginConfig.SelectedLanguage;
            _initialServer = PluginConfig.MainServer;
        }

        private void RefreshNotice() {
            if (_reloadNotice.CanBeEnabled) {
                _reloadNotice.Enabled = _initialLanguage != PluginConfig.SelectedLanguage;
            }
        }

        #endregion

        #region Construct

        private Toggle _menuButtonToggle = null!;
        private Toggle _noticeboardToggle = null;
        private TextDropdown<BLLanguage> _languageDropdown = null!;
        private TextDropdown<BeatLeaderServer> _serverDropdown = null!;
        private ReloadNotice _reloadNotice = null!;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new Toggle()
                        .With(x => x.SetActive(BeatLeaderMenuButtonManager.MenuButtonEnabled, false))
                        .WithListener(
                            x => x.Active,
                            x => BeatLeaderMenuButtonManager.MenuButtonEnabled = x
                        )
                        .Bind(ref _menuButtonToggle)
                        .InNamedRail("Menu Button Enabled"),
                    new Toggle()
                        .With(x => x.SetActive(PluginConfig.NoticeboardEnabled, false))
                        .WithListener(
                            x => x.Active,
                            x => PluginConfig.NoticeboardEnabled = x
                        )
                        .Bind(ref _noticeboardToggle)
                        .InNamedRail("Show Noticeboard"),
                    new TextDropdown<BLLanguage>()
                        .WithListener(
                            x => x.SelectedKey,
                            HandleLanguageChanged
                        )
                        .Bind(ref _languageDropdown)
                        .InNamedRail("Language"),
                    //
                    new TextDropdown<BeatLeaderServer>()
                        .WithListener(
                            x => x.SelectedKey,
                            HandleServerChanged
                        )
                        .Bind(ref _serverDropdown)
                        .InNamedRail("Server"),
                    //
                    new Toggle()
                        .With(x => x.SetActive(ConfigFileData.Instance.ExperienceBarEnabled, false))
                        .WithListener(
                            x => x.Active,
                            HandleEnableExperienceBar
                        )
                        .InNamedRail("Experience Bar"),
                    //
                    new ReloadNotice()
                        .AsFlexItem(margin: new() { top = 4f })
                        .Bind(ref _reloadNotice)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart,
                gap: 1f
            ).Use();
        }

        protected override void OnInitialize() {
            SaveInitialValues();
            RefreshNotice();
            //applying language
            var lang = PluginConfig.SelectedLanguage;
            foreach (var language in BLLocalization.SupportedLanguagesSorted()) {
                var name = BLLocalization.GetLanguageName(language);
                _languageDropdown.Items.Add(language, name);
            }
            _languageDropdown.Select(lang);
            //applying server
            var server = PluginConfig.MainServer;
            foreach (var option in BeatLeaderServerUtils.ServerOptions) {
                var name = option.GetName();
                _serverDropdown.Items.Add(option, name);
            }
            _serverDropdown.Select(server);
        }

        #endregion

        #region Callbacks

        private void HandleLanguageChanged(BLLanguage language) {
            PluginConfig.SelectedLanguage = language;
            RefreshNotice();
        }

        private void HandleServerChanged(BeatLeaderServer server) {
            PluginConfig.MainServer = server;
        }

        private void HandleEnableExperienceBar(bool enabled) {
            ConfigFileData.Instance.ExperienceBarEnabled = enabled;
            ExperienceBarConfigEvent?.Invoke(enabled);
        }

        #endregion
    }
}