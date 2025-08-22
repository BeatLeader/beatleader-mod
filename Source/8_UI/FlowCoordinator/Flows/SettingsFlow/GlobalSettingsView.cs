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

        public void Setup(MenuTransitionsHelper transitionsHelper) {
            _reloadNotice.Setup(transitionsHelper);
        }

        #endregion

        #region Event

        public static event Action<bool> ExperienceBarConfigEvent;

        #endregion

        #region Notice

        private BLLanguage _initialLanguage;

        private void SaveInitialValues() {
            _initialLanguage = PluginConfig.SelectedLanguage;
        }

        private void RefreshNotice() {
            _reloadNotice.Enabled = _initialLanguage != PluginConfig.SelectedLanguage;
        }

        #endregion

        #region Construct

        private TextDropdown<BLLanguage> _languageDropdown = null!;
        private TextDropdown<BeatLeaderServer> _serverDropdown = null!;
        private ReloadNotice _reloadNotice = null!;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
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