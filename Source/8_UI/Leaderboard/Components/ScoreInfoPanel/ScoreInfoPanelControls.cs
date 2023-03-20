using System;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ScoreInfoPanelControls : ReeUIComponentV2 {
        #region Events

        protected override void OnInitialize() {
            SetMaterials();
            LeaderboardState.ScoreInfoPanelTabChangedEvent += OnScoreInfoPanelTabChanged;
            OnScoreInfoPanelTabChanged(LeaderboardState.ScoreInfoPanelTab);
        }

        protected override void OnDispose() {
            LeaderboardState.ScoreInfoPanelTabChangedEvent -= OnScoreInfoPanelTabChanged;
        }

        #endregion

        #region Reset

        public void Reset() {
            LeaderboardState.ScoreInfoPanelTab = ScoreInfoPanelTab.OverviewPage1;
        }

        #endregion

        #region OnScoreInfoPanelTabChanged

        private void OnScoreInfoPanelTabChanged(ScoreInfoPanelTab tab) {
            SetColor(_overviewPage1Component, tab is ScoreInfoPanelTab.OverviewPage1);
            SetColor(_overviewPage2Component, tab is ScoreInfoPanelTab.OverviewPage2);
            SetColor(_accuracyComponent, tab is ScoreInfoPanelTab.Accuracy);
            SetColor(_gridComponent, tab is ScoreInfoPanelTab.Grid);
            SetColor(_graphComponent, tab is ScoreInfoPanelTab.Graph);
            SetColor(_replayComponent, tab is ScoreInfoPanelTab.Replay);
        }

        #endregion

        #region Colors

        private static readonly Color SelectedColor = new(0.0f, 0.4f, 1.0f, 1.0f);
        private static readonly Color FadedColor = new(0.8f, 0.8f, 0.8f, 0.2f);
        private static readonly Color FadedHoverColor = new(0.5f, 0.5f, 0.5f, 0.2f);

        [UIComponent("overview-page1-component"), UsedImplicitly]
        private ClickableImage _overviewPage1Component;

        [UIComponent("overview-page2-component"), UsedImplicitly]
        private ClickableImage _overviewPage2Component;

        [UIComponent("accuracy-component"), UsedImplicitly]
        private ClickableImage _accuracyComponent;

        [UIComponent("grid-component"), UsedImplicitly]
        private ClickableImage _gridComponent;

        [UIComponent("graph-component"), UsedImplicitly]
        private ClickableImage _graphComponent;

        [UIComponent("replay-component"), UsedImplicitly]
        private ClickableImage _replayComponent;

        private void SetMaterials() {
            _overviewPage1Component.material = BundleLoader.UIAdditiveGlowMaterial;
            _overviewPage2Component.material = BundleLoader.UIAdditiveGlowMaterial;
            _accuracyComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            _gridComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            _graphComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            _replayComponent.material = BundleLoader.UIAdditiveGlowMaterial;
        }

        private static void SetColor(ClickableImage image, bool selected) {
            image.DefaultColor = selected ? SelectedColor : FadedColor;
            image.HighlightColor = selected ? SelectedColor : FadedHoverColor;
        }

        #endregion

        #region ClickEvents

        [UIAction("overview-page1-on-click"), UsedImplicitly]
        private void OverviewPage1OnClick() {
            LeaderboardState.ScoreInfoPanelTab = ScoreInfoPanelTab.OverviewPage1;
        }

        [UIAction("overview-page2-on-click"), UsedImplicitly]
        private void OverviewPage2OnClick() {
            LeaderboardState.ScoreInfoPanelTab = ScoreInfoPanelTab.OverviewPage2;
        }

        [UIAction("accuracy-on-click"), UsedImplicitly]
        private void AccuracyOnClick() {
            LeaderboardState.ScoreInfoPanelTab = ScoreInfoPanelTab.Accuracy;
        }

        [UIAction("grid-on-click"), UsedImplicitly]
        private void GridOnClick() {
            LeaderboardState.ScoreInfoPanelTab = ScoreInfoPanelTab.Grid;
        }

        [UIAction("graph-on-click"), UsedImplicitly]
        private void GraphOnClick() {
            LeaderboardState.ScoreInfoPanelTab = ScoreInfoPanelTab.Graph;
        }

        [UIAction("replay-on-click"), UsedImplicitly]
        private void ReplayOnClick() {
            LeaderboardState.ScoreInfoPanelTab = ScoreInfoPanelTab.Replay;
        }

        #endregion
    }
}