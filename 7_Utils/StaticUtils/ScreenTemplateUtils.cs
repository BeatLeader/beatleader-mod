using BeatLeader.Components;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

namespace BeatLeader.Utils {
    internal static class ScreenTemplateUtils {
        public static void Apply2DTemplate(this CanvasScreen screen) {
            Apply2DTemplate(screen.Canvas, screen.CanvasScaler);
        }

        public static void ApplyVRTemplate(this CanvasScreen screen) {
            ApplyVRTemplate(screen.Canvas, screen.CanvasScaler);
        }

        public static void Apply2DTemplate(this Canvas canvas, CanvasScaler? scaler = null) {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            ApplyUniversalTemplate(canvas);
            if (scaler == null) return;
            ApplyUniversalTemplate(scaler);
            scaler.referenceResolution = new(350, 300);
            scaler.uiScaleMode = ScaleMode.ScaleWithScreenSize;
        }

        public static void ApplyVRTemplate(this Canvas canvas, CanvasScaler? scaler = null) {
            ApplyUniversalTemplate(canvas);
            if (scaler == null) return;
            ApplyUniversalTemplate(scaler);
        }

        public static void ApplyUniversalTemplate(this Canvas canvas) {
            canvas.sortingOrder = 1;
            canvas.additionalShaderChannels =
                AdditionalCanvasShaderChannels.TexCoord1
                | AdditionalCanvasShaderChannels.TexCoord2;
        }

        public static void ApplyUniversalTemplate(this CanvasScaler scaler) {
            scaler.dynamicPixelsPerUnit = 3.44f;
            scaler.referencePixelsPerUnit = 10f;
        }
    }
}
