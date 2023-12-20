using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.TypeHandlers;
using UnityEngine.UI;
using static UnityEngine.UI.ContentSizeFitter.FitMode;

namespace BeatLeader.UI.BSML_Addons.Extensions {
    [ComponentHandler(typeof(ContentSizeFitter))]
    public class ContentSizeFitterExtensionHandler : TypeHandler<ContentSizeFitter> {
        public override Dictionary<string, string[]> Props { get; } = new() {
            { "inheritHeight", new[] { "InheritHeight" } },
            { "inheritWidth", new[] { "InheritWidth" } },
            { "inheritSize", new[] { "InheritSize" } }
        };

        public override Dictionary<string, Action<ContentSizeFitter, string>> Setters { get; } = new() {
            { "inheritHeight", (fitter, str) => fitter.verticalFit = bool.Parse(str) ? Unconstrained : PreferredSize },
            { "inheritWidth", (fitter, str) => fitter.horizontalFit = bool.Parse(str) ? Unconstrained : PreferredSize }, {
                "inheritSize", (fitter, str) => {
                    var fit = bool.Parse(str) ? Unconstrained : PreferredSize;
                    fitter.verticalFit = fit;
                    fitter.verticalFit = fit;
                }
            }
        };
    }
}