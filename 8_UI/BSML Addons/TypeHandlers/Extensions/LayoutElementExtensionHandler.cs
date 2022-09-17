using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.TypeHandlers;
using UnityEngine.UI;

namespace BeatLeader.UI.BSML_Addons.Extensions
{
    [ComponentHandler(typeof(LayoutElement))]
    internal class LayoutElementExtensionHandler : TypeHandler<LayoutElement>
    {
        public override Dictionary<string, string[]> Props => new Dictionary<string, string[]>()
        {
            { "flexibleHeight", new string[] { "flexible-height" } },
            { "flexibleWidth", new string[] { "flexible-width" } },
        };   
        public override Dictionary<string, Action<LayoutElement, string>> Setters => new Dictionary<string, Action<LayoutElement, string>>
        {
            { "flexibleHeight", delegate(LayoutElement el, string str) { el.flexibleHeight = float.Parse(str.Replace('.', ',')); } },
            { "flexibleWidth", delegate(LayoutElement el, string str) { el.flexibleWidth = float.Parse(str.Replace('.', ',')); } },
        };
    }
}
