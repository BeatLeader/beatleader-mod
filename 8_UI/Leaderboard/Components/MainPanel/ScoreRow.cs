using System;
using System.Collections.Generic;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using Transform = UnityEngine.Transform;

namespace BeatLeader.Components {
    internal class ScoreRow : ReeUIComponentV2 {
        #region ColorScheme

        private static readonly int IdleColorPropertyId = Shader.PropertyToID("_IdleColor");
        private static readonly int HighlightColorPropertyId = Shader.PropertyToID("_HighlightColor");
        private static readonly int WavesPropertyId = Shader.PropertyToID("_Waves");
        private static readonly int SeedPropertyId = Shader.PropertyToID("_Seed");


        private static readonly ColorScheme DefaultColorScheme = new(
            new Color(0.1f, 0.3f, 0.4f),
            new Color(0.0f, 0.4f, 1.0f),
            0.0f
        );

        private static readonly ColorScheme SupporterColorScheme = new(
            new Color(0.6f, 0.5f, 0.3f),
            new Color(1.0f, 0.7f, 0.0f),
            1.0f
        );
        
        private void ApplyColorScheme(PlayerRole playerRole) {
            var scheme = playerRole switch {
                PlayerRole.Supporter => SupporterColorScheme,
                _ => DefaultColorScheme
            };
            
            _underlineMaterial.SetColor(IdleColorPropertyId, scheme.IdleColor);
            _underlineMaterial.SetColor(HighlightColorPropertyId, scheme.HighlightColor);
            _underlineMaterial.SetFloat(WavesPropertyId, scheme.Waves);
            _underlineMaterial.SetFloat(SeedPropertyId, Random.value);
        }

        private struct ColorScheme {
            public readonly Color IdleColor;
            public readonly Color HighlightColor;
            public readonly float Waves;
            
            public ColorScheme(Color idleColor, Color highlightColor, float waves) {
                IdleColor = idleColor;
                HighlightColor = highlightColor;
                Waves = waves;
            }
        }

        #endregion

        #region Components

        [UIValue("rank-cell"), UsedImplicitly]
        private TextScoreRowCell _rankCell;

        [UIValue("country-cell"), UsedImplicitly]
        private CountryScoreRowCell _countryCell;

        [UIValue("avatar-cell"), UsedImplicitly]
        private AvatarScoreRowCell _avatarCell;

        [UIValue("username-cell"), UsedImplicitly]
        private TextScoreRowCell _usernameCell;

        [UIValue("modifiers-cell"), UsedImplicitly]
        private TextScoreRowCell _modifiersCell;

        [UIValue("accuracy-cell"), UsedImplicitly]
        private TextScoreRowCell _accuracyCell;

        [UIValue("pp-cell"), UsedImplicitly]
        private TextScoreRowCell _ppCell;

        [UIValue("score-cell"), UsedImplicitly]
        private TextScoreRowCell _scoreCell;

        [UIValue("mistakes-cell"), UsedImplicitly]
        private MistakesScoreRowCell _mistakesCell;

        private readonly Dictionary<ScoreRowCellType, AbstractScoreRowCell> _cells = new();

        private void Awake() {
            _cells[ScoreRowCellType.Rank] = _rankCell = Instantiate<TextScoreRowCell>(transform);
            _cells[ScoreRowCellType.Country] = _countryCell = Instantiate<CountryScoreRowCell>(transform);
            _cells[ScoreRowCellType.Avatar] = _avatarCell = Instantiate<AvatarScoreRowCell>(transform);
            _cells[ScoreRowCellType.Username] = _usernameCell = Instantiate<TextScoreRowCell>(transform);
            _cells[ScoreRowCellType.Modifiers] = _modifiersCell = Instantiate<TextScoreRowCell>(transform);
            _cells[ScoreRowCellType.Accuracy] = _accuracyCell = Instantiate<TextScoreRowCell>(transform);
            _cells[ScoreRowCellType.PerformancePoints] = _ppCell = Instantiate<TextScoreRowCell>(transform);
            _cells[ScoreRowCellType.Score] = _scoreCell = Instantiate<TextScoreRowCell>(transform);
            _cells[ScoreRowCellType.Mistakes] = _mistakesCell = Instantiate<MistakesScoreRowCell>(transform);
        }

        #endregion

        #region Setup

        private void SetupFormatting() {
            _rankCell.Setup(o => FormatUtils.FormatRank((int) o, false));
            _usernameCell.Setup(o => FormatUtils.FormatUserName((string) o), TextAlignmentOptions.Left, TextOverflowModes.Ellipsis);
            _modifiersCell.Setup(o => FormatUtils.FormatModifiers((string) o), TextAlignmentOptions.Right, TextOverflowModes.Overflow, 2.4f);
            _accuracyCell.Setup(o => FormatUtils.FormatAcc((float) o));
            _ppCell.Setup(o => FormatUtils.FormatPP((float) o));
            _scoreCell.Setup(o => FormatUtils.FormatScore((int) o), TextAlignmentOptions.Right);
        }

        public void SetupLayout(ScoresTableLayoutHelper layoutHelper) {
            foreach (var (cellType, cell) in _cells) {
                layoutHelper.RegisterCell(cellType, cell);
            }
        }

        #endregion

        #region Events

        protected override void OnInitialize() {
            SetupFormatting();
            SetupBackground();
            SetupUnderline();
            FadeOut();
        }

        private void OnEnable() {
            _currentAlpha = _targetAlpha;
            _currentOffset = _targetOffset;
            _updateRequired = true;
        }

        #endregion

        #region Interaction

        private Score _score;

        public void SetScore(Score score) {
            _score = score;

            var playerRole = FormatUtils.ParseMostSignificantRole(score.player.role);
            ApplyColorScheme(playerRole);

            SetHighlight(score.player.IsCurrentPlayer());
            _rankCell.SetValue(score.rank);
            _countryCell.SetValue(score.player.country);
            _avatarCell.SetValues(score.player.avatar, playerRole);
            _usernameCell.SetValue(score.player.name);
            _modifiersCell.SetValue(score.modifiers);
            _accuracyCell.SetValue(score.accuracy);
            _ppCell.SetValue(score.pp);
            _scoreCell.SetValue(score.modifiedScore);
            _mistakesCell.SetValues(score.missedNotes, score.badCuts, score.bombCuts, score.wallsHit);
            Clickable = true;
        }

        public void ClearScore() {
            _score = null;
            foreach (var cell in _cells.Values) {
                cell.MarkEmpty();
            }

            Clickable = false;
        }

        public void SetActive(bool value) {
            IsActive = value;
        }

        public void SetHierarchyIndex(int value) {
            _rootNode.SetSiblingIndex(value);
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
        private bool _updateRequired;

        public void FadeIn() {
            _targetAlpha = 1.0f;
            _currentOffset = FadeFromOffset;
            _targetOffset = 0.0f;
        }

        public void FadeOut() {
            _targetAlpha = 0.0f;
            _targetOffset = FadeToOffset;
        }

        private void LateUpdate() {
            var t = Time.deltaTime * FadeSpeed;
            if (LerpOffset(t)) _updateRequired = true;
            if (LerpAlpha(t)) _updateRequired = true;
            if (!_updateRequired) return;

            ApplyVisualChanges();
            _updateRequired = false;
        }

        private bool LerpOffset(float t) {
            if (Math.Abs(_currentOffset - _targetOffset) < 1e-6f) return false;
            _currentOffset = Mathf.Lerp(_currentOffset, _targetOffset, t);
            return true;
        }

        private bool LerpAlpha(float t) {
            if (Math.Abs(_currentAlpha - _targetAlpha) < 1e-6f) return false;
            _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, t);
            return true;
        }

        private void ApplyVisualChanges() {
            HorizontalOffset = _currentOffset;

            foreach (var cell in _cells.Values) {
                cell.SetAlpha(_currentAlpha);
            }

            SetBackgroundAlpha(_currentAlpha);
            SetUnderlineAlpha(_currentAlpha);
        }

        #endregion

        #region RootNode

        [UIComponent("root-node"), UsedImplicitly]
        private Transform _rootNode;

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

        #region Background

        private static Color OwnScoreColor => new(0.7f, 0f, 0.7f, 0.3f);
        private static Color SomeoneElseScoreColor => new(0.07f, 0f, 0.14f, 0.05f);

        [UIComponent("background-component"), UsedImplicitly]
        private ImageView _backgroundComponent;

        private bool _highlighted;

        private void SetupBackground() {
            _backgroundComponent.material = BundleLoader.ScoreBackgroundMaterial;
        }

        private void SetHighlight(bool highlight) {
            _highlighted = highlight;
        }

        private void SetBackgroundAlpha(float value) {
            var color = _highlighted ? OwnScoreColor : SomeoneElseScoreColor;
            color.a *= value;
            _backgroundComponent.color = color;
        }

        #endregion

        #region Underline

        private static readonly Color UnderlineIdleColor = new(0.0f, 0.0f, 0.0f);
        private static readonly Color UnderlineHoverColor = new(1.0f, 0.0f, 0.0f);

        [UIComponent("info-component"), UsedImplicitly]
        private ClickableImage _infoComponent;

        private Material _underlineMaterial;

        private void SetupUnderline() {
            _underlineMaterial = Instantiate(BundleLoader.ScoreUnderlineMaterial);
            _infoComponent.material = _underlineMaterial;
            _infoComponent.DefaultColor = UnderlineIdleColor;
            _infoComponent.HighlightColor = UnderlineHoverColor;
        }

        private void SetUnderlineAlpha(float value) {
            _infoComponent.DefaultColor = UnderlineIdleColor.ColorWithAlpha(value);
            _infoComponent.HighlightColor = UnderlineHoverColor.ColorWithAlpha(value);
        }

        #endregion

        #region ClickableArea

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