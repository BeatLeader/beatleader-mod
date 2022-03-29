using System.Globalization;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreRow.bsml")]
    internal class ScoreRow : ReeUIComponent {
        #region Hierarchy

        public void SetActive(bool value) {
            IsActive = value;
        }

        public void SetHierarchyIndex(int value) {
            transform.SetSiblingIndex(value);
        }

        #endregion

        #region SetScore

        private Score _score;

        public void SetScore(Score score, bool highlight) {
            _score = score;
            RankText = FormatRank(score.rank);
            NameText = FormatName(score.player.name);
            AccText = FormatAcc(score.accuracy);
            PpText = FormatPP(score.pp);
            ScoreText = FormatScore(score.baseScore);
            InfoIsActive = true;
            FadeIn(highlight);
        }

        public void ClearScore() {
            _score = null;
            FadeOut();
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

        #region Animation

        private const float FadeFromOffset = -3.0f;
        private const float FadeToOffset = 5.0f;
        private const float FadeSpeed = 12.0f;
        private float _currentAlpha;
        private float _targetAlpha;
        private float _currentOffset;
        private float _targetOffset;
        private Color _backgroundColor;

        private void FadeIn(bool highlight) {
            _backgroundColor = highlight ? _highlightColor : _fadedColor;
            _targetAlpha = 1.0f;
            _currentOffset = FadeFromOffset;
            _targetOffset = 0.0f;
        }

        private void FadeOut() {
            _targetAlpha = 0.0f;
            _targetOffset = FadeToOffset;
        }

        private void LateUpdate() {
            var t = Time.deltaTime * FadeSpeed;
            LerpOffset(t);
            LerpAlpha(t);
        }

        private void LerpOffset(float t) {
            if (_currentOffset.Equals(_targetOffset)) return;
            _currentOffset = Mathf.Lerp(_currentOffset, _targetOffset, t);
            HorizontalOffset = _currentOffset;
        }

        private void LerpAlpha(float t) {
            if (_currentAlpha.Equals(_targetAlpha)) return;
            _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, t);
            ApplyAlpha();
        }

        private void ApplyAlpha() {
            _rankTmp.alpha = _currentAlpha;
            _nameTmp.alpha = _currentAlpha;
            _accTmp.alpha = _currentAlpha;
            _ppTmp.alpha = _currentAlpha;
            _scoreTmp.alpha = _currentAlpha;
            _infoTmp.alpha = _currentAlpha;

            _backgroundImage.color = new Color(
                _backgroundColor.r,
                _backgroundColor.g,
                _backgroundColor.b,
                _backgroundColor.a * _currentAlpha
            );
        }

        #endregion

        #region OffsetNode

        private float _horizontalOffset;

        [UIValue("horizontal-offset"), UsedImplicitly]
        private float HorizontalOffset {
            get => _horizontalOffset;
            set {
                if (_horizontalOffset.Equals(value)) return;
                _horizontalOffset = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region IsActive

        private bool _isActive = true;

        [UIValue("is-active"), UsedImplicitly]
        private bool IsActive {
            get => _isActive;
            set {
                if (_isActive.Equals(value)) return;
                _isActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Background

        private readonly Color _highlightColor = new Color(0f, 0.95f, 1f, 0.4f);
        private readonly Color _fadedColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        [UIComponent("bg-image"), UsedImplicitly]
        private Image _backgroundImage;

        #endregion

        #region Rank

        [UIComponent("rank-tmp"), UsedImplicitly]
        private TextMeshProUGUI _rankTmp;

        private string _rankText = "";

        [UIValue("rank-text"), UsedImplicitly]
        public string RankText {
            get => _rankText;
            set {
                if (_rankText.Equals(value)) return;
                _rankText = value;
                NotifyPropertyChanged();
            }
        }

        private float _rankColumnWidth;

        [UIValue("rank-column-width"), UsedImplicitly]
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

        [UIComponent("name-tmp"), UsedImplicitly]
        private TextMeshProUGUI _nameTmp;

        private string _nameText = "";

        [UIValue("name-text"), UsedImplicitly]
        private string NameText {
            get => _nameText;
            set {
                if (_nameText.Equals(value)) return;
                _nameText = value;
                NotifyPropertyChanged();
            }
        }

        private float _nameColumnWidth;

        [UIValue("name-column-width"), UsedImplicitly]
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

        [UIComponent("acc-tmp"), UsedImplicitly]
        private TextMeshProUGUI _accTmp;

        private string _accText = "";

        [UIValue("acc-text"), UsedImplicitly]
        private string AccText {
            get => _accText;
            set {
                if (_accText.Equals(value)) return;
                _accText = value;
                NotifyPropertyChanged();
            }
        }

        private float _accColumnWidth;

        [UIValue("acc-column-width"), UsedImplicitly]
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

        [UIComponent("pp-tmp"), UsedImplicitly]
        private TextMeshProUGUI _ppTmp;

        private bool _ppIsActive;

        [UIValue("pp-is-active"), UsedImplicitly]
        private bool PpIsActive {
            get => _ppIsActive;
            set {
                if (_ppIsActive.Equals(value)) return;
                _ppIsActive = value;
                NotifyPropertyChanged();
            }
        }

        private string _ppText = "";

        [UIValue("pp-text"), UsedImplicitly]
        private string PpText {
            get => _ppText;
            set {
                if (_ppText.Equals(value)) return;
                _ppText = value;
                NotifyPropertyChanged();
            }
        }

        private float _ppColumnWidth;

        [UIValue("pp-column-width"), UsedImplicitly]
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

        [UIComponent("score-tmp"), UsedImplicitly]
        private TextMeshProUGUI _scoreTmp;

        private string _scoreText = "";

        [UIValue("score-text"), UsedImplicitly]
        private string ScoreText {
            get => _scoreText;
            set {
                if (_scoreText.Equals(value)) return;
                _scoreText = value;
                NotifyPropertyChanged();
            }
        }

        private float _scoreColumnWidth;

        [UIValue("score-column-width"), UsedImplicitly]
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

        [UIComponent("info-tmp"), UsedImplicitly]
        private TextMeshProUGUI _infoTmp;

        [UIAction("info-on-click"), UsedImplicitly]
        private void InfoOnClick() {
            if (_score == null) return;
            LeaderboardEvents.NotifyScoreInfoButtonWasPressed(_score);
        }

        private bool _infoIsActive;

        [UIValue("info-is-active"), UsedImplicitly]
        private bool InfoIsActive {
            get => _infoIsActive;
            set {
                if (_infoIsActive.Equals(value)) return;
                _infoIsActive = value;
                NotifyPropertyChanged();
            }
        }

        private float _infoColumnWidth;

        [UIValue("info-column-width"), UsedImplicitly]
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