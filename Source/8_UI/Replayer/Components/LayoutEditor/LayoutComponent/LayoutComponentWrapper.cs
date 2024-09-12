using System;
using System.Collections.Generic;
using BeatLeader.Utils;
using Reactive.Components;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal interface ILayoutComponentWrapperHandler {
        void OnComponentPointerClick();

        void OnComponentPointerUp();

        void OnComponentPointerDown();

        void OnCornerPointerUp(Vector2 corner);

        void OnCornerPointerDown(Vector2 corner);
    }

    [RequireComponent(typeof(RectTransform))]
    internal class LayoutComponentWrapper : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        ILayoutComponentWrapperController {
        #region Events

        public event Action<bool>? StateChangedEvent;

        #endregion

        #region Setup

        private ILayoutComponentWrapperHandler? _handler;
        private string? _componentName;

        private RectTransform _rectTransform = null!;
        private bool _isGenerated;

        public void Setup(ILayoutComponentWrapperHandler handler, string componentName) {
            _handler = handler;
            _componentName = componentName;
        }

        private void Awake() {
            _rectTransform = gameObject.GetComponent<RectTransform>();
            _rectTransform.sizeDelta = Vector2.zero;
            _rectTransform.anchorMin = Vector2.zero;
            _rectTransform.anchorMax = Vector2.one;
        }

        private void CreateWrapperIfNeeded() {
            if (_isGenerated) return;
            CreateBackground();
            CreateCorners();
            CreateText();
            RefreshWrapperColor();
            _isGenerated = true;
        }

        #endregion

        #region Wrapper

        private bool _isWrapperComponentSelected;
        private bool _isWrapperComponentEnabled;

        void ILayoutComponentWrapperController.SetWrapperActive(bool active) {
            SetComponentSelected(false);
            SetActive(active);
        }

        void ILayoutComponentWrapperController.SetWrapperSelected(bool selected) => SetComponentSelected(selected);

        public void SetActive(bool active) {
            CreateWrapperIfNeeded();
            gameObject.SetActive(active);
            StateChangedEvent?.Invoke(active);
        }

        public void SetComponentSelected(bool selected) {
            CreateWrapperIfNeeded();
            _isWrapperComponentSelected = selected;
            RefreshWrapperColor();
        }

        public void SetComponentActive(bool active) {
            CreateWrapperIfNeeded();
            _isWrapperComponentEnabled = active;
            RefreshWrapperColor();
        }

        private void RefreshWrapperColor() {
            var color = _isWrapperComponentSelected ? selectedColor : baseColor;
            _backgroundImage!.color = color.ColorWithAlpha(_isWrapperComponentEnabled ? enabledAlpha : disabledAlpha);
        }

        #endregion

        #region Corners

        private static readonly Vector2[] scalingCorners = {
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(1, 1)
        };

        private static readonly Sprite cornerSprite = BundleLoader.WhiteBG;
        private static readonly Vector2 cornerSize = new(5, 5);

        private readonly Dictionary<PointerEventsHandler, Vector2> _corners = new();

        private void CreateCorners() {
            foreach (var corner in scalingCorners) CreateCorner(corner);
        }

        private void CreateCorner(Vector2 corner) {
            //creation
            var go = gameObject.CreateChild($"Corner({corner.x}, {corner.y})");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = cornerSize;
            //pointer handler
            var handler = go.AddComponent<PointerEventsHandler>();
            handler.PointerDownEvent += HandlePointerDown;
            handler.PointerUpEvent += HandlePointerUp;
            //image
            var img = go.AddComponent<Image>();
            img.material = GameResources.UINoGlowMaterial;
            img.sprite = cornerSprite;
            img.color = frameColor;
            img.pixelsPerUnitMultiplier = 3;
            //placing
            PlaceCorner(rect, corner, cornerSize / 2);
            _corners.Add(handler, corner);
        }

        private static void PlaceCorner(RectTransform trans, Vector2 corner, Vector2 offset) {
            trans.pivot = corner;
            var offsetMultiplier = Vector2.one;
            offsetMultiplier.x *= corner.x is 0 ? -1 : 1;
            offsetMultiplier.y *= corner.y is 0 ? -1 : 1;
            trans.localPosition = offset * offsetMultiplier / 2;
            trans.anchorMin = corner;
            trans.anchorMax = corner;
        }

        #endregion

        #region Background

        private static readonly Color baseColor = Color.cyan;
        private static readonly Color selectedColor = Color.yellow;
        private static readonly Color frameColor = Color.cyan.ColorWithG(0.7f);

        private static readonly float enabledAlpha = 0.8f;
        private static readonly float disabledAlpha = 0.6f;

        private static readonly Sprite backgroundSprite = BundleLoader.WhiteBG;
        private static readonly Sprite frameSprite = BundleLoader.WhiteFrame;

        private Image? _backgroundImage;

        private void CreateBackground() {
            var go = gameObject;
            _backgroundImage = CreateBackgroundImage(go, backgroundSprite);
            _backgroundImage.color = baseColor;

            var frameGo = gameObject.CreateChild("Frame");
            var frameTransform = frameGo.AddComponent<RectTransform>();
            frameTransform.anchorMin = Vector2.zero;
            frameTransform.anchorMax = Vector2.one;
            frameTransform.sizeDelta = Vector2.zero;

            var frameImage = CreateBackgroundImage(frameGo, frameSprite);
            frameImage.color = frameColor;

            static Image CreateBackgroundImage(GameObject gameObject, Sprite sprite) {
                var image = gameObject.AddComponent<Image>();
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
                image.material = GameResources.UINoGlowMaterial;
                image.pixelsPerUnitMultiplier = 3.5f;
                return image;
            }
        }

        #endregion

        #region Text

        private void CreateText() {
            var textGo = gameObject.CreateChild("ComponentNameText");
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = _componentName;
            text.fontSize = 6f;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Callbacks

        private void HandlePointerDown(PointerEventsHandler handler, PointerEventData data) {
            _handler?.OnCornerPointerDown(_corners[handler]);
        }

        private void HandlePointerUp(PointerEventsHandler handler, PointerEventData data) {
            _handler?.OnCornerPointerUp(_corners[handler]);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            _handler?.OnComponentPointerDown();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
            _handler?.OnComponentPointerUp();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
            _handler?.OnComponentPointerClick();
        }

        #endregion
    }
}