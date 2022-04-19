using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.ScoreInfoPanelControls.bsml")]
    internal class ScoreInfoPanelControls : ReeUIComponent {
        #region Events

        protected override void OnInitialize() {
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
            //TODO: Highlight selected tab button
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