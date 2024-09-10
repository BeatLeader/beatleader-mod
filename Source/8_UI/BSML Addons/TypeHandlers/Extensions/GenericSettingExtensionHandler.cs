﻿using System.Collections.Generic;
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
        public override Dictionary<string, string[]> Props => new() {
            { "hideText", new[] { "hide-text" } },
        };

        public override void HandleType(BSMLParser.ComponentTypeWithData componentType, BSMLParserParams parserParams) {
            if (!componentType.Data.TryGetValue("hideText", out var showText) ||
                !bool.TryParse(showText, out var showTextBool) || !showTextBool) return;

            var text = default(GameObject?);
            var transform = default(Transform?);
            var parent = default(Transform?);
            switch (componentType.Component) {
                case SliderSetting sliderSetting:
                    var sliderTransform = sliderSetting.Slider.transform;
                    parent = sliderTransform.parent;
                    transform = sliderTransform;
                    text = sliderSetting.GetComponentInChildren<TextMeshProUGUI>().gameObject;
                    break;
                case IncDecSetting incDecSetting:
                    text = incDecSetting.GetComponentInChildren<TextMeshProUGUI>().gameObject;
                    transform = incDecSetting.TextMesh.transform.parent;
                    parent = incDecSetting.transform;
                    break;
                case ToggleSetting toggleSetting:
                    text = toggleSetting.TextMesh.gameObject;
                    transform = toggleSetting.Toggle.transform;
                    parent = toggleSetting.transform;
                    break;
                default:
                    return;
            }

            Object.Destroy(text);
            foreach (var item in transform.GetChildren()) {
                item.transform.SetParent(parent, false);
            }
        }
    }
}