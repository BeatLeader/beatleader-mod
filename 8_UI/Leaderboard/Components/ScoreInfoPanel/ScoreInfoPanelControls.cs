using System;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.ScoreInfoPanelControls.bsml")]
    internal class ScoreInfoPanelControls : ReeUIComponent {
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

        private Score _score;

        public void SetScore(Score score) {
            _score = score;
            LeaderboardState.ScoreInfoPanelTab = ScoreInfoPanelTab.Overview;
        }

        #endregion

        #region OnScoreInfoPanelTabChanged
        
        private void OnScoreInfoPanelTabChanged(ScoreInfoPanelTab tab) {
            switch (tab) {
                case ScoreInfoPanelTab.Overview:
                    SetColor(_overviewComponent, true);
                    SetColor(_accuracyComponent, false);
                    SetColor(_gridComponent, false);
                    SetColor(_graphComponent, false);
                    break;
                case ScoreInfoPanelTab.Accuracy:
                    SetColor(_overviewComponent, false);
                    SetColor(_accuracyComponent, true);
                    SetColor(_gridComponent, false);
                    SetColor(_graphComponent, false);
                    break;
                case ScoreInfoPanelTab.Grid:
                    SetColor(_overviewComponent, false);
                    SetColor(_accuracyComponent, false);
                    SetColor(_gridComponent, true);
                    SetColor(_graphComponent, false);
                    break;
                case ScoreInfoPanelTab.Graph:
                    SetColor(_overviewComponent, false);
                    SetColor(_accuracyComponent, false);
                    SetColor(_gridComponent, false);
                    SetColor(_graphComponent, true);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(tab), tab, null);
            }
        }

        #endregion
        
        #region Colors

        private static readonly Color SelectedColor = new Color(0.0f, 0.4f, 1.0f, 1.0f);
        private static readonly Color FadedColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        private static readonly Color FadedHoverColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);

        [UIComponent("overview-component"), UsedImplicitly]
        private ClickableImage _overviewComponent;

        [UIComponent("accuracy-component"), UsedImplicitly]
        private ClickableImage _accuracyComponent;

        [UIComponent("grid-component"), UsedImplicitly]
        private ClickableImage _gridComponent;

        [UIComponent("graph-component"), UsedImplicitly]
        private ClickableImage _graphComponent;

        private void SetMaterials() {
            _overviewComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            _accuracyComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            _gridComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            _graphComponent.material = BundleLoader.UIAdditiveGlowMaterial;
        }

        private static void SetColor(ClickableImage image, bool selected) {
            image.DefaultColor = selected ? SelectedColor : FadedColor;
            image.HighlightColor = selected ? SelectedColor : FadedHoverColor;
        }

        #endregion

        #region TempButtons

        [UIAction("overview-on-click"), UsedImplicitly]
        private void OverviewOnClick() {
            LeaderboardState.ScoreInfoPanelTab = ScoreInfoPanelTab.Overview;
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
            if (_score == null) return;
            LeaderboardEvents.NotifyReplayButtonWasPressed(_score);
        }

        #endregion
    }
}