using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class GlobalSettingsView : ReactiveComponent {
        #region Setup

        public void Setup(MenuTransitionsHelper transitionsHelper) {
            _reloadNotice.Setup(transitionsHelper);
        }

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
        private ReloadNotice _reloadNotice = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new TextDropdown<BLLanguage>()
                        .WithListener(
                            x => x.SelectedKey,
                            HandleLanguageChanged
                        )
                        .Bind(ref _languageDropdown)
                        .InNamedRail("Language"),
                    //
                    new ReloadNotice()
                        .AsFlexItem(margin: new() { top = 4f })
                        .Bind(ref _reloadNotice)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart
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
        }

        #endregion

        #region Callbacks

        private void HandleLanguageChanged(BLLanguage language) {
            PluginConfig.SelectedLanguage = language;
            RefreshNotice();
        }

        #endregion
    }
}