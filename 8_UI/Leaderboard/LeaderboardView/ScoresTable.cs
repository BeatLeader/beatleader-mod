using System.Globalization;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader {
    internal partial class LeaderboardView {
        #region Format

        private static readonly NumberFormatInfo ScoreFormatInfo = new CultureInfo("en-US", false).NumberFormat;
        private const string AccColor = "#FFFF00";
        private const string PPColor = "#0A5BFF";

        static LeaderboardView() {
            ScoreFormatInfo.NumberGroupSeparator = " ";
        }

        private static string FormatRank(int value) {
            return $"<i>{value}";
        }

        private static string FormatUserName(string value) {
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

        #region TableLayout

        #region Constants

        private const float TotalWidth = 85.0f;
        private const float Spacing = 1.0f;
        private const int ColumnsCount = 6;

        private const float RankMinWidth = 3.0f;
        private const float AccMinWidth = 11.0f;
        private const float PPMinWidth = 11.0f;
        private const float ScoreMinWidth = 10.0f;
        private const float InfoMinWidth = 5.0f;

        private const float ApproxCharacterWidth = 1.1f;

        #endregion

        #region UpdateTableLayout

        private void UpdateTableLayout(int maximalRank, int maximalScore, bool hasPP) {
            PPColumnActive = hasPP;
            RankColumnWidth = CalculateRankWidth(maximalRank);
            AccColumnWidth = AccMinWidth;
            PpColumnWidth = PPMinWidth;
            ScoreColumnWidth = CalculateScoreWidth(maximalScore);
            InfoColumnWidth = InfoMinWidth;
            NickNameColumnWidth = CalculateNickNameWidth(hasPP);
        }

        private static float CalculateRankWidth(int maximalRank) {
            var charCount = maximalRank.ToString().Length;
            return Mathf.Max(RankMinWidth, ApproxCharacterWidth * charCount);
        }

        private static float CalculateScoreWidth(int maximalScore) {
            var charCount = maximalScore.ToString("N", ScoreFormatInfo).Length;
            return Mathf.Max(ScoreMinWidth, ApproxCharacterWidth * charCount);
        }

        private float CalculateNickNameWidth(bool hasPP) {
            var result = TotalWidth;

            if (hasPP) {
                result -= PpColumnWidth;
                result -= Spacing * (ColumnsCount - 1);
            } else {
                result -= Spacing * (ColumnsCount - 2);
            }

            result -= RankColumnWidth;
            result -= ScoreColumnWidth;
            result -= AccColumnWidth;
            result -= InfoColumnWidth;

            return result;
        }

        #endregion

        #region RankColumnWidth

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

        #region NickNameColumnWidth

        private float _nickNameColumnWidth;

        [UIValue("nickname-column-width")]
        [UsedImplicitly]
        private float NickNameColumnWidth {
            get => _nickNameColumnWidth;
            set {
                if (_nickNameColumnWidth.Equals(value)) return;
                _nickNameColumnWidth = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region AccColumnWidth

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

        #region PpColumnWidth

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

        #region ScoreColumnWidth

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

        #region InfoColumnWidth

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

        #region PPColumnActive

        private bool _ppColumnActive;

        [UIValue("pp-column-active")]
        [UsedImplicitly]
        private bool PPColumnActive {
            get => _ppColumnActive;
            set {
                if (_ppColumnActive.Equals(value)) return;
                _ppColumnActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #endregion

        #region SetScore

        private void SetScore(int rowIndex, Score score) {
            SetRowValues(
                rowIndex,
                FormatRank(score.rank),
                FormatUserName(score.player.name),
                FormatAcc(score.accuracy),
                FormatPP(score.pp),
                FormatScore(score.baseScore),
                true
            );
        }

        private void ClearScore(int rowIndex) {
            SetRowValues(
                rowIndex,
                "",
                "",
                "",
                "",
                "",
                false
            );
        }

        #endregion

        #region SetRowValues

        private void SetRowValues(
            int rowIndex,
            string rankText,
            string nickNameText,
            string accText,
            string ppText,
            string scoreText,
            bool infoButtonActive
        ) {
            switch (rowIndex) {
                case 0:
                    Row0RankText = rankText;
                    Row0NickNameText = nickNameText;
                    Row0ScoreText = scoreText;
                    Row0AccText = accText;
                    Row0PpText = ppText;
                    Row0InfoButtonActive = infoButtonActive;
                    break;
                case 1:
                    Row1RankText = rankText;
                    Row1NickNameText = nickNameText;
                    Row1ScoreText = scoreText;
                    Row1AccText = accText;
                    Row1PpText = ppText;
                    Row1InfoButtonActive = infoButtonActive;
                    break;
                case 2:
                    Row2RankText = rankText;
                    Row2NickNameText = nickNameText;
                    Row2ScoreText = scoreText;
                    Row2AccText = accText;
                    Row2PpText = ppText;
                    Row2InfoButtonActive = infoButtonActive;
                    break;
                case 3:
                    Row3RankText = rankText;
                    Row3NickNameText = nickNameText;
                    Row3ScoreText = scoreText;
                    Row3AccText = accText;
                    Row3PpText = ppText;
                    Row3InfoButtonActive = infoButtonActive;
                    break;
                case 4:
                    Row4RankText = rankText;
                    Row4NickNameText = nickNameText;
                    Row4ScoreText = scoreText;
                    Row4AccText = accText;
                    Row4PpText = ppText;
                    Row4InfoButtonActive = infoButtonActive;
                    break;
                case 5:
                    Row5RankText = rankText;
                    Row5NickNameText = nickNameText;
                    Row5ScoreText = scoreText;
                    Row5AccText = accText;
                    Row5PpText = ppText;
                    Row5InfoButtonActive = infoButtonActive;
                    break;
                case 6:
                    Row6RankText = rankText;
                    Row6NickNameText = nickNameText;
                    Row6ScoreText = scoreText;
                    Row6AccText = accText;
                    Row6PpText = ppText;
                    Row6InfoButtonActive = infoButtonActive;
                    break;
                case 7:
                    Row7RankText = rankText;
                    Row7NickNameText = nickNameText;
                    Row7ScoreText = scoreText;
                    Row7AccText = accText;
                    Row7PpText = ppText;
                    Row7InfoButtonActive = infoButtonActive;
                    break;
                case 8:
                    Row8RankText = rankText;
                    Row8NickNameText = nickNameText;
                    Row8ScoreText = scoreText;
                    Row8AccText = accText;
                    Row8PpText = ppText;
                    Row8InfoButtonActive = infoButtonActive;
                    break;
                case 9:
                    Row9RankText = rankText;
                    Row9NickNameText = nickNameText;
                    Row9ScoreText = scoreText;
                    Row9AccText = accText;
                    Row9PpText = ppText;
                    Row9InfoButtonActive = infoButtonActive;
                    break;
            }
        }

        #endregion

        #region Row0

        #region RankText

        private string _row0RankText = "";

        [UIValue("row0-rank-text")]
        [UsedImplicitly]
        private string Row0RankText {
            get => _row0RankText;
            set {
                if (_row0RankText.Equals(value)) return;
                _row0RankText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region NickNameText

        private string _row0NickNameText = "";


        [UIValue("row0-nickname-text")]
        [UsedImplicitly]
        private string Row0NickNameText {
            get => _row0NickNameText;
            set {
                if (_row0NickNameText.Equals(value)) return;
                _row0NickNameText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region AccText

        private string _row0AccText = "";


        [UIValue("row0-acc-text")]
        [UsedImplicitly]
        private string Row0AccText {
            get => _row0AccText;
            set {
                if (_row0AccText.Equals(value)) return;
                _row0AccText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region PpText

        private string _row0PpText = "";


        [UIValue("row0-pp-text")]
        [UsedImplicitly]
        private string Row0PpText {
            get => _row0PpText;
            set {
                if (_row0PpText.Equals(value)) return;
                _row0PpText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ScoreText

        private string _row0ScoreText = "";


        [UIValue("row0-score-text")]
        [UsedImplicitly]
        private string Row0ScoreText {
            get => _row0ScoreText;
            set {
                if (_row0ScoreText.Equals(value)) return;
                _row0ScoreText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonActive

        private bool _row0InfoButtonActive;

        [UIValue("row0-info-button-active")]
        [UsedImplicitly]
        private bool Row0InfoButtonActive {
            get => _row0InfoButtonActive;
            set {
                if (_row0InfoButtonActive.Equals(value)) return;
                _row0InfoButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonOnClick

        [UIAction("row0-info-button-on-click")]
        [UsedImplicitly]
        private void Row0InfoButtonOnClick() {
            OnInfoButtonClicked(0);
        }

        #endregion

        #endregion

        #region Row1

        #region RankText

        private string _row1RankText = "";

        [UIValue("row1-rank-text")]
        [UsedImplicitly]
        private string Row1RankText {
            get => _row1RankText;
            set {
                if (_row1RankText.Equals(value)) return;
                _row1RankText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region NickNameText

        private string _row1NickNameText = "";


        [UIValue("row1-nickname-text")]
        [UsedImplicitly]
        private string Row1NickNameText {
            get => _row1NickNameText;
            set {
                if (_row1NickNameText.Equals(value)) return;
                _row1NickNameText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region AccText

        private string _row1AccText = "";


        [UIValue("row1-acc-text")]
        [UsedImplicitly]
        private string Row1AccText {
            get => _row1AccText;
            set {
                if (_row1AccText.Equals(value)) return;
                _row1AccText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region PpText

        private string _row1PpText = "";


        [UIValue("row1-pp-text")]
        [UsedImplicitly]
        private string Row1PpText {
            get => _row1PpText;
            set {
                if (_row1PpText.Equals(value)) return;
                _row1PpText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ScoreText

        private string _row1ScoreText = "";


        [UIValue("row1-score-text")]
        [UsedImplicitly]
        private string Row1ScoreText {
            get => _row1ScoreText;
            set {
                if (_row1ScoreText.Equals(value)) return;
                _row1ScoreText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonActive

        private bool _row1InfoButtonActive;

        [UIValue("row1-info-button-active")]
        [UsedImplicitly]
        private bool Row1InfoButtonActive {
            get => _row1InfoButtonActive;
            set {
                if (_row1InfoButtonActive.Equals(value)) return;
                _row1InfoButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonOnClick

        [UIAction("row1-info-button-on-click")]
        [UsedImplicitly]
        private void Row1InfoButtonOnClick() {
            OnInfoButtonClicked(1);
        }

        #endregion

        #endregion

        #region Row2

        #region RankText

        private string _row2RankText = "";

        [UIValue("row2-rank-text")]
        [UsedImplicitly]
        private string Row2RankText {
            get => _row2RankText;
            set {
                if (_row2RankText.Equals(value)) return;
                _row2RankText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region NickNameText

        private string _row2NickNameText = "";


        [UIValue("row2-nickname-text")]
        [UsedImplicitly]
        private string Row2NickNameText {
            get => _row2NickNameText;
            set {
                if (_row2NickNameText.Equals(value)) return;
                _row2NickNameText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region AccText

        private string _row2AccText = "";


        [UIValue("row2-acc-text")]
        [UsedImplicitly]
        private string Row2AccText {
            get => _row2AccText;
            set {
                if (_row2AccText.Equals(value)) return;
                _row2AccText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region PpText

        private string _row2PpText = "";


        [UIValue("row2-pp-text")]
        [UsedImplicitly]
        private string Row2PpText {
            get => _row2PpText;
            set {
                if (_row2PpText.Equals(value)) return;
                _row2PpText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ScoreText

        private string _row2ScoreText = "";


        [UIValue("row2-score-text")]
        [UsedImplicitly]
        private string Row2ScoreText {
            get => _row2ScoreText;
            set {
                if (_row2ScoreText.Equals(value)) return;
                _row2ScoreText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonActive

        private bool _row2InfoButtonActive;

        [UIValue("row2-info-button-active")]
        [UsedImplicitly]
        private bool Row2InfoButtonActive {
            get => _row2InfoButtonActive;
            set {
                if (_row2InfoButtonActive.Equals(value)) return;
                _row2InfoButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonOnClick

        [UIAction("row2-info-button-on-click")]
        [UsedImplicitly]
        private void Row2InfoButtonOnClick() {
            OnInfoButtonClicked(2);
        }

        #endregion

        #endregion

        #region Row3

        #region RankText

        private string _row3RankText = "";

        [UIValue("row3-rank-text")]
        [UsedImplicitly]
        private string Row3RankText {
            get => _row3RankText;
            set {
                if (_row3RankText.Equals(value)) return;
                _row3RankText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region NickNameText

        private string _row3NickNameText = "";


        [UIValue("row3-nickname-text")]
        [UsedImplicitly]
        private string Row3NickNameText {
            get => _row3NickNameText;
            set {
                if (_row3NickNameText.Equals(value)) return;
                _row3NickNameText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region AccText

        private string _row3AccText = "";


        [UIValue("row3-acc-text")]
        [UsedImplicitly]
        private string Row3AccText {
            get => _row3AccText;
            set {
                if (_row3AccText.Equals(value)) return;
                _row3AccText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region PpText

        private string _row3PpText = "";


        [UIValue("row3-pp-text")]
        [UsedImplicitly]
        private string Row3PpText {
            get => _row3PpText;
            set {
                if (_row3PpText.Equals(value)) return;
                _row3PpText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ScoreText

        private string _row3ScoreText = "";


        [UIValue("row3-score-text")]
        [UsedImplicitly]
        private string Row3ScoreText {
            get => _row3ScoreText;
            set {
                if (_row3ScoreText.Equals(value)) return;
                _row3ScoreText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonActive

        private bool _row3InfoButtonActive;

        [UIValue("row3-info-button-active")]
        [UsedImplicitly]
        private bool Row3InfoButtonActive {
            get => _row3InfoButtonActive;
            set {
                if (_row3InfoButtonActive.Equals(value)) return;
                _row3InfoButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonOnClick

        [UIAction("row3-info-button-on-click")]
        [UsedImplicitly]
        private void Row3InfoButtonOnClick() {
            OnInfoButtonClicked(3);
        }

        #endregion

        #endregion

        #region Row4

        #region RankText

        private string _row4RankText = "";

        [UIValue("row4-rank-text")]
        [UsedImplicitly]
        private string Row4RankText {
            get => _row4RankText;
            set {
                if (_row4RankText.Equals(value)) return;
                _row4RankText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region NickNameText

        private string _row4NickNameText = "";


        [UIValue("row4-nickname-text")]
        [UsedImplicitly]
        private string Row4NickNameText {
            get => _row4NickNameText;
            set {
                if (_row4NickNameText.Equals(value)) return;
                _row4NickNameText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region AccText

        private string _row4AccText = "";


        [UIValue("row4-acc-text")]
        [UsedImplicitly]
        private string Row4AccText {
            get => _row4AccText;
            set {
                if (_row4AccText.Equals(value)) return;
                _row4AccText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region PpText

        private string _row4PpText = "";


        [UIValue("row4-pp-text")]
        [UsedImplicitly]
        private string Row4PpText {
            get => _row4PpText;
            set {
                if (_row4PpText.Equals(value)) return;
                _row4PpText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ScoreText

        private string _row4ScoreText = "";


        [UIValue("row4-score-text")]
        [UsedImplicitly]
        private string Row4ScoreText {
            get => _row4ScoreText;
            set {
                if (_row4ScoreText.Equals(value)) return;
                _row4ScoreText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonActive

        private bool _row4InfoButtonActive;

        [UIValue("row4-info-button-active")]
        [UsedImplicitly]
        private bool Row4InfoButtonActive {
            get => _row4InfoButtonActive;
            set {
                if (_row4InfoButtonActive.Equals(value)) return;
                _row4InfoButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonOnClick

        [UIAction("row4-info-button-on-click")]
        [UsedImplicitly]
        private void Row4InfoButtonOnClick() {
            OnInfoButtonClicked(4);
        }

        #endregion

        #endregion

        #region Row5

        #region RankText

        private string _row5RankText = "";

        [UIValue("row5-rank-text")]
        [UsedImplicitly]
        private string Row5RankText {
            get => _row5RankText;
            set {
                if (_row5RankText.Equals(value)) return;
                _row5RankText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region NickNameText

        private string _row5NickNameText = "";


        [UIValue("row5-nickname-text")]
        [UsedImplicitly]
        private string Row5NickNameText {
            get => _row5NickNameText;
            set {
                if (_row5NickNameText.Equals(value)) return;
                _row5NickNameText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region AccText

        private string _row5AccText = "";


        [UIValue("row5-acc-text")]
        [UsedImplicitly]
        private string Row5AccText {
            get => _row5AccText;
            set {
                if (_row5AccText.Equals(value)) return;
                _row5AccText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region PpText

        private string _row5PpText = "";


        [UIValue("row5-pp-text")]
        [UsedImplicitly]
        private string Row5PpText {
            get => _row5PpText;
            set {
                if (_row5PpText.Equals(value)) return;
                _row5PpText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ScoreText

        private string _row5ScoreText = "";


        [UIValue("row5-score-text")]
        [UsedImplicitly]
        private string Row5ScoreText {
            get => _row5ScoreText;
            set {
                if (_row5ScoreText.Equals(value)) return;
                _row5ScoreText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonActive

        private bool _row5InfoButtonActive;

        [UIValue("row5-info-button-active")]
        [UsedImplicitly]
        private bool Row5InfoButtonActive {
            get => _row5InfoButtonActive;
            set {
                if (_row5InfoButtonActive.Equals(value)) return;
                _row5InfoButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonOnClick

        [UIAction("row5-info-button-on-click")]
        [UsedImplicitly]
        private void Row5InfoButtonOnClick() {
            OnInfoButtonClicked(5);
        }

        #endregion

        #endregion

        #region Row6

        #region RankText

        private string _row6RankText = "";

        [UIValue("row6-rank-text")]
        [UsedImplicitly]
        private string Row6RankText {
            get => _row6RankText;
            set {
                if (_row6RankText.Equals(value)) return;
                _row6RankText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region NickNameText

        private string _row6NickNameText = "";


        [UIValue("row6-nickname-text")]
        [UsedImplicitly]
        private string Row6NickNameText {
            get => _row6NickNameText;
            set {
                if (_row6NickNameText.Equals(value)) return;
                _row6NickNameText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region AccText

        private string _row6AccText = "";


        [UIValue("row6-acc-text")]
        [UsedImplicitly]
        private string Row6AccText {
            get => _row6AccText;
            set {
                if (_row6AccText.Equals(value)) return;
                _row6AccText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region PpText

        private string _row6PpText = "";


        [UIValue("row6-pp-text")]
        [UsedImplicitly]
        private string Row6PpText {
            get => _row6PpText;
            set {
                if (_row6PpText.Equals(value)) return;
                _row6PpText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ScoreText

        private string _row6ScoreText = "";


        [UIValue("row6-score-text")]
        [UsedImplicitly]
        private string Row6ScoreText {
            get => _row6ScoreText;
            set {
                if (_row6ScoreText.Equals(value)) return;
                _row6ScoreText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonActive

        private bool _row6InfoButtonActive;

        [UIValue("row6-info-button-active")]
        [UsedImplicitly]
        private bool Row6InfoButtonActive {
            get => _row6InfoButtonActive;
            set {
                if (_row6InfoButtonActive.Equals(value)) return;
                _row6InfoButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonOnClick

        [UIAction("row6-info-button-on-click")]
        [UsedImplicitly]
        private void Row6InfoButtonOnClick() {
            OnInfoButtonClicked(6);
        }

        #endregion

        #endregion

        #region Row7

        #region RankText

        private string _row7RankText = "";

        [UIValue("row7-rank-text")]
        [UsedImplicitly]
        private string Row7RankText {
            get => _row7RankText;
            set {
                if (_row7RankText.Equals(value)) return;
                _row7RankText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region NickNameText

        private string _row7NickNameText = "";


        [UIValue("row7-nickname-text")]
        [UsedImplicitly]
        private string Row7NickNameText {
            get => _row7NickNameText;
            set {
                if (_row7NickNameText.Equals(value)) return;
                _row7NickNameText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region AccText

        private string _row7AccText = "";


        [UIValue("row7-acc-text")]
        [UsedImplicitly]
        private string Row7AccText {
            get => _row7AccText;
            set {
                if (_row7AccText.Equals(value)) return;
                _row7AccText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region PpText

        private string _row7PpText = "";


        [UIValue("row7-pp-text")]
        [UsedImplicitly]
        private string Row7PpText {
            get => _row7PpText;
            set {
                if (_row7PpText.Equals(value)) return;
                _row7PpText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ScoreText

        private string _row7ScoreText = "";


        [UIValue("row7-score-text")]
        [UsedImplicitly]
        private string Row7ScoreText {
            get => _row7ScoreText;
            set {
                if (_row7ScoreText.Equals(value)) return;
                _row7ScoreText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonActive

        private bool _row7InfoButtonActive;

        [UIValue("row7-info-button-active")]
        [UsedImplicitly]
        private bool Row7InfoButtonActive {
            get => _row7InfoButtonActive;
            set {
                if (_row7InfoButtonActive.Equals(value)) return;
                _row7InfoButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonOnClick

        [UIAction("row7-info-button-on-click")]
        [UsedImplicitly]
        private void Row7InfoButtonOnClick() {
            OnInfoButtonClicked(7);
        }

        #endregion

        #endregion

        #region Row8

        #region RankText

        private string _row8RankText = "";

        [UIValue("row8-rank-text")]
        [UsedImplicitly]
        private string Row8RankText {
            get => _row8RankText;
            set {
                if (_row8RankText.Equals(value)) return;
                _row8RankText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region NickNameText

        private string _row8NickNameText = "";


        [UIValue("row8-nickname-text")]
        [UsedImplicitly]
        private string Row8NickNameText {
            get => _row8NickNameText;
            set {
                if (_row8NickNameText.Equals(value)) return;
                _row8NickNameText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region AccText

        private string _row8AccText = "";


        [UIValue("row8-acc-text")]
        [UsedImplicitly]
        private string Row8AccText {
            get => _row8AccText;
            set {
                if (_row8AccText.Equals(value)) return;
                _row8AccText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region PpText

        private string _row8PpText = "";


        [UIValue("row8-pp-text")]
        [UsedImplicitly]
        private string Row8PpText {
            get => _row8PpText;
            set {
                if (_row8PpText.Equals(value)) return;
                _row8PpText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ScoreText

        private string _row8ScoreText = "";


        [UIValue("row8-score-text")]
        [UsedImplicitly]
        private string Row8ScoreText {
            get => _row8ScoreText;
            set {
                if (_row8ScoreText.Equals(value)) return;
                _row8ScoreText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonActive

        private bool _row8InfoButtonActive;

        [UIValue("row8-info-button-active")]
        [UsedImplicitly]
        private bool Row8InfoButtonActive {
            get => _row8InfoButtonActive;
            set {
                if (_row8InfoButtonActive.Equals(value)) return;
                _row8InfoButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonOnClick

        [UIAction("row8-info-button-on-click")]
        [UsedImplicitly]
        private void Row8InfoButtonOnClick() {
            OnInfoButtonClicked(8);
        }

        #endregion

        #endregion

        #region Row9

        #region RankText

        private string _row9RankText = "";

        [UIValue("row9-rank-text")]
        [UsedImplicitly]
        private string Row9RankText {
            get => _row9RankText;
            set {
                if (_row9RankText.Equals(value)) return;
                _row9RankText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region NickNameText

        private string _row9NickNameText = "";


        [UIValue("row9-nickname-text")]
        [UsedImplicitly]
        private string Row9NickNameText {
            get => _row9NickNameText;
            set {
                if (_row9NickNameText.Equals(value)) return;
                _row9NickNameText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region AccText

        private string _row9AccText = "";


        [UIValue("row9-acc-text")]
        [UsedImplicitly]
        private string Row9AccText {
            get => _row9AccText;
            set {
                if (_row9AccText.Equals(value)) return;
                _row9AccText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region PpText

        private string _row9PpText = "";


        [UIValue("row9-pp-text")]
        [UsedImplicitly]
        private string Row9PpText {
            get => _row9PpText;
            set {
                if (_row9PpText.Equals(value)) return;
                _row9PpText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ScoreText

        private string _row9ScoreText = "";


        [UIValue("row9-score-text")]
        [UsedImplicitly]
        private string Row9ScoreText {
            get => _row9ScoreText;
            set {
                if (_row9ScoreText.Equals(value)) return;
                _row9ScoreText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonActive

        private bool _row9InfoButtonActive;

        [UIValue("row9-info-button-active")]
        [UsedImplicitly]
        private bool Row9InfoButtonActive {
            get => _row9InfoButtonActive;
            set {
                if (_row9InfoButtonActive.Equals(value)) return;
                _row9InfoButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region InfoButtonOnClick

        [UIAction("row9-info-button-on-click")]
        [UsedImplicitly]
        private void Row9InfoButtonOnClick() {
            OnInfoButtonClicked(9);
        }

        #endregion

        #endregion
    }
}