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

        [UIValue("score-overview"), UsedImplicitly]
        private ScoreOverview _scoreOverview = Instantiate<ScoreOverview>();

        [UIValue("accuracy-details"), UsedImplicitly]
        private AccuracyDetails _accuracyDetails = Instantiate<AccuracyDetails>();

        [UIValue("accuracy-graph"), UsedImplicitly]
        private AccuracyGraph _accuracyGraph = Instantiate<AccuracyGraph>();

        [UIValue("controls"), UsedImplicitly]
        private ScoreInfoPanelControls _controls = Instantiate<ScoreInfoPanelControls>();

        #endregion

        #region Events

        protected override void OnInitialize() {
            LeaderboardEvents.ScoreInfoPanelTabWasSelected += OnTabWasSelected;
            OnTabWasSelected(ScoreInfoPanelControls.DefaultTab);
        }

        private void OnTabWasSelected(ScoreInfoPanelTab tab) {
            switch (tab) {
                case ScoreInfoPanelTab.Overview:
                    _scoreOverview.SetActive(true);
                    _accuracyDetails.SetActive(false);
                    _accuracyGraph.SetActive(false);
                    break;
                case ScoreInfoPanelTab.Accuracy:
                    _scoreOverview.SetActive(false);
                    _accuracyDetails.SetActive(true);
                    _accuracyGraph.SetActive(false);
                    break;
                case ScoreInfoPanelTab.Graph:
                    _scoreOverview.SetActive(false);
                    _accuracyDetails.SetActive(false);
                    _accuracyGraph.SetActive(true);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(tab), tab, null);
            }
        }

        #endregion

        #region SetScore

        public void SetScore(Score score) {
            _miniProfile.SetPlayer(score.player);
            _scoreOverview.SetScore(score);
            _accuracyDetails.SetScore(score);
            _accuracyGraph.SetScore(score);
            _controls.SetScore(score);
        }

        #endregion
    }
}