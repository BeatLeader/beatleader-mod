using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using ModifiersCore;
using System.Collections.Generic;
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

        protected override void OnInitialize() {
            _background.raycastTarget = true;
            SmoothHoverController.Custom(_background.gameObject, OnHoverStateChanged);
            GameplayModifiersPanelPatch.ModifiersChangedEvent += OnModifiersChanged;
        }

        protected override void OnDispose() {
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

            var rating = _diffInfo.modifiersRating;
            if (rating != null) {
                foreach (var modifier in ModifiersManager.Modifiers) {
                    var lowerId = modifier.Id.ToLower();
                    if (rating.ContainsKey(lowerId + "Stars") &&
                        ModifiersManager.GetModifierState(modifier.Id)) {
                        stars = rating[lowerId + "Stars"];
                        modifiersApplied = true;
                        break;
                    }
                }
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