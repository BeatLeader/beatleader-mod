using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    internal class HeaderButton : ReeUIComponentV2 {
        #region Events

        public event Action OnClick;

        protected override void OnInitialize() {
            InitializeImage();
            UpdateColor(0.0f);
        }

        #endregion

        #region Animation

        private void OnHoverStateChanged(bool isHovered, float progress) {
            var scale = 1.0f + 0.2f * progress;
            _imageComponent.transform.localScale = new Vector3(scale, scale, scale);
            UpdateColor(progress);
        }

        #endregion

        #region Setup

        public void Setup(Sprite sprite) {
            _imageComponent.sprite = sprite;
        }

        #endregion

        #region Colors

        private static readonly Color SelectedColor = new(0.0f, 0.4f, 1.0f, 1.0f);
        private static readonly Color FadedColor = new(0.8f, 0.8f, 0.8f, 0.2f);

        private void UpdateColor(float hoverProgress) {
            var color = Color.Lerp(FadedColor, SelectedColor, hoverProgress);
            _imageComponent.DefaultColor = color;
            _imageComponent.HighlightColor = color;
        }

        #endregion

        #region Image

        [UIComponent("image-component"), UsedImplicitly]
        private ClickableImage _imageComponent;

        private void InitializeImage() {
            _imageComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            var hoverController = _imageComponent.gameObject.AddComponent<SmoothHoverController>();
            hoverController.HoverStateChangedEvent += OnHoverStateChanged;
            _imageComponent.OnClickEvent += OnClickEvent;
        }

        private void OnClickEvent(PointerEventData _) {
            OnClick?.Invoke();
        }

        #endregion
    }
}