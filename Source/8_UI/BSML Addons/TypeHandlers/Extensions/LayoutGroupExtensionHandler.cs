using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.TypeHandlers;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.BSML_Addons.Extensions {
    [ComponentHandler(typeof(LayoutGroup))]
    public class LayoutGroupExtensionHandler : TypeHandler<LayoutGroup> {
        public override Dictionary<string, string[]> Props { get; } = new() {
            { "pad", new[] { "Pad" } }
        };

        public override Dictionary<string, Action<LayoutGroup, string>> Setters { get; } = new() {
            { "pad", (group, str) => group.padding = StringConverter.Convert<RectOffset>(str) },
        };
    }
}