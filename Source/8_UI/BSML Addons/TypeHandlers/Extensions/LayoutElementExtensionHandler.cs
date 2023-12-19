using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.TypeHandlers;
using UnityEngine.UI;

namespace BeatLeader.UI.BSML_Addons.Extensions {
    [ComponentHandler(typeof(LayoutElement))]
    internal class LayoutElementExtensionHandler : TypeHandler<LayoutElement> {
        public override Dictionary<string, string[]> Props { get; } = new() {
            { "flexibleHeight", new[] { "flexible-height", "FlexibleHeight" } },
            { "flexibleWidth", new[] { "flexible-width", "FlexibleWidth" } },
            { "maxHeight", new[] { "MaxHeight" } },
            { "maxWidth", new[] { "MaxWidth" } },
        };

        public override Dictionary<string, Action<LayoutElement, string>> Setters { get; } = new() {
            { "flexibleHeight", (el, str) => { el.flexibleHeight = float.Parse(str.Replace('.', ',')); } },
            { "flexibleWidth", (el, str) => { el.flexibleWidth = float.Parse(str.Replace('.', ',')); } },
            { "maxHeight", (el, str) => {
                var bElement = el.gameObject.AddComponent<BoundedLayoutElement>();
                if (str is "fit-parent") {
                    bElement.inheritMaxHeight = true;
                    return;
                }
                bElement.maxHeight = float.Parse(str.Replace('.', ','));
            } },
            { "maxWidth", (el, str) => {
                var bElement = el.gameObject.AddComponent<BoundedLayoutElement>();
                if (str is "fit-parent") {
                    bElement.inheritMaxWidth = true;
                    return;
                }
                bElement.maxWidth = float.Parse(str.Replace('.', ','));
            } },
        };
    }
}