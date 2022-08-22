using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class MiniProfileButton : ReeUIComponentV2 {
        #region Events

        public event Action OnClick;

        protected override void OnInitialize() {
            InitializeImage();
            ApplyVisuals();
        }

        private void OnHoverStateChanged(bool isHovered) {
            _targetT = isHovered ? 1 : 0;
        }

        #endregion

        #region Animation

        private float _currentT;
        private float _targetT;

        private void Update() {
            if (Mathf.Abs(_currentT - _targetT) < 1e-10) return;
            _currentT = Mathf.Lerp(_currentT, _targetT, Time.deltaTime * 10);
            ApplyVisuals();
        }

        private void ApplyVisuals() {
            _labelRoot.localScale = new Vector3(1.0f, _currentT, 1.0f);
            _labelComponent.alpha = _currentT;
        }

        #endregion

        #region Setup

        private const float BaseOffset = 4.0f;

        [UIComponent("label-root"), UsedImplicitly] private RectTransform _labelRoot;
        [UIComponent("label-component"), UsedImplicitly] private TextMeshProUGUI _labelComponent;

        public void Setup(Sprite sprite, string label, bool labelOnLeft) {
            _imageComponent.sprite = sprite;
            _labelComponent.text = label;
            var offset = BaseOffset + _labelComponent.preferredWidth / 2;
            _labelRoot.localPosition = new Vector3(labelOnLeft ? -offset : offset, 0, 0);
        }

        #endregion

        #region Image

        [UIComponent("image-component"), UsedImplicitly] private ClickableImage _imageComponent;

        private void InitializeImage() {
            var hoverController = _imageComponent.gameObject.AddComponent<HoverController>();
            hoverController.HoverStateChangedEvent += OnHoverStateChanged;
            _imageComponent.OnClickEvent += _ => OnClick?.Invoke();
        }

        #endregion
    }
}