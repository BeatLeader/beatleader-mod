using System;
using BeatSaberMarkupLanguage;
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

        #region Setup

        [UIValue("button-size")]
        public float Size {
            get => _buttonSize;
            set {
                _buttonSize = value;
                NotifyPropertyChanged();
            }
        }

        private float _buttonSize = 4f;

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
            _imageComponent.OnClickEvent += OnClickEvent;

            SmoothHoverController.Scale(_imageComponent.gameObject, 1.0f, 1.2f);
        }

        private void OnClickEvent(PointerEventData _) {
            OnClick?.Invoke();
            BeatSaberUI.BasicUIAudioManager.HandleButtonClickEvent();
        }

        #endregion
    }
}