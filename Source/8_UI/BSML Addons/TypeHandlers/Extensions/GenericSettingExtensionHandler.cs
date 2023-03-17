using System.Collections.Generic;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.TypeHandlers;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatLeader.Utils;
using UnityEngine;
using TMPro;

namespace BeatLeader.UI.BSML_Addons.Extensions {
    [ComponentHandler(typeof(GenericSetting))]
    internal class GenericSettingExtensionHandler : TypeHandler {
        public override Dictionary<string, string[]> Props => new Dictionary<string, string[]>()
        {
            { "hideText", new[] { "hide-text" } },
        };

        public override void HandleType(BSMLParser.ComponentTypeWithData componentType, BSMLParserParams parserParams) {
            if (!componentType.data.TryGetValue("hideText", out string showText) || 
                !bool.TryParse(showText, out bool showTextBool) || !showTextBool) return;

            GameObject text = null;
            Transform transform = null;
            Transform parent = null;
            switch (componentType.component) {
                case SliderSetting sliderSetting:
                    parent = sliderSetting.slider.transform.parent;
                    transform = sliderSetting.slider.transform;
                    text = sliderSetting.GetComponentInChildren<TextMeshProUGUI>().gameObject;
                    break;
                case IncDecSetting incDecSetting:
                    text = incDecSetting.GetComponentInChildren<TextMeshProUGUI>().gameObject;
                    transform = incDecSetting.text.transform.parent; 
                    parent = incDecSetting.transform;
                    break;
                case ToggleSetting toggleSetting:
                    text = toggleSetting.text.gameObject;
                    transform = toggleSetting.toggle.transform; 
                    parent = toggleSetting.transform;
                    break;
                default:
                    return;
            }
            
            GameObject.Destroy(text);
            transform.GetChildren().ForEach(x => x.transform.SetParent(parent, false));
        }
    }
}

