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
    internal class SubMenuButton : MonoBehaviour
    {
        public SubMenu SubMenu { get; private set; }

        public event Action<SubMenu> OnClick;

        [UIValue("text")] private string text => SubMenu.Name;

        public void Init(SubMenu menu)
        {
            var rect = gameObject.GetOrAddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(48, 5);
            SubMenu = menu;
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), 
                Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.SubMenuButton.bsml"), gameObject, this);
        }
        [UIAction("button-clicked")] private void OnButtonClicked() => OnClick?.Invoke(SubMenu);
    }
}
