using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.SubMenuButton.bsml")]
    internal class SubMenuButton : ReeUIComponentV2
    {
        [UIValue("text")] public string Text
        {
            get => _text;
            set
            {
                _text = value;
                NotifyPropertyChanged(nameof(Text));
            }
        }
        public Menu Menu { get; private set; }
        public GameObject ButtonGameObject => Content.gameObject;

        public event Action<Menu> OnClick;

        private string _text;

        public void Init(Menu menu, string text)
        {
            Text = text;
            Menu = menu;
        }
        [UIAction("button-clicked")] private void OnButtonClicked() => OnClick?.Invoke(Menu);
    }
}
