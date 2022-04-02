using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreRow.bsml")]
    internal class ScoreRow : ReeUIComponent {
        #region OnInitialize

        protected override void OnInitialize() {
            SetMaterials();
            ApplyAlpha();
        }

        public void SetHierarchyIndex(int value) {
            Root.SetSiblingIndex(value);
        }

        #endregion

        #region SetScore

        private Score _score;

        public void SetScore(Score score) {
            SetHighlight(score.player.IsCurrentPlayer());

            _score = score;
            RankText = FormatUtils.FormatRank(score.rank, false);
            NameText = FormatUtils.FormatUserName(score.player.name);
            ModifiersText = FormatUtils.FormatModifiers(score.modifiers);
            AccText = FormatUtils.FormatAcc(score.accuracy);
            PpText = FormatUtils.FormatPP(score.pp);
            ScoreText = FormatUtils.FormatScore(score.baseScore);
            Clickable = true;
            
            UpdateFlexibleColumns(score.modifiers);
            FadeIn();
        }

        public void ClearScore() {
            _score = null;
            Clickable = false;
            FadeOut();
        }

        #endregion

        #region UpdateLayout

        private float _flexibleWidth;

        private void UpdateFlexibleColumns(string modifiersString) {
            TableLayoutUtils.CalculateFlexibleWidths(_flexibleWidth, modifiersString,
                out var nameColumnWidth,
                out var modifiersColumnWidth
            );

            NameColumnWidth = nameColumnWidth;
            ModifiersColumnWidth = modifiersColumnWidth;
        }

        public void UpdateLayout(
            float rankColumnWidth,
            float accColumnWidth,
            float ppColumnWidth,
            float scoreColumnWidth,
            float flexibleWidth,
            bool ppColumnActive
        ) {
            RankColumnWidth = rankColumnWidth;
            AccColumnWidth = accColumnWidth;
            PpColumnWidth = ppColumnWidth;
            ScoreColumnWidth = scoreColumnWidth;
            _flexibleWidth = flexibleWidth;
            PpIsActive = ppColumnActive;
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

        private void FadeIn() {
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
            _modifiersTmp.alpha = _currentAlpha;
            _accTmp.alpha = _currentAlpha;
            _ppTmp.alpha = _currentAlpha;
            _scoreTmp.alpha = _currentAlpha;

            _backgroundComponent.color = _backgroundColor.ColorWithAlpha(_backgroundColor.a * _currentAlpha);
            _infoComponent.color = _infoComponent.color.ColorWithAlpha(_infoComponent.color.a * _currentAlpha);
        }

        #endregion

        #region HorizontalOffset

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

        #region Modifiers

        [UIComponent("modifiers-tmp"), UsedImplicitly]
        private TextMeshProUGUI _modifiersTmp;

        private string _modifiersText = "";

        [UIValue("modifiers-text"), UsedImplicitly]
        private string ModifiersText {
            get => _modifiersText;
            set {
                if (_modifiersText.Equals(value)) return;
                _modifiersText = value;
                NotifyPropertyChanged();
            }
        }

        private float _modifiersColumnWidth;

        [UIValue("modifiers-column-width"), UsedImplicitly]
        private float ModifiersColumnWidth {
            get => _modifiersColumnWidth;
            set {
                if (_modifiersColumnWidth.Equals(value)) return;
                _modifiersColumnWidth = value;
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

        #region ClickableArea

        private readonly Color _ownScoreColor = new Color(0.7f, 0f, 0.7f, 0.3f);
        private readonly Color _someoneElseScoreColor = new Color(0.07f, 0f, 0.14f, 0.05f);

        private readonly Color _underlineIdleColor = new Color(0.1f, 0.3f, 0.4f, 0.0f);
        private readonly Color _underlineHoverColor = new Color(0.0f, 0.4f, 1.0f, 1.0f);

        private Color _backgroundColor;

        private void SetHighlight(bool highlight) {
            _backgroundColor = highlight ? _ownScoreColor : _someoneElseScoreColor;
        }

        private void SetMaterials() {
            _backgroundComponent.material = BundleLoader.ScoreBackgroundMaterial;
            _infoComponent.material = BundleLoader.ScoreUnderlineMaterial;
            _infoComponent.DefaultColor = _underlineIdleColor;
            _infoComponent.HighlightColor = _underlineHoverColor;
        }

        [UIComponent("background-component"), UsedImplicitly]
        private ImageView _backgroundComponent;

        [UIComponent("info-component"), UsedImplicitly]
        private ClickableImage _infoComponent;

        [UIAction("info-on-click"), UsedImplicitly]
        private void InfoOnClick() {
            if (_score == null) return;
            LeaderboardEvents.NotifyScoreInfoButtonWasPressed(_score);
        }

        private bool _clickable;

        [UIValue("clickable"), UsedImplicitly]
        private bool Clickable {
            get => _clickable;
            set {
                if (_clickable.Equals(value)) return;
                _clickable = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}