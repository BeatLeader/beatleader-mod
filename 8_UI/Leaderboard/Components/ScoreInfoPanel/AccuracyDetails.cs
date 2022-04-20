using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.AccuracyDetails.bsml")]
    internal class AccuracyDetails : ReeUIComponent {
        #region Clear

        public void Clear() {
            LeftPreScore = "";
            LeftAccScore = "";
            LeftPostScore = "";
            LeftTotalScore = "";
            LeftTD = "";
            LeftPrePercentage = "";
            LeftPostPercentage = "";

            RightPreScore = "";
            RightAccScore = "";
            RightPostScore = "";
            RightTotalScore = "";
            RightTD = "";
            RightPrePercentage = "";
            RightPostPercentage = "";
        }

        #endregion

        #region SetScoreStats

        public void SetScoreStats(ScoreStats scoreStats) {
            var tracker = scoreStats.accuracyTracker;

            LeftPreScore = FormatAverageScore(tracker.leftAverageCut[0]);
            LeftAccScore = FormatAverageScore(tracker.leftAverageCut[1]);
            LeftPostScore = FormatAverageScore(tracker.leftAverageCut[2]);
            LeftTotalScore = FormatAverageScore(tracker.accLeft);
            LeftTD = FormatTimeDependence(tracker.leftTimeDependence);
            LeftPrePercentage = FormatSwingPercentage(tracker.leftPreswing);
            LeftPostPercentage = FormatSwingPercentage(tracker.leftPostswing);

            RightPreScore = FormatAverageScore(tracker.rightAverageCut[0]);
            RightAccScore = FormatAverageScore(tracker.rightAverageCut[1]);
            RightPostScore = FormatAverageScore(tracker.rightAverageCut[2]);
            RightTotalScore = FormatAverageScore(tracker.accRight);
            RightTD = FormatTimeDependence(tracker.rightTimeDependence);
            RightPrePercentage = FormatSwingPercentage(tracker.rightPreswing);
            RightPostPercentage = FormatSwingPercentage(tracker.rightPostswing);
        }

        #endregion

        #region Formatting

        private static string FormatAverageScore(float value) {
            return $"{value:F2}";
        }

        private static string FormatTimeDependence(float value) {
            return $"{value:F3}";
        }

        private static string FormatSwingPercentage(float value) {
            return $"{value * 100f:F2}<size=60%>%";
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

        #region Left Hand Fields

        #region LeftPreScore

        private string _leftPreScore = "";

        [UIValue("left-pre-score"), UsedImplicitly]
        private string LeftPreScore {
            get => _leftPreScore;
            set {
                if (_leftPreScore.Equals(value)) return;
                _leftPreScore = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region LeftAccScore

        private string _leftAccScore = "";

        [UIValue("left-acc-score"), UsedImplicitly]
        private string LeftAccScore {
            get => _leftAccScore;
            set {
                if (_leftAccScore.Equals(value)) return;
                _leftAccScore = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region LeftPostScore

        private string _leftPostScore = "";

        [UIValue("left-post-score"), UsedImplicitly]
        private string LeftPostScore {
            get => _leftPostScore;
            set {
                if (_leftPostScore.Equals(value)) return;
                _leftPostScore = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region LeftTotalScore

        private string _leftTotalScore = "";

        [UIValue("left-total-score"), UsedImplicitly]
        private string LeftTotalScore {
            get => _leftTotalScore;
            set {
                if (_leftTotalScore.Equals(value)) return;
                _leftTotalScore = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region LeftTD

        private string _leftTD = "";

        [UIValue("left-td"), UsedImplicitly]
        private string LeftTD {
            get => _leftTD;
            set {
                if (_leftTD.Equals(value)) return;
                _leftTD = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region LeftPrePercentage

        private string _leftPrePercentage = "";

        [UIValue("left-pre-percentage"), UsedImplicitly]
        private string LeftPrePercentage {
            get => _leftPrePercentage;
            set {
                if (_leftPrePercentage.Equals(value)) return;
                _leftPrePercentage = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region LeftPostPercentage

        private string _leftPostPercentage = "";

        [UIValue("left-post-percentage"), UsedImplicitly]
        private string LeftPostPercentage {
            get => _leftPostPercentage;
            set {
                if (_leftPostPercentage.Equals(value)) return;
                _leftPostPercentage = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #endregion

        #region Right Hand Fields

        #region RightPreScore

        private string _rightPreScore = "";

        [UIValue("right-pre-score"), UsedImplicitly]
        private string RightPreScore {
            get => _rightPreScore;
            set {
                if (_rightPreScore.Equals(value)) return;
                _rightPreScore = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region RightAccScore

        private string _rightAccScore = "";

        [UIValue("right-acc-score"), UsedImplicitly]
        private string RightAccScore {
            get => _rightAccScore;
            set {
                if (_rightAccScore.Equals(value)) return;
                _rightAccScore = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region RightPostScore

        private string _rightPostScore = "";

        [UIValue("right-post-score"), UsedImplicitly]
        private string RightPostScore {
            get => _rightPostScore;
            set {
                if (_rightPostScore.Equals(value)) return;
                _rightPostScore = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region RightTotalScore

        private string _rightTotalScore = "";

        [UIValue("right-total-score"), UsedImplicitly]
        private string RightTotalScore {
            get => _rightTotalScore;
            set {
                if (_rightTotalScore.Equals(value)) return;
                _rightTotalScore = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region RightTD

        private string _rightTD = "";

        [UIValue("right-td"), UsedImplicitly]
        private string RightTD {
            get => _rightTD;
            set {
                if (_rightTD.Equals(value)) return;
                _rightTD = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region RightPrePercentage

        private string _rightPrePercentage = "";

        [UIValue("right-pre-percentage"), UsedImplicitly]
        private string RightPrePercentage {
            get => _rightPrePercentage;
            set {
                if (_rightPrePercentage.Equals(value)) return;
                _rightPrePercentage = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region RightPostPercentage

        private string _rightPostPercentage = "";

        [UIValue("right-post-percentage"), UsedImplicitly]
        private string RightPostPercentage {
            get => _rightPostPercentage;
            set {
                if (_rightPostPercentage.Equals(value)) return;
                _rightPostPercentage = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #endregion
    }
}