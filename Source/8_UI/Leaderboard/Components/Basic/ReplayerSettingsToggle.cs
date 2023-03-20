using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    internal class ReplayerSettingsToggle : ReeUIComponentV2 {
        #region Components

        [UIComponent("image-component"), UsedImplicitly]
        private ClickableImage _imageComponent;

        #endregion

        #region Value

        public event Action<bool> OnClick;

        private bool _value;

        public bool Value {
            get => _value;
            set {
                if (_value == value) return;
                _value = value;
                UpdateColors();
            }
        }

        private void OnClickEvent(PointerEventData _) {
            Value = !Value;
            OnClick?.Invoke(Value);
        }

        #endregion

        #region Initialize

        protected override void OnInitialize() {
            InitializeImage();
            UpdateColors();
        }

        private void InitializeImage() {
            _imageComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            var hoverController = _imageComponent.gameObject.AddComponent<SmoothHoverController>();
            hoverController.HoverStateChangedEvent += OnHoverStateChanged;
            _imageComponent.OnClickEvent += OnClickEvent;
        }

        #endregion

        #region Setup

        private string _hintText;
        private HintField.HintHandler _hintHandler;

        public void Setup(Sprite sprite, string hintText, HintField hintField) {
            _imageComponent.sprite = sprite;
            _hintText = hintText;
            _hintHandler = hintField.RegisterHandler();
        }

        #endregion

        #region Animation

        private void OnHoverStateChanged(bool isHovered, float progress) {
            var scale = 1.0f + 0.2f * progress;
            _imageComponent.transform.localScale = new Vector3(scale, scale, scale);

            if (isHovered) {
                _hintHandler?.ShowHint(_hintText);
            } else {
                _hintHandler?.HideHint();
            }
        }

        #endregion

        #region Colors

        private static Color SelectedColor => new(0.0f, 0.4f, 1.0f, 1.0f);
        private static readonly Color FadedColor = new(0.8f, 0.8f, 0.8f, 0.2f);

        private Color _glowColor = SelectedColor;

        public void SetGlowColor(Color color) {
            _glowColor = color;
            UpdateColors();
        }

        private void UpdateColors() {
            var col = Value ? _glowColor : FadedColor;
            _imageComponent.DefaultColor = col;
            _imageComponent.HighlightColor = col;
        }

        #endregion
    }
}