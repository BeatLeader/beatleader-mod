using BeatLeader.UI.BSML_Addons.Components;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.ToggleButton.bsml")]
    internal class SimpleButton : ReeUIComponentV2
    {
        public Sprite Sprite 
        {
            get => _image.Image.sprite; 
            set => _image.Image.sprite = value;
        }

        public event Action OnClick;

        public Color HighlightedColor = Color.white;
        public Color Color = Color.white;

        [UIComponent("image")] private BetterImage _image;
        [UIComponent("background")] private BetterImage _background;
        private NoTransitionsButton _button;

        public void Click()
        {
            OnClick?.Invoke();
        }

        protected override void OnInitialize()
        {
            _button = _background.gameObject.AddComponent<NoTransitionsButton>();
            _button.selectionStateDidChangeEvent += HandleStateChanged;
            _button.onClick.AddListener(Click);
        }
        private void HandleStateChanged(NoTransitionsButton.SelectionState state)
        {
            if (state is NoTransitionsButton.SelectionState.Highlighted)
            {
                _image.Image.color = HighlightedColor;
            }
            else if (state is NoTransitionsButton.SelectionState.Normal)
            {
                _image.Image.color = Color;
            }
        }
    }
}
