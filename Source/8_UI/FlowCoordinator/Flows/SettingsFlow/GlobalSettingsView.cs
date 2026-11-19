using System;
using System.Linq;
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

        public static event Action<bool>? ExperienceBarConfigEvent;

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

            _menuButtonToggle.Toggled = _initialMenuButtonEnabled;
            _noticeboardToggle.Toggled = _initialNoticeboardEnabled;
            _languageDropdown.Key = _initialLanguage;
            _serverDropdown.Key = _initialServer;
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

        private BsToggle _menuButtonToggle = null!;
        private BsToggle _noticeboardToggle = null!;
        private BsDropdown<BLLanguage> _languageDropdown = null!;
        private BsDropdown<BeatLeaderServer> _serverDropdown = null!;
        private ReloadNotice _reloadNotice = null!;

        protected override GameObject Construct() {
            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    JustifyContent = Justify.FlexStart,
                    Gap = 1f
                },

                Children = {
                    new BsToggle {
                            Toggled = BeatLeaderMenuButtonManager.MenuButtonEnabled,
                            OnToggleStateChanged = x => BeatLeaderMenuButtonManager.MenuButtonEnabled = x
                        }
                        .Bind(ref _menuButtonToggle)
                        .InNamedRail("Menu Button Enabled"),
                    new BsToggle {
                            Toggled = PluginConfig.NoticeboardEnabled,
                            OnToggleStateChanged = x => PluginConfig.NoticeboardEnabled = x
                        }
                        .Bind(ref _noticeboardToggle)
                        .InNamedRail("Show Noticeboard"),
                    new BsDropdown<BLLanguage> {
                            Items = BLLocalization.SupportedLanguagesSorted().ToDictionary(
                                static x => x,
                                static x => new BsDropdownItem(BLLocalization.GetLanguageName(x), null)
                            ),
                            Key = PluginConfig.SelectedLanguage,
                            OnKeyChanged = HandleLanguageChanged
                        }
                        .Bind(ref _languageDropdown)
                        .InNamedRail("Language"),
                    //
                    new BsDropdown<BeatLeaderServer> {
                            Items = BeatLeaderServerUtils.ServerOptions.ToDictionary(
                                static x => x,
                                static x => new BsDropdownItem(x.GetName(), null)
                            ),
                            Key = PluginConfig.MainServer,
                            OnKeyChanged = HandleServerChanged
                        }
                        .Bind(ref _serverDropdown)
                        .InNamedRail("Server"),
                    //
                    new BsToggle {
                        Toggled = ConfigFileData.Instance.ExperienceBarEnabled,
                        OnToggleStateChanged = HandleEnableExperienceBar
                    }.InNamedRail("Experience Bar"),
                    //
                    new ReloadNotice {
                        FlexItem = {
                            Margin = new() { top = 4f }
                        }
                    }.Bind(ref _reloadNotice)
                }
            }.Use();
        }

        protected override void OnInitialize() {
            SaveInitialValues();
            RefreshNotice();
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