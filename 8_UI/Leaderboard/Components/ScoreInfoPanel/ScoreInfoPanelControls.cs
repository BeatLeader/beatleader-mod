using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.ScoreInfoPanelControls.bsml")]
    internal class ScoreInfoPanelControls : ReeUIComponent {
        #region SetScore

        private Score _score;

        public void SetScore(Score score) {
            SelectTab(DefaultTab);
            _score = score;
        }

        #endregion

        #region SelectTab

        public const ScoreInfoPanelTab DefaultTab = ScoreInfoPanelTab.Overview;
        private ScoreInfoPanelTab _currentTab = DefaultTab;

        private void SelectTab(ScoreInfoPanelTab tab) {
            if (tab.Equals(_currentTab)) return;

            _currentTab = tab;
            LeaderboardEvents.NotifyScoreInfoPanelTabWasSelected(tab);
        }

        #endregion

        #region TempButtons

        [UIAction("overview-on-click"), UsedImplicitly]
        private void OverviewOnClick() {
            SelectTab(ScoreInfoPanelTab.Overview);
        }

        [UIAction("accuracy-on-click"), UsedImplicitly]
        private void AccuracyOnClick() {
            SelectTab(ScoreInfoPanelTab.Accuracy);
        }

        [UIAction("graph-on-click"), UsedImplicitly]
        private void GraphOnClick() {
            SelectTab(ScoreInfoPanelTab.Graph);
        }

        [UIAction("replay-on-click"), UsedImplicitly]
        private void ReplayOnClick() {
            if (_score == null) return;
            LeaderboardEvents.NotifyReplayButtonWasPressed(_score);
        }

        #endregion
    }
}