using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;

namespace BeatLeader.Components {
    internal class MapStatus : ReeUIComponentV2 {
        #region Components

        [UIComponent("background"), UsedImplicitly]
        private ImageView _background;

        [UIComponent("status-text"), UsedImplicitly]
        private TextMeshProUGUI _statusText;

        #endregion

        #region Init/Dispose

        private SmoothHoverController _smoothHoverController;

        protected override void OnInitialize() {
            _background.raycastTarget = true;
            _smoothHoverController = _background.gameObject.AddComponent<SmoothHoverController>();
            _smoothHoverController.HoverStateChangedEvent += OnHoverStateChanged;
        }

        protected override void OnDispose() {
            _smoothHoverController.HoverStateChangedEvent -= OnHoverStateChanged;
        }

        private void OnHoverStateChanged(bool isHovered, float progress) {
            MapDifficultyPanel.NotifyMapStatusHoverStateChanged(_background.transform.position, isHovered, progress);
        }

        #endregion

        #region SetValues

        public void SetActive(bool value) {
            _background.gameObject.SetActive(value);
            OnHoverStateChanged(false, 0.0f);
        }

        public void SetValues(RankedStatus rankedStatus, DiffInfo diffInfo) {
            MapDifficultyPanel.NotifyDiffInfoChanged(diffInfo);
            _statusText.text = diffInfo.stars > 0 ? $"{rankedStatus}: {FormatUtils.FormatStars(diffInfo.stars)}" : rankedStatus.ToString();
        }

        #endregion
    }
}