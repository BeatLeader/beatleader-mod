using System;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ReplayStatisticsPanel : ReeUIComponentV2 {
        #region Components

        [UIValue("score-stats-loading-screen"), UsedImplicitly]
        private ScoreStatsLoadingScreen _scoreStatsLoadingScreen = null!;

        [UIValue("score-overview-page1"), UsedImplicitly]
        private ScoreOverviewPage1 _scoreOverviewPage1 = null!;

        [UIValue("score-overview-page2"), UsedImplicitly]
        private ScoreOverviewPage2 _scoreOverviewPage2 = null!;

        [UIValue("accuracy-details"), UsedImplicitly]
        private AccuracyDetails _accuracyDetails = null!;

        [UIValue("accuracy-grid"), UsedImplicitly]
        private AccuracyGrid _accuracyGrid = null!;

        [UIValue("accuracy-graph"), UsedImplicitly]
        private AccuracyGraphPanel _accuracyGraphPanel = null!;

        [UIValue("replay-panel"), UsedImplicitly]
        private ReplayerSettingsPanel _replaySettingsPanel = null!;

        [UIValue("panel-controls"), UsedImplicitly]
        private ScoreInfoPanelControls _panelControls = null!;

        [UIObject("panel-controls-container")]
        private readonly GameObject _panelControlsContainer = null!;

        #endregion

        #region Init

        protected override void OnInstantiate() {
            _scoreStatsLoadingScreen = Instantiate<ScoreStatsLoadingScreen>(transform);
            _scoreOverviewPage1 = Instantiate<ScoreOverviewPage1>(transform);
            _scoreOverviewPage2 = Instantiate<ScoreOverviewPage2>(transform);
            _accuracyDetails = Instantiate<AccuracyDetails>(transform);
            _accuracyGrid = Instantiate<AccuracyGrid>(transform);
            _accuracyGraphPanel = Instantiate<AccuracyGraphPanel>(transform);
            _replaySettingsPanel = Instantiate<ReplayerSettingsPanel>(transform);
            _panelControls = Instantiate<ScoreInfoPanelControls>(transform);

            _panelControls.followLeaderboardEvents = false;
            //_panelControls.TabsMask &= ~ScoreInfoPanelTab.OverviewPage2;
            _panelControls.TabsMask &= ~ScoreInfoPanelTab.Replay;

            _panelControls.TabChangedEvent += HandleSelectedTabChanged;
            UpdateVisibility(ScoreInfoPanelTab.OverviewPage1);
        }

        #endregion

        #region SetScore

        private bool _scoreStatsUpdateRequired;

        public void SwitchTab(ScoreInfoPanelTab tab) {
            if (_scoreStatsUpdateRequired) {
                _openedTab = tab;
                return;
            }
            UpdateVisibility(tab);
        }

        public void SetData(Score? score, ScoreStats? stats, bool invalid, bool notSelected = false) {
            if (score is null || stats is null) {
                _scoreStatsLoadingScreen.SetFailed(invalid, notSelected ? string.Empty : null);
                _panelControlsContainer.SetActive(!invalid);
                _scoreStatsUpdateRequired = true;
                UpdateVisibility(invalid ? ScoreInfoPanelTab.OverviewPage1 : _openedTab);
                return;
            }
            _scoreStatsUpdateRequired = false;
            _scoreOverviewPage1.SetScore(score);
            _scoreOverviewPage2.SetScoreAndStats(score, stats);
            _accuracyDetails.SetScoreStats(stats);
            _accuracyGrid.SetScoreStats(stats);
            _accuracyGraphPanel.SetScoreStats(stats);
            UpdateVisibility(_openedTab);
        }

        #endregion

        #region UpdateVisibility

        private ScoreInfoPanelTab _openedTab;

        private void UpdateVisibility(ScoreInfoPanelTab tab) {
            _scoreStatsLoadingScreen.SetActive(false);
            _scoreOverviewPage1.SetActive(false);
            _scoreOverviewPage2.SetActive(false);
            _accuracyDetails.SetActive(false);
            _accuracyGrid.SetActive(false);
            _accuracyGraphPanel.SetActive(false);
            _replaySettingsPanel.SetActive(false);

            switch (tab) {
                case ScoreInfoPanelTab.OverviewPage1:
                    _scoreOverviewPage1.SetActive(!_scoreStatsUpdateRequired);
                    break;
                case ScoreInfoPanelTab.OverviewPage2:
                    _scoreOverviewPage2.SetActive(!_scoreStatsUpdateRequired);
                    break;
                case ScoreInfoPanelTab.Accuracy:
                    _accuracyDetails.SetActive(!_scoreStatsUpdateRequired);
                    break;
                case ScoreInfoPanelTab.Grid:
                    _accuracyGrid.SetActive(!_scoreStatsUpdateRequired);
                    break;
                case ScoreInfoPanelTab.Graph:
                    _accuracyGraphPanel.SetActive(!_scoreStatsUpdateRequired);
                    break;
            }

            _scoreStatsLoadingScreen.SetActive(_scoreStatsUpdateRequired);
            _openedTab = tab;
        }

        #endregion

        #region Callbacks

        private void HandleSelectedTabChanged(ScoreInfoPanelTab tab) {
            SwitchTab(tab);
        }

        #endregion
    }
}