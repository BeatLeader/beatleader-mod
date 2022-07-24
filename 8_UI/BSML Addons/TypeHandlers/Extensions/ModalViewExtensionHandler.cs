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
    [ComponentHandler(typeof(ModalView))]
    internal class ModalViewExtensionHandler : TypeHandler
    {
        public override Dictionary<string, string[]> Props => new Dictionary<string, string[]>()
        {
            { "hideBackground", new string[] { "hide-background", "hide-bg" } }
        };

        public override void HandleType(BSMLParser.ComponentTypeWithData componentType, BSMLParserParams parserParams)
        {
            ModalView modal;
            if ((modal = componentType.component as ModalView) != null)
            {
                if (componentType.data.TryGetValue("hideBackground", out string overrideBackground))
                {
                    modal.GetComponentInChildren<ImageView>().enabled = !bool.Parse(overrideBackground);
                }
            }
        }
    }
}
