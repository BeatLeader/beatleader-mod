using System;
using System.Collections.Generic;
using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.Models;
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

        private static readonly Dictionary<PlayerRole, ColorScheme> ColorSchemes = new() {
            {
                PlayerRole.Default, new ColorScheme(
                    new Color(0.4f, 0.6f, 1.0f),
                    new Color(0.5f, 0.7f, 1.0f),
                    0.0f,
                    0.0f,
                    1.0f
                )
            }, {
                PlayerRole.Tipper, new ColorScheme(
                    new Color(1.0f, 1.0f, 0.7f),
                    new Color(1.0f, 0.6f, 0.0f),
                    0.0f,
                    0.0f,
                    1.0f
                )
            }, {
                PlayerRole.Supporter, new ColorScheme(
                    new Color(1.0f, 1.0f, 0.7f),
                    new Color(1.0f, 0.6f, 0.0f),
                    1.0f,
                    0.0f,
                    1.0f
                )
            }, {
                PlayerRole.Sponsor, new ColorScheme(
                    new Color(1.0f, 1.0f, 0.6f),
                    new Color(1.0f, 0.3f, 0.0f),
                    1.0f,
                    0.8f,
                    1.0f
                )
            }
        };

        private void ApplyColorScheme(PlayerRole[] playerRoles) {
            var supporterRole = FormatUtils.GetSupporterRole(playerRoles);
            var scheme = ColorSchemes.ContainsKey(supporterRole) ? ColorSchemes[supporterRole] : ColorSchemes[PlayerRole.Default];
            SetUnderlineHighlight(scheme.IdleHighlight, scheme.HoverHighlight);
            scheme.Apply(_underlineMaterial);
        }

        private readonly struct ColorScheme {
            private static readonly int RimColorPropertyId = Shader.PropertyToID("_RimColor");
            private static readonly int HaloColorPropertyId = Shader.PropertyToID("_HaloColor");
            private static readonly int WavesAmplitudePropertyId = Shader.PropertyToID("_WavesAmplitude");
            private static readonly int SeedPropertyId = Shader.PropertyToID("_Seed");

            private readonly Color _rimColor;
            private readonly Color _haloColor;
            private readonly float _wavesAmplitude;
            public readonly float IdleHighlight;
            public readonly float HoverHighlight;

            public ColorScheme(Color rimColor, Color haloColor, float wavesAmplitude, float idleHighlight, float hoverHighlight) {
                _rimColor = rimColor;
                _haloColor = haloColor;
                _wavesAmplitude = wavesAmplitude;
                IdleHighlight = idleHighlight;
                HoverHighlight = hoverHighlight;
            }

            public void Apply(Material material) {
                material.SetColor(RimColorPropertyId, _rimColor);
                material.SetColor(HaloColorPropertyId, _haloColor);
                material.SetFloat(WavesAmplitudePropertyId, _wavesAmplitude);
                material.SetFloat(SeedPropertyId, Random.value);
            }
        }

        #endregion

        #region Components

        [UIValue("rank-cell"), UsedImplicitly]
        private TextScoreRowCell _rankCell;

        [UIValue("avatar-cell"), UsedImplicitly]
        private AvatarScoreRowCell _avatarCell;

        [UIValue("country-cell"), UsedImplicitly]
        private CountryScoreRowCell _countryCell;

        [UIValue("username-cell"), UsedImplicitly]
        private TextScoreRowCell _usernameCell;

        [UIValue("clans-cell"), UsedImplicitly]
        private ClansScoreRowCell _clansCell;

        [UIValue("modifiers-cell"), UsedImplicitly]
        private TextScoreRowCell _modifiersCell;

        [UIValue("accuracy-cell"), UsedImplicitly]
        private TextScoreRowCell _accuracyCell;

        [UIValue("pp-cell"), UsedImplicitly]
        private TextScoreRowCell _ppCell;

        [UIValue("score-cell"), UsedImplicitly]
        private TextScoreRowCell _scoreCell;

        [UIValue("time-cell"), UsedImplicitly]
        private TextScoreRowCell _timeCell;

        [UIValue("mistakes-cell"), UsedImplicitly]
        private MistakesScoreRowCell _mistakesCell;

        [UIValue("pauses-cell"), UsedImplicitly]
        private TextScoreRowCell _pausesCell;

        private readonly Dictionary<ScoreRowCellType, AbstractScoreRowCell> _cells = new();

        private void Awake() {
            _cells[ScoreRowCellType.Rank] = _rankCell = Instantiate<TextScoreRowCell>(transform);
            _cells[ScoreRowCellType.Avatar] = _avatarCell = Instantiate<AvatarScoreRowCell>(transform);
            _cells[ScoreRowCellType.Country] = _countryCell = Instantiate<CountryScoreRowCell>(transform);
            _cells[ScoreRowCellType.Username] = _usernameCell = Instantiate<TextScoreRowCell>(transform);
            _cells[ScoreRowCellType.Clans] = _clansCell = Instantiate<ClansScoreRowCell>(transform);
            _cells[ScoreRowCellType.Modifiers] = _modifiersCell = Instantiate<TextScoreRowCell>(transform);
            _cells[ScoreRowCellType.Accuracy] = _accuracyCell = Instantiate<TextScoreRowCell>(transform);
            _cells[ScoreRowCellType.PerformancePoints] = _ppCell = Instantiate<TextScoreRowCell>(transform);
            _cells[ScoreRowCellType.Score] = _scoreCell = Instantiate<TextScoreRowCell>(transform);
            _cells[ScoreRowCellType.Time] = _timeCell = Instantiate<TextScoreRowCell>(transform);
            _cells[ScoreRowCellType.Mistakes] = _mistakesCell = Instantiate<MistakesScoreRowCell>(transform);
            _cells[ScoreRowCellType.Pauses] = _pausesCell = Instantiate<TextScoreRowCell>(transform);
        }

        #endregion

        #region Setup

        private void SetupFormatting() {
            _rankCell.Setup(o => FormatUtils.FormatRank((int) o, false));
            _usernameCell.Setup(o => FormatUtils.FormatUserName((string) o), TextAlignmentOptions.Left, TextOverflowModes.Ellipsis, 3.4f, false);
            _modifiersCell.Setup(o => FormatUtils.FormatModifiers((string) o), TextAlignmentOptions.Right, TextOverflowModes.Overflow, 2.4f);
            _accuracyCell.Setup(o => FormatUtils.FormatAcc((float) o));
            _ppCell.Setup(o => FormatUtils.FormatPP((float) o));
            _scoreCell.Setup(o => FormatUtils.FormatScore((int) o), TextAlignmentOptions.Right);
            _timeCell.Setup(o => FormatUtils.FormatTimeset((string) o), TextAlignmentOptions.Center, TextOverflowModes.Overflow, 2.4f);
            _pausesCell.Setup(o => FormatUtils.FormatPauses((int) o));
        }

        public void SetupLayout(ScoresTableLayoutHelper layoutHelper) {
            foreach (var keyValuePair in _cells) {
                layoutHelper.RegisterCell(keyValuePair.Key, keyValuePair.Value);
            }
        }

        #endregion

        #region Events

        protected override void OnInitialize() {
            HiddenPlayersCache.HiddenPlayersUpdatedEvent += UpdatePlayer;
            
            SetupFormatting();
            SetupBackground();
            SetupUnderline();
            FadeOut();
        }

        protected override void OnDispose() {
            HiddenPlayersCache.HiddenPlayersUpdatedEvent -= UpdatePlayer;
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

            _rankCell.SetValue(score.rank);
            _modifiersCell.SetValue(score.modifiers);
            _accuracyCell.SetValue(score.accuracy);
            _ppCell.SetValue(score.pp);
            _scoreCell.SetValue(score.modifiedScore);
            _timeCell.SetValue(score.timeSet);
            _mistakesCell.SetValues(score.missedNotes, score.badCuts, score.bombCuts, score.wallsHit);
            _pausesCell.SetValue(score.pauses);
            Clickable = true;

            UpdatePlayer();
        }

        private void UpdatePlayer() {
            if (_score == null) return;
            var player = HiddenPlayersCache.ModifyPlayer(_score.player);
            
            SetHighlight(ProfileManager.IsCurrentPlayer(player));
            
            var playerRoles = FormatUtils.ParsePlayerRoles(player.role);
            ApplyColorScheme(playerRoles);

            _avatarCell.SetValues(player.avatar, playerRoles);
            _countryCell.SetValue(player.country);
            _usernameCell.SetValue(player.name);
            _clansCell.SetValues(player.clans ?? Array.Empty<Clan>());
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

        [UIComponent("info-component"), UsedImplicitly]
        private ClickableImage _infoComponent;

        private Material _underlineMaterial;
        private float _underlineAlpha = 1.0f;
        private float _idleHighlight;
        private float _hoverHighlight = 1.0f;

        private void SetupUnderline() {
            _underlineMaterial = Instantiate(BundleLoader.ScoreUnderlineMaterial);
            _infoComponent.material = _underlineMaterial;
            UpdateUnderlineColors();
        }

        private void SetUnderlineAlpha(float value) {
            _underlineAlpha = value;
            UpdateUnderlineColors();
        }

        private void SetUnderlineHighlight(float idle, float hover) {
            _idleHighlight = idle;
            _hoverHighlight = hover;
            UpdateUnderlineColors();
        }

        private void UpdateUnderlineColors() {
            _infoComponent.DefaultColor = new Color(_idleHighlight, 0, 0, _underlineAlpha);
            _infoComponent.HighlightColor = new Color(_hoverHighlight, 0, 0, _underlineAlpha);
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