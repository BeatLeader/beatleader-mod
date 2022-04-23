using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.AccuracyDetails.bsml")]
    internal class AccuracyDetails : ReeUIComponent {
        #region Components

        [UIValue("left-averages"), UsedImplicitly]
        private AccuracyDetailsAverages _leftAverages = Instantiate<AccuracyDetailsAverages>();

        [UIValue("left-pie-chart"), UsedImplicitly]
        private AccuracyPieChart _leftPieChart = Instantiate<AccuracyPieChart>();

        [UIValue("right-averages"), UsedImplicitly]
        private AccuracyDetailsAverages _rightAverages = Instantiate<AccuracyDetailsAverages>();

        [UIValue("right-pie-chart"), UsedImplicitly]
        private AccuracyPieChart _rightPieChart = Instantiate<AccuracyPieChart>();

        [UIValue("td-row"), UsedImplicitly]
        private AccuracyDetailsRow _tdRow = Instantiate<AccuracyDetailsRow>();

        [UIValue("pre-row"), UsedImplicitly]
        private AccuracyDetailsRow _preRow = Instantiate<AccuracyDetailsRow>();

        [UIValue("post-row"), UsedImplicitly]
        private AccuracyDetailsRow _postRow = Instantiate<AccuracyDetailsRow>();

        #endregion

        #region SetScoreStats

        public void SetScoreStats(ScoreStats scoreStats) {
            var tracker = scoreStats.accuracyTracker;

            _leftAverages.SetValues(tracker.leftAverageCut[0], tracker.leftAverageCut[1], tracker.leftAverageCut[2]);
            _rightAverages.SetValues(tracker.rightAverageCut[0], tracker.rightAverageCut[1], tracker.rightAverageCut[2]);
            
            _leftPieChart.SetValues(AccuracyPieChart.Type.Left, tracker.accLeft);
            _rightPieChart.SetValues(AccuracyPieChart.Type.Right, tracker.accRight);
            
            _tdRow.SetValues(AccuracyDetailsRow.Type.TD, tracker.leftTimeDependence, tracker.rightTimeDependence);
            _preRow.SetValues(AccuracyDetailsRow.Type.Pre, tracker.leftPreswing, tracker.rightPreswing);
            _postRow.SetValues(AccuracyDetailsRow.Type.Post, tracker.leftPostswing, tracker.rightPostswing);
        }

        #endregion

        #region SetActive

        public void SetActive(bool value) {
            Active = value;
        }

        #endregion

        #region Active

        private bool _active = true;

        [UIValue("active"), UsedImplicitly]
        private bool Active {
            get => _active;
            set {
                if (_active.Equals(value)) return;
                _active = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}