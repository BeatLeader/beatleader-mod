using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [RequireComponent(typeof(RectTransform))]
    internal class GlassWrapper : MonoBehaviour, IPointerDownHandler {
        public bool IsEnabled => _wrapperState;
        public string ComponentName {
            get => _text?.text ?? _componentName;
            set {
                _componentName = value;
                if (_text != null) {
                    _text.text = value;
                }
            }
        }
        public Color Color {
            get => _background?.color ?? _color;
            set {
                _color = value;
                if (_background != null) {
                    _background.color = _color;
                }
            }
        }
        public Color TextColor {
            get => _text?.color ?? _textColor;
            set {
                _textColor = value;
                if (_text != null) {
                    _text.color = _textColor;
                }
            }
        }

        public event Action<bool>? WrapperStateChangedEvent;
        public event Action? WrapperWasSelectedEvent;

        public RectTransform? container;

        private Color _color = Color.clear;
        private Color _textColor = Color.black;
        private string _componentName = string.Empty;
        private bool _wrapperState;

        private bool _wrapperConstructed;
        private RectTransform _wrapperRect = null!;
        private TextMeshProUGUI _text = null!;
        private Image _background = null!;

        public void SetEnabled(bool enabled = true) {
            if (enabled) {
                if (container == null || _wrapperState) return;
                CreateWrapperIfNeeded();
                _wrapperRect.sizeDelta = container.rect.size;
                _wrapperRect.localPosition = Vector2.zero;
                _wrapperRect.localScale = Vector2.one;
                _wrapperRect.gameObject.SetActive(true);
                _wrapperRect.SetParent(container, false);
                WrapperStateChangedEvent?.Invoke(true);
                _wrapperState = true;
            } else if (_wrapperState) {
                _wrapperRect.gameObject.SetActive(false);
                _wrapperRect.SetParent(null);
                WrapperStateChangedEvent?.Invoke(false);
                _wrapperState = false;
            }
        }
        private void CreateWrapperIfNeeded() {
            if (_wrapperConstructed) return;
            _background = gameObject.AddComponent<Image>();
            _background.sprite = BundleLoader.WhiteBG;
            _background.color = _color;
            _background.type = Image.Type.Sliced;
            _background.pixelsPerUnitMultiplier = 5;

            gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

            var horizontal = gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontal.childForceExpandHeight = true;
            horizontal.childForceExpandWidth = true;
            horizontal.childControlWidth = true;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(transform, false);
            _text = textGo.AddComponent<TextMeshProUGUI>();

            _text.fontSize = 6;
            _text.color = _textColor;
            _text.text = _componentName;
            _text.alignment = TextAlignmentOptions.Midline;
            _text.overflowMode = TextOverflowModes.Ellipsis;
            _wrapperConstructed = true;
        }

        private void Awake() {
            _wrapperRect = GetComponent<RectTransform>();
        }
        public void OnPointerDown(PointerEventData eventData) {
            WrapperWasSelectedEvent?.Invoke();
        }
    }
}
