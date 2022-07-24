using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.TypeHandlers;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatLeader.UI.BSML_Addons.Components;
using BeatLeader.Utils;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using HMUI;

namespace BeatLeader.UI.BSML_Addons.Extensions
{
    [ComponentHandler(typeof(GenericSetting))]
    internal class GenericSettingExtensionHandler : TypeHandler
    {
        public override Dictionary<string, string[]> Props => new Dictionary<string, string[]>()
        {
            { "hideText", new[] { "hide-text" } },
        };

        public override void HandleType(BSMLParser.ComponentTypeWithData componentType, BSMLParserParams parserParams)
        {
            if (componentType.data.TryGetValue("hideText", out string showText) && bool.TryParse(showText, out bool showTextBool) && showTextBool)
            {
                SliderSetting sliderSetting;
                IncDecSetting incDecSetting;
                ToggleSetting toggleSetting;
                if ((sliderSetting = componentType.component as SliderSetting) != null)
                {
                    Transform parent = sliderSetting.slider.transform.parent;
                    GameObject.Destroy(sliderSetting.GetComponentInChildren<TextMeshProUGUI>().gameObject);
                    sliderSetting.slider.transform.GetChildren().ForEach(x => x.transform.SetParent(parent, false));
                }
                else if ((incDecSetting = componentType.component as IncDecSetting) != null)
                {
                    GameObject.Destroy(incDecSetting.GetComponentInChildren<TextMeshProUGUI>().gameObject);
                    incDecSetting.text.transform.parent.GetChildren().ForEach(x => x.transform.SetParent(incDecSetting.transform, false));
                }
                else if ((toggleSetting = componentType.component as ToggleSetting) != null)
                {
                    GameObject.Destroy(toggleSetting.text.gameObject);
                    toggleSetting.toggle.transform.GetChildren().ForEach(x => x.transform.SetParent(toggleSetting.transform, false));
                }
            }
        }
    }
}

