using System;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScopeSelector.bsml")]
    internal class ScopeSelector : ReeUIComponent {
        #region Initialize/Dispose

        protected override void OnInitialize() { }

        protected override void OnDispose() { }

        #endregion

        #region SetScope

        private ScoresScope _currentScope;

        private void Start() {
            SelectScope(ScoresScope.Global);
        }

        private void SelectScope(ScoresScope newScope) {
            _currentScope = newScope;

            switch (newScope) {
                case ScoresScope.Global:
                    _globalText.faceColor = SelectedColor;
                    _friendsText.faceColor = FadedColor;
                    _countryText.faceColor = FadedColor;
                    LeaderboardEvents.NotifyGlobalButtonWasPressed();
                    break;
                case ScoresScope.Friends:
                    _globalText.faceColor = FadedColor;
                    _friendsText.faceColor = SelectedColor;
                    _countryText.faceColor = FadedColor;
                    LeaderboardEvents.NotifyFriendsButtonWasPressed();
                    break;
                case ScoresScope.Country:
                    _globalText.faceColor = FadedColor;
                    _friendsText.faceColor = FadedColor;
                    _countryText.faceColor = SelectedColor;
                    LeaderboardEvents.NotifyCountryButtonWasPressed();
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(newScope), newScope, null);
            }
        }

        #endregion

        #region Colors

        private static readonly Color SelectedColor = Color.white;
        private static readonly Color FadedColor = Color.gray;

        [UIComponent("global-component"), UsedImplicitly]
        private TextMeshProUGUI _globalText;

        [UIComponent("friends-component"), UsedImplicitly]
        private TextMeshProUGUI _friendsText;

        [UIComponent("country-component"), UsedImplicitly]
        private TextMeshProUGUI _countryText;

        #endregion

        #region Callbacks

        [UIAction("global-on-click"), UsedImplicitly]
        private void NavGlobalOnClick() {
            if (_currentScope.Equals(ScoresScope.Global)) return;
            SelectScope(ScoresScope.Global);
        }

        [UIAction("friends-on-click"), UsedImplicitly]
        private void NavFriendsOnClick() {
            if (_currentScope.Equals(ScoresScope.Friends)) return;
            SelectScope(ScoresScope.Friends);
        }

        [UIAction("country-on-click"), UsedImplicitly]
        private void NavCountryOnClick() {
            if (_currentScope.Equals(ScoresScope.Country)) return;
            SelectScope(ScoresScope.Country);
        }

        #endregion
    }
}