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
            UpdateColors();
        }

        #endregion

        #region Animation

        private void OnHoverStateChanged(bool isHovered, float progress) {
            var scale = _state is State.NonInteractable ? 0.8f : 1.0f + 0.5f * progress;
            _imageComponent.transform.localScale = new Vector3(scale, scale, scale);

            var maxAlpha = _state is State.NonInteractable ? 0.5f : 1.0f;
            _labelComponent.alpha = maxAlpha * progress;
            _labelRoot.localScale = new Vector3(1.0f, progress, 1.0f);
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

        private static readonly Color InactiveColor = new(0.4f, 0.4f, 0.4f, 0.2f);
        private static readonly Color SelectedColor = new(0.0f, 0.4f, 1.0f, 1.0f);
        private static readonly Color FadedColor = new(0.8f, 0.8f, 0.8f, 0.2f);

        private Color _glowColor = SelectedColor;

        public void SetGlowColor(Color color) {
            _glowColor = color;
            UpdateColors();
        }

        private void UpdateColors() {
            switch (_state) {
                case State.NonInteractable:
                    _imageComponent.DefaultColor = InactiveColor;
                    _imageComponent.HighlightColor = InactiveColor;
                    break;
                case State.InteractableFaded: 
                    _imageComponent.DefaultColor = FadedColor;
                    _imageComponent.HighlightColor = FadedColor;
                    break;
                case State.InteractableGlowing:
                    _imageComponent.DefaultColor = _glowColor;
                    _imageComponent.HighlightColor = _glowColor;
                    break;
            }
        }

        #endregion

        #region State

        [UIObject("root"), UsedImplicitly]
        private GameObject _rootObject;

        private State _state = State.InteractableFaded;

        public void SetState(State state) {
            _state = state;
            _rootObject.SetActive(state is not State.Hidden);
            UpdateColors();
        }

        public enum State {
            NonInteractable,
            InteractableFaded,
            InteractableGlowing,
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
            _imageComponent.material = BundleLoader.UIAdditiveGlowMaterial;
            var hoverController = _imageComponent.gameObject.AddComponent<SmoothHoverController>();
            hoverController.HoverStateChangedEvent += OnHoverStateChanged;
            _imageComponent.OnClickEvent += OnClickEvent;
        }

        private void OnClickEvent(PointerEventData _) {
            if (_state is State.NonInteractable or State.Hidden) return;
            OnClick?.Invoke();
        }

        #endregion
    }
}