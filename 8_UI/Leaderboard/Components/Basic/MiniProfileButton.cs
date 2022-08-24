using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    internal class MiniProfileButton : ReeUIComponentV2 {
        #region Events

        public event Action OnClick;

        protected override void OnInitialize() {
            InitializeImage();
            ApplyVisuals();
            UpdateColors();
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


        [UIComponent("label-root"), UsedImplicitly] private RectTransform _labelRoot;
        [UIComponent("label-component"), UsedImplicitly] private TextMeshProUGUI _labelComponent;

        public void Setup(Sprite sprite, bool labelOnLeft) {
            _imageComponent.sprite = sprite;
            _labelOnLeft = labelOnLeft;
            UpdateLabelOffset();
        }

        #endregion

        #region Colors

        private static readonly Color InactiveColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        private static readonly Color DefaultColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        private static readonly Color HighlightColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);

        public void UpdateColors() {
            if (_state is not State.Active) {
                _imageComponent.DefaultColor = InactiveColor;
                _imageComponent.HighlightColor = InactiveColor;
                return;
            }
            
            _imageComponent.DefaultColor = DefaultColor;
            _imageComponent.HighlightColor = HighlightColor;
        }

        #endregion

        #region State

        [UIObject("root"), UsedImplicitly]
        private GameObject _rootObject;

        private State _state = State.Active;

        public void SetState(State state) {
            _state = state;
            _rootObject.SetActive(state is not State.Hidden);
            UpdateColors();
        }

        public enum State {
            Inactive,
            Active,
            Hidden
        }

        #endregion

        #region Label
        
        private const float BaseOffset = 4.0f;
        private bool _labelOnLeft;

        public void SetLabel(string label) {
            _labelComponent.text = label;
            UpdateLabelOffset();
        }

        private void UpdateLabelOffset() {
            var offset = BaseOffset + _labelComponent.preferredWidth / 2;
            _labelRoot.localPosition = new Vector3(_labelOnLeft ? -offset : offset, 0, 0);
        }

        #endregion

        #region Image

        [UIComponent("image-component"), UsedImplicitly] private ClickableImage _imageComponent;

        private void InitializeImage() {
            var hoverController = _imageComponent.gameObject.AddComponent<HoverController>();
            hoverController.HoverStateChangedEvent += OnHoverStateChanged;
            _imageComponent.OnClickEvent += OnClickEvent;
        }

        private void OnClickEvent(PointerEventData _) {
            if (_state is not State.Active) return;
            OnClick?.Invoke();
        }

        #endregion
    }
}