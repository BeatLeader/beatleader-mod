using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.TypeHandlers;
using BeatLeader.Replays.UI.BSML_Addons.Components;
using UnityEngine.UI;
using UnityEngine;

namespace BeatLeader.Replays.UI.BSML_Addons.TypeHandlers
{
    [ComponentHandler(typeof(StyledButton))]
    public class StyledButtonHandler : TypeHandler<StyledButton>
    {
        public override Dictionary<string, string[]> Props => new Dictionary<string, string[]>()
        {
            { "onClick", new[]{ "on-click" } },
            { "clickEvent", new[]{ "click-event", "event-click"} },
            { "normalStateImage", new[] { "normal-state-image"} },
            { "pressedStateImage", new[] { "pressed-state-image" } },
            { "highlightedStateImage", new[] { "highlighted-state-image" } },
            { "preserveAspect", new[] { "preserve-aspect" } }
        };
        public override Dictionary<string, Action<StyledButton, string>> Setters => new Dictionary<string, Action<StyledButton, string>>()
        {
            { "normalStateImage", new Action<StyledButton, string>((button, path) => button.NormalStateImage = Utilities.FindSpriteInAssembly(path)) },
            { "pressedStateImage", new Action<StyledButton, string>((button, path) => button.PressedStateImage = Utilities.FindSpriteInAssembly(path)) },
            { "highlightedStateImage", new Action<StyledButton, string>((button, path) => button.HighlightedStateImage = Utilities.FindSpriteInAssembly(path)) },
            { "preserveAspect", new Action<StyledButton, string>((button, preserveAspect) => button.PreserveAspectRate = bool.Parse(preserveAspect)) }
        };

        public override void HandleType(BSMLParser.ComponentTypeWithData componentType, BSMLParserParams parserParams)
        {
            try
            {
                StyledButton button;
                if (button = componentType.component as StyledButton)
                {
                    /*if (componentType.data.TryGetValue("normalStateImage", out string normalPath))
                    {
                        button.NormalStateImage = Utilities.FindSpriteInAssembly(normalPath);
                    }
                    if (componentType.data.TryGetValue("pressedStateImage", out string pressedPath))
                    {
                        button.PressedStateImage = Utilities.FindSpriteInAssembly(pressedPath);
                    }
                    if (componentType.data.TryGetValue("highlightedStateImage", out string highlightedPath))
                    {
                        button.HighlightedStateImage = Utilities.FindSpriteInAssembly(highlightedPath);
                    }

                    if (componentType.data.TryGetValue("preserveAspect", out string preserveAspect))
                    {
                        button.PreserveAspectRate = bool.Parse(preserveAspect);
                    }
                    */
                    if (componentType.data.TryGetValue("onClick", out string onClick))
                    {
                        button.Button.onClick.AddListener(delegate
                        {
                            if (!parserParams.actions.TryGetValue(onClick, out BSMLAction onClickAction))
                                throw new Exception("on-click action '" + onClick + "' not found");

                            onClickAction.Invoke();
                        });
                    }
                    if (componentType.data.TryGetValue("clickEvent", out string clickEvent))
                    {
                        button.Button.onClick.AddListener(delegate
                        {
                            parserParams.EmitEvent(clickEvent);
                        });
                    }
                    base.HandleType(componentType, parserParams);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Critical(ex);
            }
        }
    }
}
