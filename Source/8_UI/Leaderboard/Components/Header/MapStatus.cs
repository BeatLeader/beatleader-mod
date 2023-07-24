using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;

namespace BeatLeader.Components {
    internal class MapStatus : ReeUIComponentV2 {
        #region Components

        [UIComponent("background"), UsedImplicitly]
        private ImageView _background = null!;

        [UIComponent("status-text"), UsedImplicitly]
        private TextMeshProUGUI _statusText = null!;

        #endregion

        #region Init/Dispose

        private SmoothHoverController _smoothHoverController = null!;

        protected override void OnInitialize() {
            _background.raycastTarget = true;
            _smoothHoverController = _background.gameObject.AddComponent<SmoothHoverController>();
            _smoothHoverController.HoverStateChangedEvent += OnHoverStateChanged;
            GameplayModifiersPanelPatch.ModifiersChangedEvent += OnModifiersChanged;
        }

        protected override void OnDispose() {
            _smoothHoverController.HoverStateChangedEvent -= OnHoverStateChanged;
            GameplayModifiersPanelPatch.ModifiersChangedEvent -= OnModifiersChanged;
        }
        
        #endregion

        #region SetValues

        private GameplayModifiers? _gameplayModifiers;
        private RankedStatus _rankedStatus;
        private DiffInfo _diffInfo;
        
        public void SetActive(bool value) {
            _background.gameObject.SetActive(value);
            OnHoverStateChanged(false, 0.0f);
        }

        public void SetValues(RankedStatus rankedStatus, DiffInfo diffInfo) {
            _rankedStatus = rankedStatus;
            _diffInfo = diffInfo;
            UpdateVisuals();
        }

        private void UpdateVisuals() {
            MapDifficultyPanel.NotifyDiffInfoChanged(_diffInfo);
            var stars = _diffInfo.stars;
            var modifiersApplied = false;
            if (_diffInfo.modifiersRating is { } rating &&
                _gameplayModifiers is { songSpeed: not GameplayModifiers.SongSpeed.Normal } modifiers) {
                stars = modifiers.songSpeed switch {
                    GameplayModifiers.SongSpeed.Slower => rating.ssStars,
                    GameplayModifiers.SongSpeed.Faster => rating.fsStars,
                    GameplayModifiers.SongSpeed.SuperFast => rating.sfStars,
                    _ => stars,
                };
                modifiersApplied = true;
            }
            var text = _rankedStatus.ToString();
            var modifiersIndicator = modifiersApplied ? "<color=green>[M]</color>" : string.Empty;
            if (_diffInfo.stars > 0) text += $": {FormatUtils.FormatStars(stars)} {modifiersIndicator}";
            _statusText.text = text;
        }

        #endregion

        #region Callbacks
        
        private void OnHoverStateChanged(bool isHovered, float progress) {
            MapDifficultyPanel.NotifyMapStatusHoverStateChanged(_background.transform.position, isHovered, progress);
        }

        private void OnModifiersChanged(GameplayModifiers modifiers) {
            _gameplayModifiers = modifiers;
            UpdateVisuals();
        }

        #endregion
    }
}