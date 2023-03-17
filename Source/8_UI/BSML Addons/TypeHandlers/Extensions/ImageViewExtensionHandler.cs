using System.Collections.Generic;
using IPA.Utilities;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.TypeHandlers;
using UnityEngine;
using TMPro;
using HMUI;

namespace BeatLeader.UI.BSML_Addons.Extensions {
    [ComponentHandler(typeof(RectTransform))]
    internal class ImageViewExtensionHandler : TypeHandler {
        public override Dictionary<string, string[]> Props => new Dictionary<string, string[]>()
        {
            { "skew", new[] { "skew" } }
        };

        public override void HandleType(BSMLParser.ComponentTypeWithData componentType, BSMLParserParams parserParams) {
            if (!componentType.data.TryGetValue("skew", out string skew) ||
                !float.TryParse(skew.Replace('.', ','), out float skewFloat)) return;

            var component = componentType.component;
            foreach (var view in component.GetComponentsInChildren<ImageView>()) {
                view.SetField("_skew", skewFloat);
                view.SetAllDirty();
            }
            foreach (var text in component.GetComponentsInChildren<TextMeshProUGUI>()) {
                text.fontStyle -= FontStyles.Italic;
            }
        }
    }
}
