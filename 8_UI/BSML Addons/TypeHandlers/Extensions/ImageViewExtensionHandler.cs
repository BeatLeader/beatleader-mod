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
    [ComponentHandler(typeof(RectTransform))]
    internal class ImageViewExtensionHandler : TypeHandler
    {
        public override Dictionary<string, string[]> Props => new Dictionary<string, string[]>()
        {
            { "skew", new[] { "skew" } }
        };

        public override void HandleType(BSMLParser.ComponentTypeWithData componentType, BSMLParserParams parserParams)
        {
            if (componentType.data.TryGetValue("skew", out string skew) && float.TryParse(skew.Replace('.', ','), out float skewFloat))
            {
                ImageView[] images = componentType.component.GetComponentsInChildren<ImageView>();
                images.ToList().ForEach(delegate (ImageView view)
                {
                    view.SetField("_skew", skewFloat);
                    view.SetAllDirty();
                });
                TextMeshProUGUI[] texts = componentType.component.GetComponentsInChildren<TextMeshProUGUI>();
                texts.ToList().ForEach(delegate (TextMeshProUGUI text)
                {
                    text.fontStyle -= FontStyles.Italic;
                });
            }
        }
    }
}
