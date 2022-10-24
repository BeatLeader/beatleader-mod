using System;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;
using BeatLeader.UI.BSML_Addons.Components;
using TMPro;
using UnityEngine.UI;
using BeatLeader.Utils;

namespace BeatLeader.Components {
    internal class NavigationButton : ReeUIComponentV2 {
        #region FlowDirection

        public enum FlowDirection {
            Forward,
            Backward,
            None
        }

        #endregion

        #region Config

        public string Text {
            get => _text.text;
            set => _text.text = value;
        }
        public bool Interactable {
            get => _button.Button.interactable;
            set => _button.Button.interactable = value;
        }
        public bool CanClick {
            get => _button.isActiveAndEnabled;
            set => _button.enabled = value;
        }

        public FlowDirection Flow {
            get => _flow;
            set {
                switch (value) {
                    case FlowDirection.Forward:
                        _contentsGroup.childAlignment = TextAnchor.MiddleLeft;
                        _text.alignment = TextAlignmentOptions.Left;
                        _arrow.Image.sprite = _rightArrowSprite;
                        _arrowGroup.childAlignment = TextAnchor.MiddleRight;
                        _arrowGroup.transform.SetAsLastSibling();
                        _arrowGroup.gameObject.SetActive(true);
                        break;
                    case FlowDirection.Backward:
                        _contentsGroup.childAlignment = TextAnchor.MiddleLeft;
                        _text.alignment = TextAlignmentOptions.Left;
                        _arrow.Image.sprite = _leftArrowSprite;
                        _arrowGroup.childAlignment = TextAnchor.MiddleLeft;
                        _arrowGroup.transform.SetAsFirstSibling();
                        _arrowGroup.gameObject.SetActive(true);
                        break;
                    case FlowDirection.None:
                        _contentsGroup.childAlignment = TextAnchor.MiddleCenter;
                        _text.alignment = TextAlignmentOptions.Center;
                        _arrowGroup.gameObject.SetActive(false);
                        break;
                }
                _flow = value;
            }
        }
        public Color Color {
            get => _button.Button.colors.highlightedColor;
            set {
                var colors = _button.Button.colors;
                colors.highlightedColor = value;
                colors.pressedColor = value.SetAlpha(0.5f);
                _button.Button.colors = colors;
            }
        }

        private FlowDirection _flow = FlowDirection.Forward;

        #endregion

        #region Events

        public event Action OnClick;

        #endregion

        #region UI Components

        [UIComponent("button")]
        private BetterButton _button;

        [UIComponent("text")]
        private TextMeshProUGUI _text;

        [UIComponent("arrow")]
        private BetterImage _arrow;

        [UIComponent("arrow-group")]
        private HorizontalLayoutGroup _arrowGroup;

        [UIComponent("contents-group")]
        private HorizontalLayoutGroup _contentsGroup;

        #endregion

        #region Setup

        private static readonly ColorBlock Colors = new() {
            normalColor = Color.black,
            highlightedColor = Color.cyan,
            pressedColor = new(0, 1, 1, 0.5f),
            selectedColor = Color.white,
            disabledColor = new(170, 170, 170, 0.2f),
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };

        private Sprite _leftArrowSprite = BSMLUtility.LoadSprite("#left-arrow-icon");
        private Sprite _rightArrowSprite = BSMLUtility.LoadSprite("#left-arrow-icon");

        protected override void OnInitialize() {
            _button.Button.colors = Colors;
        }

        #endregion

        #region ContentView

        private ContentView _presentorView;
        private ContentView _contentView;

        public void Setup(ContentView presentor, ContentView content, string name) {
            _presentorView = presentor;
            _contentView = content;
            Text = name;
        }

        private void PresentView() {
            if (_presentorView != null && _contentView != null) {
                _presentorView.PresentView(_contentView);
            }
        }

        #endregion

        #region UI Callbacks

        [UIAction("button-clicked")]
        private void OnButtonClicked() {
            OnClick?.Invoke();
            PresentView();
        }

        #endregion
    }
}
