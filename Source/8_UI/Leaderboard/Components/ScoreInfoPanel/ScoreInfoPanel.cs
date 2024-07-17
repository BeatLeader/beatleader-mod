using System;
using BeatLeader.API.Methods;
using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ScoreInfoPanel : AbstractReeModal<(Score, IReplayerStarter)> {
        #region Components

        [UIValue("mini-profile"), UsedImplicitly]
        private MiniProfile _miniProfile = null!;

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
        private ReplayPanel _replayPanel = null!;

        [UIValue("controls"), UsedImplicitly]
        private ScoreInfoPanelControls _controls = null!;

        [UIObject("accuracy-graph-container"), UsedImplicitly]
        private GameObject _accuracyGraphContainer = null!;

        [UIComponent("middle-panel"), UsedImplicitly]
        private ImageView _middlePanel = null!;

        [UIComponent("bottom-panel"), UsedImplicitly]
        private ImageView _bottomPanel = null!;

        private void Awake() {
            _miniProfile = Instantiate<MiniProfile>(transform);
            _scoreStatsLoadingScreen = Instantiate<ScoreStatsLoadingScreen>(transform);
            _scoreOverviewPage1 = Instantiate<ScoreOverviewPage1>(transform);
            _scoreOverviewPage2 = Instantiate<ScoreOverviewPage2>(transform);
            _accuracyDetails = Instantiate<AccuracyDetails>(transform);
            _accuracyGrid = Instantiate<AccuracyGrid>(transform);
            _accuracyGraphPanel = Instantiate<AccuracyGraphPanel>(transform);
            _replayPanel = Instantiate<ReplayPanel>(transform);
            _controls = Instantiate<ScoreInfoPanelControls>(transform);

            _replayPanel.DownloadStateChangedEvent += OnReplayDownloadStateChangedEvent;
        }

        #endregion

        #region Init / Dispose

        protected override void OnInitialize() {
            base.OnInitialize();
            _middlePanel.raycastTarget = true;
            _bottomPanel.raycastTarget = true;

            ScoreStatsRequest.AddStateListener(OnScoreStatsRequestStateChanged);
            LeaderboardState.ScoreInfoPanelTabChangedEvent += OnTabWasSelected;
            HiddenPlayersCache.HiddenPlayersUpdatedEvent += RefreshPlayer;
            OnTabWasSelected(LeaderboardState.ScoreInfoPanelTab);
        }

        protected override void OnDispose() {
            ScoreStatsRequest.RemoveStateListener(OnScoreStatsRequestStateChanged);
            HiddenPlayersCache.HiddenPlayersUpdatedEvent -= RefreshPlayer;
            LeaderboardState.ScoreInfoPanelTabChangedEvent -= OnTabWasSelected;
        }

        #endregion

        #region Events

        protected override void OnResume() {
            SetScore(Context.Item1);
            _replayPanel.Setup(Context.Item2);
        }

        private void OnReplayDownloadStateChangedEvent(bool state) {
            offClickCloses = !state;
        }

        private void OnTabWasSelected(ScoreInfoPanelTab tab) {
            switch (tab) {
                case ScoreInfoPanelTab.OverviewPage1:
                case ScoreInfoPanelTab.Replay:
                    break;

                case ScoreInfoPanelTab.OverviewPage2:
                case ScoreInfoPanelTab.Accuracy:
                case ScoreInfoPanelTab.Grid:
                case ScoreInfoPanelTab.Graph:
                    if (_score != null && _scoreStatsUpdateRequired) {
                        LeaderboardEvents.RequestScoreStats(_score.id);
                    }

                    break;
                default: throw new ArgumentOutOfRangeException(nameof(tab), tab, null);
            }

            UpdateVisibility();
        }

        #endregion

        #region UpdateVisibility

        private void UpdateVisibility() {
            _scoreStatsLoadingScreen.SetActive(false);
            _scoreOverviewPage1.SetActive(false);
            _scoreOverviewPage2.SetActive(false);
            _accuracyDetails.SetActive(false);
            _accuracyGrid.SetActive(false);
            _accuracyGraphContainer.SetActive(false);
            _replayPanel.SetActive(false);

            switch (LeaderboardState.ScoreInfoPanelTab) {
                case ScoreInfoPanelTab.OverviewPage1:
                    _scoreOverviewPage1.SetActive(true);
                    break;
                case ScoreInfoPanelTab.OverviewPage2:
                    _scoreOverviewPage2.SetActive(!_scoreStatsUpdateRequired);
                    _scoreStatsLoadingScreen.SetActive(_scoreStatsUpdateRequired);
                    break;
                case ScoreInfoPanelTab.Accuracy:
                    _accuracyDetails.SetActive(!_scoreStatsUpdateRequired);
                    _scoreStatsLoadingScreen.SetActive(_scoreStatsUpdateRequired);
                    break;
                case ScoreInfoPanelTab.Grid:
                    _accuracyGrid.SetActive(!_scoreStatsUpdateRequired);
                    _scoreStatsLoadingScreen.SetActive(_scoreStatsUpdateRequired);
                    break;
                case ScoreInfoPanelTab.Graph:
                    _accuracyGraphContainer.SetActive(!_scoreStatsUpdateRequired);
                    _scoreStatsLoadingScreen.SetActive(_scoreStatsUpdateRequired);
                    break;
                case ScoreInfoPanelTab.Replay:
                    _replayPanel.SetActive(true);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region SetScore

        private bool _scoreStatsUpdateRequired;
        private Score? _score;

        private void OnScoreStatsRequestStateChanged(API.RequestState state, ScoreStats result, string failReason) {
            if (_score == null || state is not API.RequestState.Finished) return;
            _scoreOverviewPage2.SetScoreAndStats(_score, result);
            _accuracyDetails.SetScoreStats(result);
            _accuracyGrid.SetScoreStats(result);
            _accuracyGraphPanel.SetScoreStats(result);
            _scoreStatsUpdateRequired = false;
            UpdateVisibility();
        }

        public void SetScore(Score score) {
            _score = score;
            _scoreStatsUpdateRequired = true;
            _miniProfile.SetPlayer(score.Player);
            _scoreOverviewPage1.SetScore(score);
            _replayPanel.SetScore(score);
            _controls.Reset();
            UpdateVisibility();
        }

        private void RefreshPlayer() {
            _miniProfile.SetPlayer(_score.Player);
        }

        #endregion
    }
}