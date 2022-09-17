using System;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;
using BeatLeader.UI.BSML_Addons.Components;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.SubMenuButton.bsml")]
    internal class SubMenuButton : ReeUIComponentV2
    {
        public Menu Menu { get; private set; }
        public GameObject ButtonGameObject => Content.gameObject;
        public bool Interactable
        {
            get => _button.Button.interactable;
            set => _button.Button.interactable = value;
        }

        public event Action<Menu> OnClick;

        [UIValue("text")] public string Text
        {
            get => _text;
            set
            {
                _text = value;
                NotifyPropertyChanged(nameof(Text));
            }
        }
        [UIComponent("button")] private BetterButton _button;
        private string _text;

        public void Init(Menu menu, string text = null)
        {
            Text = string.IsNullOrEmpty(text) ? Text : text;
            Menu = menu;
        }

        protected override void OnInitialize()
        {
            var colors = _button.Button.colors;
            ColorUtility.TryParseHtmlString("#C8C8C832", out Color color);
            colors.disabledColor = color;
            _button.Button.colors = colors;
        }
        [UIAction("button-clicked")] private void OnButtonClicked() => OnClick?.Invoke(Menu);
    }
}
