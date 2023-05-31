using System;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace BeatLeader.Components {
    internal class ScoreInfoPanelControls : ReeUIComponentV2 {
        #region Events

        public event Action<ScoreInfoPanelTab>? TabChangedEvent;

        #endregion

        #region Init & Dispose

        public bool followLeaderboardEvents = true;

        private bool _isInitialized;

        protected override void OnInitialize() {
            SetMaterials();
            LeaderboardState.ScoreInfoPanelTabChangedEvent += OnScoreInfoPanelTabChanged;
            ChangeTab(followLeaderboardEvents ? LeaderboardState.ScoreInfoPanelTab : ScoreInfoPanelTab.OverviewPage1);
            UpdateVisibility();
            _isInitialized = true;
        }

        protected override void OnDispose() {
            LeaderboardState.ScoreInfoPanelTabChangedEvent -= OnScoreInfoPanelTabChanged;
        }

        #endregion

        #region Reset

        public void Reset() {
            ChangeTab(ScoreInfoPanelTab.OverviewPage1);
        }

        #endregion

        #region ScoreInfoPanelTab

        private void OnScoreInfoPanelTabChanged(ScoreInfoPanelTab tab) {
            if (!followLeaderboardEvents) return;
            UpdateColors(tab);
        }

        private void ChangeTab(ScoreInfoPanelTab tab) {
            if (!followLeaderboardEvents) TabChangedEvent?.Invoke(tab);
            else LeaderboardState.ScoreInfoPanelTab = tab;
            UpdateColors(tab);
        }

        #endregion

        #region UI

        private static readonly Color selectedColor = new(0.0f, 0.4f, 1.0f, 1.0f);
        private static readonly Color fadedColor = new(0.8f, 0.8f, 0.8f, 0.2f);
        private static readonly Color fadedHoverColor = new(0.5f, 0.5f, 0.5f, 0.2f);

        [UIComponent("overview-page1-component"), UsedImplicitly]
        private ClickableImage _overviewPage1Component = null!;

        [UIComponent("overview-page2-component"), UsedImplicitly]
        private ClickableImage _overviewPage2Component = null!;

        [UIComponent("accuracy-component"), UsedImplicitly]
        private ClickableImage _accuracyComponent = null!;

        [UIComponent("grid-component"), UsedImplicitly]
        private ClickableImage _gridComponent = null!;

        [UIComponent("graph-component"), UsedImplicitly]
        private ClickableImage _graphComponent = null!;

        [UIComponent("replay-component"), UsedImplicitly]
        private ClickableImage _replayComponent = null!;

        public ScoreInfoPanelTab TabsMask {
            get => _tabsMask;
            set {
                _tabsMask = value;
                if (!_isInitialized) return;
                UpdateVisibility();
            }
        }

        private ScoreInfoPanelTab _tabsMask =
            ScoreInfoPanelTab.OverviewPage1 |
            ScoreInfoPanelTab.OverviewPage2 |
            ScoreInfoPanelTab.Accuracy |
            ScoreInfoPanelTab.Grid |
            ScoreInfoPanelTab.Graph |
            ScoreInfoPanelTab.Replay;

        private void SetMaterials() {
            _overviewPage1Component.material = BundleLoader.UIAdditiveGlowMaterial;
            _overviewPage2Component.material = BundleLoader.UIAdditiveGlowMaterial;
            _accuracyComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            _gridComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            _graphComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            _replayComponent.material = BundleLoader.UIAdditiveGlowMaterial;
        }

        private void UpdateColors(ScoreInfoPanelTab tab) {
            SetColor(_overviewPage1Component, tab is ScoreInfoPanelTab.OverviewPage1);
            SetColor(_overviewPage2Component, tab is ScoreInfoPanelTab.OverviewPage2);
            SetColor(_accuracyComponent, tab is ScoreInfoPanelTab.Accuracy);
            SetColor(_gridComponent, tab is ScoreInfoPanelTab.Grid);
            SetColor(_graphComponent, tab is ScoreInfoPanelTab.Graph);
            SetColor(_replayComponent, tab is ScoreInfoPanelTab.Replay);
        }

        private void UpdateVisibility() {
            _overviewPage1Component.gameObject.SetActive(_tabsMask.HasFlag(ScoreInfoPanelTab.OverviewPage1));
            _overviewPage2Component.gameObject.SetActive(_tabsMask.HasFlag(ScoreInfoPanelTab.OverviewPage2));
            _accuracyComponent.gameObject.SetActive(_tabsMask.HasFlag(ScoreInfoPanelTab.Accuracy));
            _gridComponent.gameObject.SetActive(_tabsMask.HasFlag(ScoreInfoPanelTab.Grid));
            _graphComponent.gameObject.SetActive(_tabsMask.HasFlag(ScoreInfoPanelTab.Graph));
            _replayComponent.gameObject.SetActive(_tabsMask.HasFlag(ScoreInfoPanelTab.Replay));
        }

        private static void SetColor(ClickableImage image, bool selected) {
            image.DefaultColor = selected ? selectedColor : fadedColor;
            image.HighlightColor = selected ? selectedColor : fadedHoverColor;
        }

        #endregion

        #region ClickEvents

        [UIAction("overview-page1-on-click"), UsedImplicitly]
        private void OverviewPage1OnClick() {
            ChangeTab(ScoreInfoPanelTab.OverviewPage1);
        }

        [UIAction("overview-page2-on-click"), UsedImplicitly]
        private void OverviewPage2OnClick() {
            ChangeTab(ScoreInfoPanelTab.OverviewPage2);
        }

        [UIAction("accuracy-on-click"), UsedImplicitly]
        private void AccuracyOnClick() {
            ChangeTab(ScoreInfoPanelTab.Accuracy);
        }

        [UIAction("grid-on-click"), UsedImplicitly]
        private void GridOnClick() {
            ChangeTab(ScoreInfoPanelTab.Grid);
        }

        [UIAction("graph-on-click"), UsedImplicitly]
        private void GraphOnClick() {
            ChangeTab(ScoreInfoPanelTab.Graph);
        }

        [UIAction("replay-on-click"), UsedImplicitly]
        private void ReplayOnClick() {
            ChangeTab(ScoreInfoPanelTab.Replay);
        }

        #endregion
    }
}