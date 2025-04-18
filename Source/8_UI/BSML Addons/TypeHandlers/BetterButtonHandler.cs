using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.TypeHandlers;
using BeatLeader.UI.BSML_Addons.Components;
using UnityEngine.UI;
using UnityEngine;
using BeatLeader.Utils;

namespace BeatLeader.UI.BSML_Addons.TypeHandlers
{
    [ComponentHandler(typeof(BetterButton))]
    public class BetterButtonHandler : TypeHandler<BetterButton>
    {
        public override Dictionary<string, string[]> Props => new Dictionary<string, string[]>()
        {
            { "onClick", new[] { "on-click" } },
            { "clickEvent", new[] { "click-event", "event-click"} },
            //button
            { "transition", new[] { "transition" } },

            { "normalColor", new[] { "normal-color" } },
            { "highlightedColor", new[] { "highlighted-color" } },
            { "pressedColor", new[] { "pressed-color" } },
            { "colorMultiplier", new[] { "color-multiplier" } },

            { "highlightedSprite", new[] { "highlighted-sprite", "highlighted-image" } },
            { "pressedSprite", new[] { "pressed-sprite" , "pressed-image" } },
            //image 
            { "image", new[] { "normal-sprite", "normal-image" } },
            { "preserveAspect", new[] { "preserve-aspect" } },
            { "imageColor", new[] { "image-color", "img-color" } },
            { "imageType", new[] { "image-type", "type" } },
            { "fillMethod", new[] { "fill-method" } },
            { "fillOrigin", new[] { "fill-origin" } },
            { "fillAmount", new[] { "fill-amount" } },
            { "fillClockwise", new[] { "fill-clockwise", "clockwise" } },
            { "pixelsPerUnit", new[] { "pixels-per-unit-multiplier", "pixels-per-unit", "ppu" } }
        };
        public override Dictionary<string, Action<BetterButton, string>> Setters => new Dictionary<string, Action<BetterButton, string>>();

        public override void HandleType(BSMLParser.ComponentTypeWithData componentType, BSMLParserParams parserParams)
        {
            try
            {
                BetterButton button;
                if (button = componentType.Component as BetterButton)
                {
                    if (componentType.Data.TryGetValue("transition", out string transition) && transition != "Animation")
                    {
                        button.Button.transition = (Selectable.Transition)Enum.Parse(typeof(Selectable.Transition), transition);
                    }

                    #region Button colors
                    if (componentType.Data.TryGetValue("normalColor", out string normalColor))
                    {
                        ColorBlock colors = button.Button.colors;
                        if (ColorUtility.TryParseHtmlString(normalColor, out Color color))
                        {
                            colors.normalColor = color;
                            button.Button.colors = colors;
                        }
                    }
                    if (componentType.Data.TryGetValue("highlightedColor", out string highlightedColor))
                    {
                        ColorBlock colors = button.Button.colors;
                        if (ColorUtility.TryParseHtmlString(highlightedColor, out Color color))
                        {
                            colors.highlightedColor = color;
                            button.Button.colors = colors;
                        }
                    }
                    if (componentType.Data.TryGetValue("pressedColor", out string pressedColor))
                    {
                        ColorBlock colors = button.Button.colors;
                        if (ColorUtility.TryParseHtmlString(pressedColor, out Color color))
                        {
                            colors.pressedColor = color;
                            button.Button.colors = colors;
                        }
                    }
                    if (componentType.Data.TryGetValue("colorMultiplier", out string colorMultiplier))
                    {
                        ColorBlock colors = button.Button.colors;
                        if (float.TryParse(colorMultiplier, out float result))
                        {
                            colors.colorMultiplier = result;
                            button.Button.colors = colors;
                        }
                    }
                    #endregion

                    if (componentType.Data.TryGetValue("highlightedSprite", out string highlightedImage))
                    {
                        SpriteState spriteState = button.Button.spriteState;
                        spriteState.highlightedSprite = BSMLUtility.LoadSprite(highlightedImage);
                        button.Button.spriteState = spriteState;
                    }
                    if (componentType.Data.TryGetValue("pressedSprite", out string pressedImage))
                    {
                        SpriteState spriteState = button.Button.spriteState;
                        spriteState.pressedSprite = BSMLUtility.LoadSprite(pressedImage);
                        button.Button.spriteState = spriteState;
                    }

                    BetterImageHandler.HandleImage(componentType.Data, button.TargetGraphic);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Critical(ex);
            }
        }
    }
}
