using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReplayStatisticsPanel : ReactiveComponent {
        #region Components

        private ScoreStatsLoadingScreen _scoreStatsLoadingScreen = null!;
        private ScoreOverviewPage1 _scoreOverviewPage1 = null!;
        private ScoreOverviewPage2 _scoreOverviewPage2 = null!;
        private AccuracyDetails _accuracyDetails = null!;
        private AccuracyGrid _accuracyGrid = null!;
        private AccuracyGraphPanel _accuracyGraphPanel = null!;
        private ReplayerSettingsPanel _replaySettingsPanel = null!;
        private ScoreInfoPanelControls _panelControls = null!;
        private ReactiveComponent _panelControlsContainer = null!;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new Background {
                        Children = {
                            new ReeWrapperV2<ScoreStatsLoadingScreen>()
                                .WithRectExpand()
                                .BindRee(ref _scoreStatsLoadingScreen),

                            new ReeWrapperV2<ScoreOverviewPage1>()
                                .WithRectExpand()
                                .BindRee(ref _scoreOverviewPage1),

                            new ReeWrapperV2<ScoreOverviewPage2>()
                                .WithRectExpand()
                                .BindRee(ref _scoreOverviewPage2),

                            new ReeWrapperV2<AccuracyDetails>()
                                .WithRectExpand()
                                .BindRee(ref _accuracyDetails),

                            new ReeWrapperV2<AccuracyGrid>()
                                .WithRectExpand()
                                .BindRee(ref _accuracyGrid),

                            new ReeWrapperV2<AccuracyGraphPanel>()
                                .WithRectExpand()
                                .BindRee(ref _accuracyGraphPanel),

                            new ReeWrapperV2<ReplayerSettingsPanel>()
                                .WithRectExpand()
                                .BindRee(ref _replaySettingsPanel)
                        }
                    }.AsBlurBackground().AsFlexItem(grow: 1f),

                    new ReeWrapperV2<ScoreInfoPanelControls>()
                        .With(
                            x => {
                                var ree = x.ReeComponent;
                                ree.TabsMask &= ~ScoreInfoPanelTab.Replay;
                                ree.followLeaderboardEvents = false;
                                ree.TabChangedEvent += SwitchTab;
                            }
                        )
                        .AsFlexItem(grow: 1f)
                        .BindRee(ref _panelControls)
                        .InBlurBackground()
                        .AsFlexGroup(padding: 1f)
                        .AsFlexItem(size: new() { y = 6 })
                        .Bind(ref _panelControlsContainer)
                }
            }.AsFlexGroup(direction: FlexDirection.Column, gap: 0.5f).Use();
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

        public async Task SetDataByHeaderAsync(IReplayHeader header, CancellationToken token = default) {
            if (!StatsStorage.TryGetStats(header, out var score, out var stats)) {
                //loading if wasn't represented in the cache
                var replay = await header.LoadReplayAsync(token);

                if (token.IsCancellationRequested) {
                    return;
                }

                if (replay != null) {
                    stats = await Task.Run(() => ReplayStatisticUtils.ComputeScoreStats(replay), token);
                    score = ReplayUtils.ComputeScore(replay);
                    score.fcAccuracy = stats?.accuracyTracker.fcAcc ?? 0;
                }

                StatsStorage.AddStats(header, score, stats);
            }

            if (score == null || stats == null) {
                SetFailed();
                return;
            }

            SetData(score, stats);
        }

        public void SetLoading() {
            _panelControlsContainer.Enabled = false;
            
            HideAllTabs();
            _scoreStatsLoadingScreen.SetActive(true);
            _scoreStatsLoadingScreen.SetFailed(false);
        }

        private void SetData(Score score, ScoreStats stats) {
            _scoreStatsUpdateRequired = false;
            _scoreOverviewPage1.SetScore(score);
            _scoreOverviewPage2.SetScoreAndStats(score, stats);
            _accuracyDetails.SetScoreStats(stats);
            _accuracyGrid.SetScoreStats(stats);
            _accuracyGraphPanel.SetScoreStats(stats);

            UpdateVisibility(_openedTab);
        }

        private void SetFailed() {
            _scoreStatsLoadingScreen.SetFailed(true);
            _panelControls.gameObject.SetActive(false);
            _scoreStatsUpdateRequired = true;

            UpdateVisibility(ScoreInfoPanelTab.OverviewPage1);
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
            _panelControlsContainer.Enabled = true;
            _openedTab = tab;
        }

        #endregion
    }
}