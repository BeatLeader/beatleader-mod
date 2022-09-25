using System.Collections.Generic;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.TypeHandlers;
using UnityEngine.UI;

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
