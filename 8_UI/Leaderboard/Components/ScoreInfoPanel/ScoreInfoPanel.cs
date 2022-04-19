using System;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.ScoreInfoPanel.bsml")]
    internal class ScoreInfoPanel : ReeUIComponent {
        #region Components

        [UIValue("mini-profile"), UsedImplicitly]
        private MiniProfile _miniProfile = Instantiate<MiniProfile>();

        [UIValue("score-stats-loading-screen"), UsedImplicitly]
        private ScoreStatsLoadingScreen _scoreStatsLoadingScreen = Instantiate<ScoreStatsLoadingScreen>();

        [UIValue("score-overview"), UsedImplicitly]
        private ScoreOverview _scoreOverview = Instantiate<ScoreOverview>();

        [UIValue("accuracy-details"), UsedImplicitly]
        private AccuracyDetails _accuracyDetails = Instantiate<AccuracyDetails>();

        [UIValue("accuracy-grid"), UsedImplicitly]
        private AccuracyGrid _accuracyGrid = Instantiate<AccuracyGrid>();

        [UIValue("accuracy-graph"), UsedImplicitly]
        private AccuracyGraph _accuracyGraph = Instantiate<AccuracyGraph>();

        [UIValue("controls"), UsedImplicitly]
        private ScoreInfoPanelControls _controls = Instantiate<ScoreInfoPanelControls>();

        #endregion

        #region Init / Dispose

        protected override void OnInitialize() {
            LeaderboardState.ScoreInfoPanelTabChangedEvent += OnTabWasSelected;
            LeaderboardState.ScoreStatsRequest.FinishedEvent += SetScoreStats;
            OnTabWasSelected(LeaderboardState.ScoreInfoPanelTab);
        }

        protected override void OnDispose() {
            LeaderboardState.ScoreInfoPanelTabChangedEvent -= OnTabWasSelected;
            LeaderboardState.ScoreStatsRequest.FinishedEvent -= SetScoreStats;
        }

        #endregion

        #region OnTabWasSelected

        private void OnTabWasSelected(ScoreInfoPanelTab tab) {
            switch (tab) {
                case ScoreInfoPanelTab.Overview: break;
                case ScoreInfoPanelTab.Accuracy:
                case ScoreInfoPanelTab.Grid:
                case ScoreInfoPanelTab.Graph:
                    if (_score == null) return;
                    LeaderboardEvents.RequestScoreStats(_score.id);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(tab), tab, null);
            }
            UpdateVisibility();
        }

        #endregion

        #region UpdateVisibility

        private void UpdateVisibility() {
            _scoreOverview.SetActive(false);
            _accuracyDetails.SetActive(false);
            _accuracyGrid.SetActive(false);
            _accuracyGraph.SetActive(false);
            _scoreStatsLoadingScreen.SetActive(false);
            
            switch (LeaderboardState.ScoreInfoPanelTab) {
                case ScoreInfoPanelTab.Overview:
                    _scoreOverview.SetActive(true);
                    break;
                case ScoreInfoPanelTab.Accuracy:
                    _accuracyDetails.SetActive(!_showLoadingScreen);
                    _scoreStatsLoadingScreen.SetActive(_showLoadingScreen);
                    break;
                case ScoreInfoPanelTab.Grid:
                    _accuracyGrid.SetActive(!_showLoadingScreen);
                    _scoreStatsLoadingScreen.SetActive(_showLoadingScreen);
                    break;
                case ScoreInfoPanelTab.Graph:
                    _accuracyGraph.SetActive(!_showLoadingScreen);
                    _scoreStatsLoadingScreen.SetActive(_showLoadingScreen);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region SetScore

        private bool _showLoadingScreen;
        private Score _score;

        private void SetScoreStats(ScoreStats scoreStats) {
            _accuracyDetails.SetScoreStats(scoreStats);
            _showLoadingScreen = false;
            UpdateVisibility();
        }

        public void SetScore(Score score) {
            _score = score;
            _showLoadingScreen = true;
            _miniProfile.SetPlayer(score.player);
            _scoreOverview.SetScore(score);
            _accuracyDetails.Clear();
            _accuracyGrid.Clear();
            _accuracyGraph.Clear();
            _controls.SetScore(score);
            UpdateVisibility();
        }

        #endregion
    }
}