using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.TypeHandlers;
using BeatLeader.UI.BSML_Addons.Components;
using UnityEngine.UI;
using UnityEngine;
using HMUI;

namespace BeatLeader.UI.BSML_Addons.Extensions
{
    [ComponentHandler(typeof(Graphic))]
    internal class GraphicExtensionHandler : TypeHandler
    {
        public override Dictionary<string, string[]> Props => new Dictionary<string, string[]>()
        {
            { "useMask", new string[] { "as-mask", "maskable", "use-as-mask", } }
        };

        public override void HandleType(BSMLParser.ComponentTypeWithData componentType, BSMLParserParams parserParams)
        {
            Graphic graphic;
            if ((graphic = componentType.component as Graphic) != null)
            {
                if (componentType.data.TryGetValue("useMask", out string useAsMask))
                {
                    graphic.gameObject.AddComponent<Mask>();
                }
            }
        }
    }
}
