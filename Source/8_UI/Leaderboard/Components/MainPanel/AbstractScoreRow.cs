using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BeatLeader.Components {
    internal abstract class AbstractScoreRow : ReeUIComponentV2 {
        #region ColorSchemes

        [StructLayout(LayoutKind.Auto)]
        protected readonly struct ColorScheme {
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

        private static readonly Dictionary<PlayerRole, ColorScheme> ColorSchemes = new Dictionary<PlayerRole, ColorScheme> {
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

        #endregion

        #region Components

        [UIComponent("Underline"), UsedImplicitly]
        private protected ClickableImage _underline;

        [UIComponent("Background"), UsedImplicitly]
        private protected ImageView _background;

        [UIComponent("CellsContainer"), UsedImplicitly]
        private protected RectTransform _animationRoot;

        [UIComponent("CellsContainer"), UsedImplicitly]
        private protected RectTransform _cellsContainer;

        private Material _underlineMaterialInstance;

        protected override void OnInitialize() {
            InitializeCells();

            _background.material = BundleLoader.ScoreBackgroundMaterial;
            _background.sprite = BundleLoader.TransparentPixel;

            _underlineMaterialInstance = Instantiate(BundleLoader.ScoreUnderlineMaterial);
            _underline.material = _underlineMaterialInstance;
        }

        protected override void OnDispose() {
            Destroy(_underlineMaterialInstance);
        }

        private void OnEnable() {
            _state = AnimationState.Fix;
            MarkAnimationDirty();
        }

        protected virtual void Update() {
            UpdateAnimationIfDirty();
            UpdateColorSchemeIfDirty();
            UpdateVisualsIfDirty();
        }

        [UIAction("OnClick"), UsedImplicitly]
        protected virtual void OnClick() { }

        #endregion

        #region Interaction

        public void SetupLayout(ScoresTableLayoutHelper layoutHelper) {
            foreach (var keyValuePair in _cells) {
                layoutHelper.RegisterCell(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public void FadeIn() {
            _targetAlpha = 1.0f;
            _currentOffset = FadeFromOffset;
            _targetOffset = 0.0f;
            _state = AnimationState.Lerp;
            MarkAnimationDirty();
        }

        public void FadeOut() {
            _targetAlpha = 0.0f;
            _targetOffset = FadeToOffset;
            _state = AnimationState.Lerp;
            MarkAnimationDirty();
        }

        public void SetActive(bool value) {
            Content.gameObject.SetActive(value);
        }

        public void SetHierarchyIndex(int value) {
            Content.SetSiblingIndex(value);
        }

        #endregion

        #region Cells

        private readonly Dictionary<ScoreRowCellType, AbstractScoreRowCell> _cells = new Dictionary<ScoreRowCellType, AbstractScoreRowCell>();

        protected T InstantiateCell<T>(ScoreRowCellType cellType) where T : AbstractScoreRowCell {
            var component = Instantiate<T>(_cellsContainer);
            _cells[cellType] = component;
            return component;
        }

        private void InitializeCells() {
            foreach (var cell in _cells.Values) {
                cell.ManualInit(_cellsContainer);
            }
        }

        #endregion

        #region Content

        protected IScoreRowContent? ScoreRowContent { get; private set; }

        public void SetContent(IScoreRowContent score) {
            ScoreRowContent = score;
            UpdateContent();
        }

        public void ClearContent() {
            ScoreRowContent = null;
            UpdateContent();
        }

        protected void UpdateContent() {
            MarkColorSchemeDirty();
            MarkVisualsDirty();

            switch (ScoreRowContent) {
                case Score playerScore: {
                    var player = playerScore.Player;
                    var playerRoles = FormatUtils.ParsePlayerRoles(player.role);

                    SetHighlight(ProfileManager.IsCurrentPlayer(player.id));

                    var supporterRole = FormatUtils.GetSupporterRole(playerRoles);
                    if (!ColorSchemes.TryGetValue(supporterRole, out colorScheme)) colorScheme = ColorSchemes[PlayerRole.Default];
                    break;
                }
                case ClanPlayer clanPlayer: {
                    var player = clanPlayer.Player;
                    var playerRoles = FormatUtils.ParsePlayerRoles(player.role);

                    SetHighlight(ProfileManager.IsCurrentPlayer(player.id));

                    var supporterRole = FormatUtils.GetSupporterRole(playerRoles);
                    if (!ColorSchemes.TryGetValue(supporterRole, out colorScheme)) colorScheme = ColorSchemes[PlayerRole.Default];
                    break;
                }
                case ClanScore clanScore: {
                    SetHighlight(ProfileManager.IsCurrentPlayerTopClan(clanScore.clan));
                    colorScheme = ColorSchemes[PlayerRole.Default];
                    break;
                }
                default: {
                    foreach (var cell in _cells.Values) {
                        cell.MarkEmpty();
                    }

                    return;
                }
            }

            foreach (var pair in _cells) {
                if (ScoreRowContent.ContainsValue(pair.Key)) pair.Value.SetValue(ScoreRowContent.GetValue(pair.Key));
                else pair.Value.MarkEmpty();
            }
        }

        #endregion

        #region Animation

        private enum AnimationState {
            Idle,
            Lerp,
            Fix
        }

        private AnimationState _state = AnimationState.Fix;
        private const float FadeFromOffset = -3.0f;
        private const float FadeToOffset = 5.0f;
        private const float FadeSpeed = 12.0f;
        private float _targetAlpha;
        private float _targetOffset;

        private bool _animationDirty = true;

        public void MarkAnimationDirty() {
            _animationDirty = true;
        }

        private void UpdateAnimationIfDirty() {
            if (!_animationDirty) return;

            switch (_state) {
                case AnimationState.Lerp:
                    _state = ProgressLerpState();
                    break;
                case AnimationState.Fix:
                    _state = ProgressFixState();
                    break;
                case AnimationState.Idle:
                default:
                    _animationDirty = false;
                    break;
            }

            MarkVisualsDirty();
        }

        private AnimationState ProgressLerpState() {
            var t = Time.deltaTime * FadeSpeed;
            _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, t);
            _currentOffset = Mathf.Lerp(_currentOffset, _targetOffset, t);

            var alphaSet = Math.Abs(_currentAlpha - _targetAlpha) < 1e-4f;
            var offsetSet = Math.Abs(_currentOffset - _targetOffset) < 1e-4f;
            return offsetSet && alphaSet ? AnimationState.Fix : AnimationState.Lerp;
        }

        private AnimationState ProgressFixState() {
            _currentAlpha = _targetAlpha;
            _currentOffset = _targetOffset;
            return AnimationState.Idle;
        }

        #endregion

        #region Visuals

        private bool _visualsDirty = true;

        private float _currentAlpha;
        private float _currentOffset;
        private bool _highlighted;
        private Func<IScoreRowContent?, bool>? _customHighlight;

        public void MarkVisualsDirty() {
            _visualsDirty = true;
        }

        private void UpdateVisualsIfDirty() {
            if (!_visualsDirty) return;
            _visualsDirty = false;

            foreach (var cell in _cells.Values) {
                cell.SetAlpha(_currentAlpha);
            }

            _animationRoot.localPosition = new Vector3(_currentOffset, 0.0f, 0.0f);

            _underline.DefaultColor = new Color(colorScheme.IdleHighlight, 0, 0, _currentAlpha);
            _underline.HighlightColor = new Color(colorScheme.HoverHighlight, 0, 0, _currentAlpha);

            var highlight = _customHighlight?.Invoke(ScoreRowContent) ?? _highlighted;
            var bgColor = highlight ? new Color(0.7f, 0f, 0.7f, 0.3f) : new Color(0.07f, 0f, 0.14f, 0.05f);
            bgColor.a *= _currentAlpha;
            _background.color = bgColor;
        }

        private void SetHighlight(bool highlight) {
            _highlighted = highlight;
            MarkVisualsDirty();
        }

        public void SetCustomHighlight(Func<IScoreRowContent?, bool>? func) {
            _customHighlight = func;
            MarkVisualsDirty();
        }

        #endregion

        #region ColorScheme

        private bool _colorSchemeDirty = true;

        protected ColorScheme colorScheme = ColorSchemes[PlayerRole.Default];

        public void MarkColorSchemeDirty() {
            _colorSchemeDirty = true;
        }

        private void UpdateColorSchemeIfDirty() {
            if (!_colorSchemeDirty) return;
            _colorSchemeDirty = false;

            colorScheme.Apply(_underlineMaterialInstance);
        }

        #endregion
    }
}