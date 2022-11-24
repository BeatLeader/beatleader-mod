using BeatLeader.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [RequireComponent(typeof(RectTransform))]
    internal class GlassWrapper : MonoBehaviour, IPointerDownHandler {
        public static readonly Color DefaultColor = new(0, 1, 1);
        public static readonly Color SelectedColor = new(1, 1, 0);

        public const float EnabledOpacity = 0.7f;
        public const float DisabledOpacity = 0.4f;

        public RectTransform container;

        public Color BgColor {
            get => _background?.color ?? _bgColor;
            set {
                _bgColor = value.SetAlpha(_bgFixedOpacity != -1 ? _bgFixedOpacity : value.a);
                if (_background != null) {
                    _background.color = _bgColor;
                }
            }
        }
        public Color TextColor {
            get => _text?.color ?? _textColor;
            set {
                _textColor = value;
                if (_text != null) {
                    _text.color = value;
                }
            }
        }
        public float BgFixedOpacity {
            get => _bgFixedOpacity;
            set {
                _bgFixedOpacity = value;
                BgColor = BgColor;
            }
        }
        public string ComponentName {
            get => _text?.text ?? _componentName;
            set {
                _componentName = value;
                if (_text != null) {
                    _text.text = value;
                }
            }
        }
        public bool State {
            get => _wrapperState;
            set {
                if (value)
                    Wrap();
                else
                    Unwrap();
            }
        }

        public event Action<bool> WrapperStateChangedEvent;
        public event Action WrapperSelectedEvent;

        private Color _bgColor = DefaultColor;
        private Color _textColor = Color.black;
        private string _componentName;
        private float _bgFixedOpacity = -1;
        private bool _wrapperState;

        private bool _wrapperConstructed;
        private RectTransform _wrapperRect;
        private TextMeshProUGUI _text;
        private Image _background;

        private void Awake() {
            _wrapperRect = GetComponent<RectTransform>();
        }
        private void CreateWrapperIfNeeded() {
            if (_wrapperConstructed) return;
            _background = gameObject.AddComponent<Image>();
            _background.sprite = BundleLoader.WhiteBG;
            _background.color = _bgColor;
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

        public void Wrap() {
            if (container == null) return;
            CreateWrapperIfNeeded();
            _wrapperRect.sizeDelta = container.rect.size;
            _wrapperRect.localPosition = Vector2.zero;
            _wrapperRect.localScale = Vector2.one;
            _wrapperRect.gameObject.SetActive(true);
            _wrapperRect.SetParent(container, false);
            WrapperStateChangedEvent?.Invoke(true);
            _wrapperState = true;
        }
        public void Unwrap() {
            if (!State) return;
            _wrapperRect.gameObject.SetActive(false);
            _wrapperRect.SetParent(null);
            WrapperStateChangedEvent?.Invoke(false);
            _wrapperState = false;
        }

        public void OnPointerDown(PointerEventData eventData) {
            WrapperSelectedEvent?.Invoke();
        }
    }
}
