using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.UI.BSML_Addons.Components;
using BeatSaberMarkupLanguage.TypeHandlers;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage;
using UnityEngine.UI;
using UnityEngine;

namespace BeatLeader.UI.BSML_Addons.TypeHandlers
{
    [ComponentHandler(typeof(BetterImage))]
    public class BetterImageHandler : TypeHandler
    {
        public override Dictionary<string, string[]> Props => new Dictionary<string, string[]>()
        {
            { "image", new[] { "source" , "src" } },
            { "preserveAspect", new[] { "preserve-aspect" } },
            { "raycastImage", new[] { "raycast-image" } },
            { "imageColor", new[] { "image-color", "img-color", "color" } },
            { "transparency", new[] { "alpha", "transparency" } },
            { "imageType", new[] { "image-type", "type" } },
            { "fillMethod", new[] { "fill-method" } },
            { "fillOrigin", new[] { "fill-origin" } },
            { "fillAmount", new[] { "fill-amount" } },
            { "fillClockwise", new[] { "fill-clockwise", "clockwise" } },
            { "pixelsPerUnit", new[] { "pixels-per-unit-multiplier", "pixels-per-unit", "ppu" } }
        };

        public static void HandleImage(Dictionary<string, string> data, Image image)
        {
            try
            {
                if (data.TryGetValue("image", out string imagePath))
                {
                    image.SetImage(imagePath);
                }
                if (data.TryGetValue("imageColor", out string imageColor) && ColorUtility.TryParseHtmlString(imageColor, out Color color))
                {
                    image.color = color;
                }
                if (data.TryGetValue("preserveAspect", out string preserveAspect))
                {
                    image.preserveAspect = bool.Parse(preserveAspect);
                }
                if (data.TryGetValue("raycastImage", out string raycastImage))
                {
                    image.raycastTarget = bool.Parse(raycastImage);
                }
                if (data.TryGetValue("imageType", out string imageType))
                {
                    image.type = (Image.Type)Enum.Parse(typeof(Image.Type), imageType);
                }
                if (data.TryGetValue("transparency", out string transparency))
                {
                    var col = image.color;
                    if (float.TryParse(transparency, out float result))
                    {
                        image.color = new Color(col.r, col.g, col.b, result);
                    }
                }
                if (data.TryGetValue("fillMethod", out string fillMethod))
                {
                    image.fillMethod = (Image.FillMethod)Enum.Parse(typeof(Image.FillMethod), fillMethod);
                }
                if (data.TryGetValue("fillOrigin", out string fillOrigin))
                {
                    int num = -1;
                    switch (image.fillMethod)
                    {
                        case Image.FillMethod.Horizontal:
                            num = (int)Enum.Parse(typeof(Image.OriginHorizontal), fillOrigin);
                            break;
                        case Image.FillMethod.Vertical:
                            num = (int)Enum.Parse(typeof(Image.OriginVertical), fillOrigin);
                            break;
                        case Image.FillMethod.Radial90:
                            num = (int)Enum.Parse(typeof(Image.Origin90), fillOrigin);
                            break;
                        case Image.FillMethod.Radial180:
                            num = (int)Enum.Parse(typeof(Image.Origin180), fillOrigin);
                            break;
                        case Image.FillMethod.Radial360:
                            num = (int)Enum.Parse(typeof(Image.Origin360), fillOrigin);
                            break;
                    }
                    image.fillOrigin = num;
                }
                if (data.TryGetValue("fillAmount", out string fillAmount))
                {
                    image.fillAmount = float.Parse(fillAmount);
                }
                if (data.TryGetValue("fillClockwise", out string clockwise))
                {
                    image.fillClockwise = bool.Parse(clockwise);
                }
                if (data.TryGetValue("pixelsPerUnit", out string ppu))
                {
                    ppu = ppu.Replace('.', ',');
                    image.pixelsPerUnitMultiplier = float.Parse(ppu);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Critical($"An unhanled exception occuried during attempting to parse image component! {ex}");
            }
        }
        public static void HandleImage(Dictionary<string, string> data, Image image, string prefix)
        {
            Dictionary<string, string> generatedData = new Dictionary<string, string>();
            foreach (var item in data)
            {
                string key = item.Key;
                if (key.Contains(prefix))
                {
                    char[] chars = item.Key.Replace(prefix, "").ToCharArray();
                    chars[0] = char.ToLower(chars[0]);
                    key = new string(chars);
                }
                generatedData.Add(key, item.Value);
            }
            HandleImage(generatedData, image);
        }

        public override void HandleType(BSMLParser.ComponentTypeWithData componentType, BSMLParserParams parserParams)
        {
            BetterImage image;
            if ((image = componentType.component as BetterImage) != null)
            {
                HandleImage(componentType.data, image.Image);
            }
        }
    }
}
