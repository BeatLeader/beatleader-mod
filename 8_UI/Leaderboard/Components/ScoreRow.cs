using System.Globalization;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreRow.bsml")]
    internal class ScoreRow : ReeUiComponent {
        #region SetScore

        public void SetScore(Score score, bool highlight) {
            RankText = $"{score.rank}";
            NameText = FormatName(score.player.name);
            AccText = FormatAcc(score.accuracy);
            PpText = FormatPP(score.pp);
            ScoreText = FormatScore(score.baseScore);
            InfoIsActive = true;
            BackgroundColor = highlight ? HighlightColor : FadedColor;
        }

        public void ClearScore() {
            RankText = "";
            NameText = "";
            AccText = "";
            PpText = "";
            ScoreText = "";
            InfoIsActive = false;
        }

        #endregion

        #region UpdateLayout

        public void UpdateLayout(
            float rankColumnWidth,
            float nameColumnWidth,
            float accColumnWidth,
            float ppColumnWidth,
            float scoreColumnWidth,
            float infoColumnWidth,
            bool ppColumnActive
        ) {
            RankColumnWidth = rankColumnWidth;
            NameColumnWidth = nameColumnWidth;
            AccColumnWidth = accColumnWidth;
            PpColumnWidth = ppColumnWidth;
            ScoreColumnWidth = scoreColumnWidth;
            InfoColumnWidth = infoColumnWidth;
            PpIsActive = ppColumnActive;
        }

        #endregion

        #region Format

        public static readonly NumberFormatInfo ScoreFormatInfo = new CultureInfo("en-US", false).NumberFormat;
        private const string AccColor = "#FFFF32";
        private const string PPColor = "#3277FF";

        static ScoreRow() {
            ScoreFormatInfo.NumberGroupSeparator = " ";
        }

        private static string FormatRank(int value) {
            return $"<i>{value}";
        }

        private static string FormatName(string value) {
            return $"<i>{value}";
        }

        private static string FormatScore(int value) {
            var formattedScore = value.ToString("N0", ScoreFormatInfo);
            return $"<i>{formattedScore}";
        }

        private static string FormatAcc(float value) {
            return $"<i><color={AccColor}>{value * 100.0f:F2}<size=70%>%";
        }

        private static string FormatPP(float value) {
            return $"<i><color={PPColor}>{value:F2}<size=70%>pp";
        }

        #endregion

        #region Background

        private const string HighlightColor = "#FFAC3899";
        private const string FadedColor = "#00000000";

        private string _backgroundColor = "";

        [UIValue("background-color")]
        [UsedImplicitly]
        private string BackgroundColor {
            get => _backgroundColor;
            set {
                if (_backgroundColor.Equals(value)) return;
                _backgroundColor = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Rank

        private string _rankText = "";

        [UIValue("rank-text")]
        [UsedImplicitly]
        public string RankText {
            get => _rankText;
            set {
                if (_rankText.Equals(value)) return;
                _rankText = value;
                NotifyPropertyChanged();
            }
        }

        private float _rankColumnWidth;

        [UIValue("rank-column-width")]
        [UsedImplicitly]
        private float RankColumnWidth {
            get => _rankColumnWidth;
            set {
                if (_rankColumnWidth.Equals(value)) return;
                _rankColumnWidth = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Name

        private string _nameText = "";

        [UIValue("name-text")]
        [UsedImplicitly]
        private string NameText {
            get => _nameText;
            set {
                if (_nameText.Equals(value)) return;
                _nameText = value;
                NotifyPropertyChanged();
            }
        }

        private float _nameColumnWidth;

        [UIValue("name-column-width")]
        [UsedImplicitly]
        private float NameColumnWidth {
            get => _nameColumnWidth;
            set {
                if (_nameColumnWidth.Equals(value)) return;
                _nameColumnWidth = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Acc

        private string _accText = "";

        [UIValue("acc-text")]
        [UsedImplicitly]
        private string AccText {
            get => _accText;
            set {
                if (_accText.Equals(value)) return;
                _accText = value;
                NotifyPropertyChanged();
            }
        }

        private float _accColumnWidth;

        [UIValue("acc-column-width")]
        [UsedImplicitly]
        private float AccColumnWidth {
            get => _accColumnWidth;
            set {
                if (_accColumnWidth.Equals(value)) return;
                _accColumnWidth = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Pp

        private bool _ppIsActive;

        [UIValue("pp-is-active")]
        [UsedImplicitly]
        private bool PpIsActive {
            get => _ppIsActive;
            set {
                if (_ppIsActive.Equals(value)) return;
                _ppIsActive = value;
                NotifyPropertyChanged();
            }
        }

        private string _ppText = "";

        [UIValue("pp-text")]
        [UsedImplicitly]
        private string PpText {
            get => _ppText;
            set {
                if (_ppText.Equals(value)) return;
                _ppText = value;
                NotifyPropertyChanged();
            }
        }

        private float _ppColumnWidth;

        [UIValue("pp-column-width")]
        [UsedImplicitly]
        private float PpColumnWidth {
            get => _ppColumnWidth;
            set {
                if (_ppColumnWidth.Equals(value)) return;
                _ppColumnWidth = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Score

        private string _scoreText = "";

        [UIValue("score-text")]
        [UsedImplicitly]
        private string ScoreText {
            get => _scoreText;
            set {
                if (_scoreText.Equals(value)) return;
                _scoreText = value;
                NotifyPropertyChanged();
            }
        }

        private float _scoreColumnWidth;

        [UIValue("score-column-width")]
        [UsedImplicitly]
        private float ScoreColumnWidth {
            get => _scoreColumnWidth;
            set {
                if (_scoreColumnWidth.Equals(value)) return;
                _scoreColumnWidth = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Info

        [UIAction("info-on-click")]
        [UsedImplicitly]
        private void InfoOnClick() {
            Plugin.Log.Info("Info click");
        }

        private bool _infoIsActive;

        [UIValue("info-is-active")]
        [UsedImplicitly]
        private bool InfoIsActive {
            get => _infoIsActive;
            set {
                if (_infoIsActive.Equals(value)) return;
                _infoIsActive = value;
                NotifyPropertyChanged();
            }
        }

        private float _infoColumnWidth;

        [UIValue("info-column-width")]
        [UsedImplicitly]
        private float InfoColumnWidth {
            get => _infoColumnWidth;
            set {
                if (_infoColumnWidth.Equals(value)) return;
                _infoColumnWidth = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}