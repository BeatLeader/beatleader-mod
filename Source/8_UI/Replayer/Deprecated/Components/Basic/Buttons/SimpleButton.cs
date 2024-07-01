using BeatLeader.UI.BSML_Addons.Components;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using UnityEngine;

namespace BeatLeader.Components {
    internal class SimpleButton : ReeUIComponentV2 {
        public Sprite Sprite {
            get => _image.Image.sprite;
            set => _image.Image.sprite = value;
        }

        [UIValue("width")]
        public int Width {
            get => _width;
            set {
                _width = value;
                NotifyPropertyChanged(nameof(Width));
            }
        }

        [UIValue("height")]
        public int Height {
            get => _height;
            set {
                _height = value;
                NotifyPropertyChanged(nameof(Height));
            }
        }

        public event Action OnClick;

        public Color highlightedColor = Color.white;
        public Color color = Color.white;

        [UIComponent("image")]
        private readonly BetterImage _image;

        [UIComponent("background")]
        private readonly BetterImage _background;

        private NoTransitionsButton _button;
        private int _width = 6;
        private int _height = 6;

        public void Click() {
            OnClick?.Invoke();
        }

        protected override void OnInitialize() {
            _button = _background.gameObject.AddComponent<NoTransitionsButton>();
            _button.selectionStateDidChangeEvent += HandleStateChanged;
            _button.onClick.AddListener(Click);
        }
        private void HandleStateChanged(NoTransitionsButton.SelectionState state) {
            _image.Image.color = state switch {
                NoTransitionsButton.SelectionState.Highlighted => highlightedColor,
                NoTransitionsButton.SelectionState.Normal => color,
                NoTransitionsButton.SelectionState.Pressed => highlightedColor,
                _ => Color.grey
            };
        }
    }
}
