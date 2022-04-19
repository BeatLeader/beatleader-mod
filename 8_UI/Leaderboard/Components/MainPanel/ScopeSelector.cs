using System;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.MainPanel.ScopeSelector.bsml")]
    internal class ScopeSelector : ReeUIComponent {
        #region Start

        private void Start() {
            SelectScope(BLContext.DefaultScoresScope);
            SetMaterials();
        }

        #endregion

        #region SetScope

        private ScoresScope _currentScope;

        private void SelectScope(ScoresScope newScope) {
            _currentScope = newScope;

            switch (newScope) {
                case ScoresScope.Global:
                    SetColor(_globalComponent, true);
                    SetColor(_friendsComponent, false);
                    SetColor(_countryComponent, false);
                    LeaderboardEvents.NotifyScopeWasSelected(ScoresScope.Global);
                    break;
                case ScoresScope.Friends:
                    SetColor(_globalComponent, false);
                    SetColor(_friendsComponent, true);
                    SetColor(_countryComponent, false);
                    LeaderboardEvents.NotifyScopeWasSelected(ScoresScope.Friends);
                    break;
                case ScoresScope.Country:
                    SetColor(_globalComponent, false);
                    SetColor(_friendsComponent, false);
                    SetColor(_countryComponent, true);
                    LeaderboardEvents.NotifyScopeWasSelected(ScoresScope.Country);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(newScope), newScope, null);
            }
        }

        #endregion

        #region Colors

        private static readonly Color SelectedColor = new Color(0.0f, 0.4f, 1.0f, 1.0f);
        private static readonly Color FadedColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        private static readonly Color FadedHoverColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);

        [UIComponent("global-component"), UsedImplicitly]
        private ClickableImage _globalComponent;

        [UIComponent("friends-component"), UsedImplicitly]
        private ClickableImage _friendsComponent;

        [UIComponent("country-component"), UsedImplicitly]
        private ClickableImage _countryComponent;

        private void SetMaterials() {
            _globalComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            _friendsComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            _countryComponent.material = BundleLoader.UIAdditiveGlowMaterial;
        }

        private static void SetColor(ClickableImage image, bool selected) {
            image.DefaultColor = selected ? SelectedColor : FadedColor;
            image.HighlightColor = selected ? SelectedColor : FadedHoverColor;
        }

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