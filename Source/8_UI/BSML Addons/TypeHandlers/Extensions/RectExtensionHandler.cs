using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.TypeHandlers;
using UnityEngine;

namespace BeatLeader.UI.BSML_Addons.Extensions {
    [ComponentHandler(typeof(RectTransform))]
    public class RectExtensionHandler : TypeHandler<RectTransform> {
        public override Dictionary<string, string[]> Props { get; } = new() {
            { "pivot", new[] { "Pivot" } },
            { "anchorMin", new[] { "AnchorMin" } },
            { "anchorMax", new[] { "AnchorMax" } },
            { "deltaSize", new[] { "DeltaSize" } },
        };

        public override Dictionary<string, Action<RectTransform, string>> Setters { get; } = new() {
            { "pivot", (rect, str) => rect.pivot = StringConverter.Convert<Vector2>(str) },
            { "anchorMin", (rect, str) => rect.anchorMin = StringConverter.Convert<Vector2>(str) },
            { "anchorMax", (rect, str) => rect.anchorMax = StringConverter.Convert<Vector2>(str) },
            { "deltaSize", (rect, str) => rect.sizeDelta = StringConverter.Convert<Vector2>(str) },
        };
    }
}