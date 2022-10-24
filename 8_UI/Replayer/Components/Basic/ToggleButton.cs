using BeatLeader.UI.BSML_Addons.Components;
using BeatSaberMarkupLanguage.Attributes;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components
{
    internal class ToggleButton : ReeUIComponentV2
    {
        public Sprite EnabledSprite
        {
            get => _enabledSprite == null ? DisabledSprite : _enabledSprite;
            set => _enabledSprite = value;
        }
        public Sprite DisabledSprite
        {
            get => _disabledSprite;
            set
            {
                _disabledSprite = value;
                if (!_isOn) _image.Image.sprite = _disabledSprite;
            }
        }

        public event Action<bool> OnToggle;

        public Color EnabledColor = Color.white;
        public Color DisabledColor = Color.white;

        [UIComponent("image")] private BetterImage _image;
        [UIComponent("background")] private BetterImage _background;
        private Sprite _disabledSprite;
        private Sprite _enabledSprite;
        private Button _button;
        private bool _isOn;

        public void Toggle(bool val)
        {
            _isOn = val;
            _image.Image.sprite = val ? EnabledSprite : DisabledSprite;
            _image.Image.color = val ? EnabledColor : DisabledColor;
            OnToggle?.Invoke(val);
        }
        public void Toggle()
        {
            Toggle(!_isOn);
        }

        protected override void OnInitialize()
        {
            _button = _background.gameObject.AddComponent<Button>();
            _button.onClick.AddListener(Toggle);
        }
    }
}
