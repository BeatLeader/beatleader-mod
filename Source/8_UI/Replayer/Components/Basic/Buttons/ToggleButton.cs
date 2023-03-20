using BeatLeader.UI.BSML_Addons.Components;
using BeatSaberMarkupLanguage.Attributes;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Basic.SimpleButton.bsml")]
    internal class ToggleButton : ReeUIComponentV2 {
        public Sprite EnabledSprite {
            get => _enabledSprite ?? DisabledSprite;
            set => _enabledSprite = value;
        }
        public Sprite DisabledSprite {
            get => _disabledSprite;
            set {
                _disabledSprite = value;
                if (!_isOn) _image.Image.sprite = _disabledSprite;
            }
        }

        public bool ShowBg {
            get => _background.Image.enabled;
            set => _background.Image.enabled = value;
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

        public event Action<bool> OnToggle;

        public Color enabledColor = Color.white;
        public Color disabledColor = Color.white;

        [UIComponent("image")] 
        private readonly BetterImage _image;

        [UIComponent("background")] 
        private readonly BetterImage _background;

        private Sprite _disabledSprite;
        private Sprite _enabledSprite;
        private Button _button;
        private int _width = 6;
        private int _height = 6;
        private bool _isOn;

        public void Toggle(bool val) {
            _isOn = val;
            _image.Image.sprite = val ? EnabledSprite : DisabledSprite;
            _image.Image.color = val ? enabledColor : disabledColor;
            OnToggle?.Invoke(val);
        }
        public void Toggle() {
            Toggle(!_isOn);
        }

        protected override void OnInitialize() {
            _button = _background.gameObject.AddComponent<Button>();
            _button.onClick.AddListener(Toggle);
        }
    }
}
