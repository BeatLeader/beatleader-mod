using System;
using BeatLeader.API.Methods;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class ScoreInfoPanel : ReeUIComponentV2 {
        #region Components

        [UIValue("mini-profile"), UsedImplicitly]
        private MiniProfile _miniProfile;

        [UIValue("score-stats-loading-screen"), UsedImplicitly]
        private ScoreStatsLoadingScreen _scoreStatsLoadingScreen;

        [UIValue("score-overview-page1"), UsedImplicitly]
        private ScoreOverviewPage1 _scoreOverviewPage1;

        [UIValue("score-overview-page2"), UsedImplicitly]
        private ScoreOverviewPage2 _scoreOverviewPage2;

        [UIValue("accuracy-details"), UsedImplicitly]
        private AccuracyDetails _accuracyDetails;

        [UIValue("accuracy-grid"), UsedImplicitly]
        private AccuracyGrid _accuracyGrid;

        [UIValue("accuracy-graph"), UsedImplicitly]
        private AccuracyGraphPanel _accuracyGraphPanel;

        [UIValue("replay-panel"), UsedImplicitly]
        private ReplayPanel _replayPanel;

        [UIValue("controls"), UsedImplicitly]
        private ScoreInfoPanelControls _controls;

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
        }

        #endregion

        #region Init / Dispose

        protected override void OnInitialize() {
            InitializeModal();
            
            ScoreStatsRequest.AddStateListener(OnScoreStatsRequestStateChanged);
            LeaderboardEvents.ScoreInfoButtonWasPressed += OnScoreInfoButtonWasPressed;
            LeaderboardEvents.HideAllOtherModalsEvent += OnHideModalsEvent;
            LeaderboardState.IsVisibleChangedEvent += OnLeaderboardVisibilityChanged;
            LeaderboardState.ScoreInfoPanelTabChangedEvent += OnTabWasSelected;
            OnTabWasSelected(LeaderboardState.ScoreInfoPanelTab);
        }

        protected override void OnDispose() {
            ScoreStatsRequest.RemoveStateListener(OnScoreStatsRequestStateChanged);
            LeaderboardEvents.ScoreInfoButtonWasPressed -= OnScoreInfoButtonWasPressed;
            LeaderboardEvents.HideAllOtherModalsEvent -= OnHideModalsEvent;
            LeaderboardState.IsVisibleChangedEvent -= OnLeaderboardVisibilityChanged;
            LeaderboardState.ScoreInfoPanelTabChangedEvent -= OnTabWasSelected;
        }

        #endregion

        #region Events

        private void OnHideModalsEvent(ModalView except) {
            if (_modal == null || _modal.Equals(except)) return;
            _modal.Hide(false);
        }

        private void OnLeaderboardVisibilityChanged(bool isVisible) {
            if (!isVisible) HideAnimated();
        }

        private void OnScoreInfoButtonWasPressed(Score score) {
            SetScore(score);
            ShowModal();
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
            _accuracyGraphPanel.SetActive(false);
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
                    _accuracyGraphPanel.SetActive(!_scoreStatsUpdateRequired);
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
        private Score _score;

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
            _miniProfile.SetPlayer(score.player);
            _scoreOverviewPage1.SetScore(score);
            _replayPanel.SetScore(score);
            _controls.Reset();
            UpdateVisibility();
        }

        #endregion

        #region Modal

        [UIComponent("modal"), UsedImplicitly]
        private ModalView _modal;
        
        [UIComponent("middle-panel"), UsedImplicitly]
        private ImageView _middlePanel;
        
        [UIComponent("bottom-panel"), UsedImplicitly]
        private ImageView _bottomPanel;

        private void InitializeModal() {
            var background = _modal.GetComponentInChildren<ImageView>();
            if (background != null) background.enabled = false;
            var touchable = _modal.GetComponentInChildren<Touchable>();
            if (touchable != null) touchable.enabled = false;
            
            _middlePanel.raycastTarget = true;
            _bottomPanel.raycastTarget = true;
        }

        private void ShowModal() {
            if (_modal == null) return;
            LeaderboardEvents.FireHideAllOtherModalsEvent(_modal);
            _modal.Show(true, true);
        }

        private void HideAnimated() {
            if (_modal == null) return;
            _modal.Hide(true);
        }

        #endregion
    }
}