using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

namespace BeatLeader.Utils
{
    public static class HMUIUtils
    {
        public static Canvas SetupAsViewController(this Canvas canvas, float dynamicPixelsPerUnit = 3.44f, float referencePixelsPerUnit = 10f)
        {
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
            CanvasScaler scaler = canvas.gameObject.GetOrAddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = dynamicPixelsPerUnit;
            scaler.referencePixelsPerUnit = referencePixelsPerUnit;
            canvas.gameObject.GetOrAddComponent<CanvasGroup>();
            canvas.gameObject.GetOrAddComponent<HMUI.Screen>();
            canvas.gameObject.GetOrAddComponent<GraphicRaycaster>();
            return canvas;
        }
    }
}
