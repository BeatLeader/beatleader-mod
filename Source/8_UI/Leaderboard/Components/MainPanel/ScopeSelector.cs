using System;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ScopeSelector : ReeUIComponentV2 {
        #region Init / Dispose

        protected override void OnInitialize() {
            SetMaterials();
            LeaderboardState.ScoresScopeChangedEvent += OnScoresScopeChanged;
            OnScoresScopeChanged(LeaderboardState.ScoresScope);
        }

        protected override void OnDispose() {
            LeaderboardState.ScoresScopeChangedEvent -= OnScoresScopeChanged;
        }

        #endregion

        #region OnScoresScopeChanged

        private void OnScoresScopeChanged(ScoresScope scoresScope) {
            switch (scoresScope) {
                case ScoresScope.Global:
                    SetColor(_globalComponent, true);
                    SetColor(_friendsComponent, false);
                    SetColor(_countryComponent, false);
                    break;
                case ScoresScope.Friends:
                    SetColor(_globalComponent, false);
                    SetColor(_friendsComponent, true);
                    SetColor(_countryComponent, false);
                    break;
                case ScoresScope.Country:
                    SetColor(_globalComponent, false);
                    SetColor(_friendsComponent, false);
                    SetColor(_countryComponent, true);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(scoresScope), scoresScope, null);
            }
        }

        #endregion

        #region Colors

        private static readonly Color SelectedColor = new(0.0f, 0.4f, 1.0f, 1.0f);
        private static readonly Color FadedColor = new(0.8f, 0.8f, 0.8f, 0.2f);
        private static readonly Color FadedHoverColor = new(0.5f, 0.5f, 0.5f, 0.2f);

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
            LeaderboardState.ScoresScope = ScoresScope.Global;
        }

        [UIAction("friends-on-click"), UsedImplicitly]
        private void NavFriendsOnClick() {
            LeaderboardState.ScoresScope = ScoresScope.Friends;
        }

        [UIAction("country-on-click"), UsedImplicitly]
        private void NavCountryOnClick() {
            LeaderboardState.ScoresScope = ScoresScope.Country;
        }

        #endregion
    }
}