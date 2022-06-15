using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class AccuracyDetails : ReeUIComponentV2 {
        #region Components

        [UIValue("left-averages"), UsedImplicitly]
        private AccuracyDetailsAverages _leftAverages;

        [UIValue("left-pie-chart"), UsedImplicitly]
        private AccuracyPieChart _leftPieChart;

        [UIValue("right-averages"), UsedImplicitly]
        private AccuracyDetailsAverages _rightAverages;

        [UIValue("right-pie-chart"), UsedImplicitly]
        private AccuracyPieChart _rightPieChart;

        [UIValue("td-row"), UsedImplicitly]
        private AccuracyDetailsRow _tdRow;

        [UIValue("pre-row"), UsedImplicitly]
        private AccuracyDetailsRow _preRow;

        [UIValue("post-row"), UsedImplicitly]
        private AccuracyDetailsRow _postRow;

        private void Awake() {
            _leftAverages = Instantiate<AccuracyDetailsAverages>(transform);
            _leftPieChart = Instantiate<AccuracyPieChart>(transform);
            _rightAverages = Instantiate<AccuracyDetailsAverages>(transform);
            _rightPieChart = Instantiate<AccuracyPieChart>(transform);
            _tdRow = Instantiate<AccuracyDetailsRow>(transform);
            _preRow = Instantiate<AccuracyDetailsRow>(transform);
            _postRow = Instantiate<AccuracyDetailsRow>(transform);
        }

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