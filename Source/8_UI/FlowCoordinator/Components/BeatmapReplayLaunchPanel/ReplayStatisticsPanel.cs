using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.UI.Hub {
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

        [UIObject("panel-controls-container"), UsedImplicitly]
        private GameObject? _panelControlsContainer;

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
            _panelControls.TabsMask &= ~ScoreInfoPanelTab.Replay;

            _panelControls.TabChangedEvent += HandleSelectedTabChanged;
        }

        #endregion

        #region Dismiss

        public void PrepareForDismiss() {
            HideAllTabs();
        }

        public void PrepareForDisplay() {
            UpdateVisibility(_openedTab);
        }

        #endregion

        #region SwitchTab

        private bool _scoreStatsUpdateRequired;

        public void SwitchTab(ScoreInfoPanelTab tab) {
            if (_scoreStatsUpdateRequired) {
                _openedTab = tab;
                return;
            }
            UpdateVisibility(tab);
        }

        #endregion

        #region SetScore

        private CancellationTokenSource _cancellationTokenSource = new();

        public Task SetDataByHeaderAsync(IReplayHeader? header) {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new();
            return SetDataByHeaderAsyncInternal(header, _cancellationTokenSource.Token);
        }

        private async Task SetDataByHeaderAsyncInternal(IReplayHeader? header, CancellationToken token) {
            if (header is null) {
                SetData(null, null, true, true);
                return;
            }
            var replay = await header.LoadReplayAsync(default);
            if (token.IsCancellationRequested) return;

            ScoreStats? stats = null;
            Score? score = null;
            if (replay is not null) {
                stats = await Task.Run(() => ReplayStatisticUtils.ComputeScoreStats(replay), token);
                score = ReplayUtils.ComputeScore(replay);
                score.fcAccuracy = stats?.accuracyTracker.fcAcc ?? 0;
            }
            if (token.IsCancellationRequested) return;
            SetData(score, stats, score is null || stats is null);
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

        private ScoreInfoPanelTab _openedTab = ScoreInfoPanelTab.OverviewPage1;

        private void HideAllTabs() {
            _scoreStatsLoadingScreen.SetActive(false);
            _scoreOverviewPage1.SetActive(false);
            _scoreOverviewPage2.SetActive(false);
            _accuracyDetails.SetActive(false);
            _accuracyGrid.SetActive(false);
            _accuracyGraphPanel.SetActive(false);
            _replaySettingsPanel.SetActive(false);
        }

        private void UpdateVisibility(ScoreInfoPanelTab tab) {
            HideAllTabs();

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
            _panelControlsContainer?.SetActive(true);
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