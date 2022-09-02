using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    internal class ReplayerSettingsToggle: ReeUIComponentV2 {
        #region Components

        [UIComponent("label-root"), UsedImplicitly] private RectTransform _labelRoot;
        [UIComponent("label-component"), UsedImplicitly] private TextMeshProUGUI _labelComponent;
        [UIComponent("image-component"), UsedImplicitly] private ClickableImage _imageComponent;
        
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

        public void Setup(Sprite sprite, string label, Transform labelRoot) {
            _imageComponent.sprite = sprite;
            _labelComponent.text = label;
            _labelRoot.transform.SetParent(labelRoot, false);
        }

        #endregion

        #region Animation

        private void OnHoverStateChanged(bool isHovered, float progress) {
            var scale = 1.0f + 0.2f * progress;
            _imageComponent.transform.localScale = new Vector3(scale, scale, scale);
            
            _labelComponent.alpha = progress;
            _labelRoot.transform.localScale = new Vector3(1.0f, progress, 1.0f);
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