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
            { "height", new[] { "Height" } },
            { "width", new[] { "Width" } },
            { "ignoreLayout", new[] { "IgnoreLayout" } },
        };

        public override Dictionary<string, Action<LayoutElement, string>> Setters { get; } = new() {
            {
                "flexibleHeight", (el, str) => {
                    if (str is "max") {
                        el.flexibleHeight = int.MaxValue;
                        return;
                    }
                    el.flexibleHeight = ParseFloat(str);
                }
            }, {
                "flexibleWidth", (el, str) => {
                    if (str is "max") {
                        el.flexibleWidth = int.MaxValue;
                        return;
                    }
                    el.flexibleWidth = ParseFloat(str);
                }
            }, {
                "maxHeight", (el, str) => {
                    var bElement = el.gameObject.AddComponent<BoundedLayoutElement>();
                    if (str is "fit-parent") {
                        bElement.inheritMaxHeight = true;
                        return;
                    }
                    bElement.maxHeight = ParseFloat(str);
                }
            }, {
                "maxWidth", (el, str) => {
                    var bElement = el.gameObject.AddComponent<BoundedLayoutElement>();
                    if (str is "fit-parent") {
                        bElement.inheritMaxWidth = true;
                        return;
                    }
                    bElement.maxWidth = ParseFloat(str);
                }
            },
            { "height", (el, str) => el.preferredHeight = ParseFloat(str) },
            { "width", (el, str) => el.preferredWidth = ParseFloat(str) },
            { "ignoreLayout", (el, str) => el.ignoreLayout = bool.Parse(str) },
        };

        private static float ParseFloat(string str) {
            return float.Parse(str.Replace('.', ','));
        }
    }
}