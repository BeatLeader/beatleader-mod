using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.Reactive {
    internal static class ReactiveUtils {
        public static void AddCanvas(IReactiveComponent component) {
            AddCanvas(component, 3000, out _, out _);
        }
        
        public static void AddCanvas(IReactiveComponent component, int sortingOrder, out Canvas canvas, out CanvasScaler scaler) {
            var content = component.Content;
            //
            canvas = content.AddComponent<Canvas>();
            canvas.sortingOrder = sortingOrder;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
            //
            scaler = content.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 3.44f;
            scaler.referencePixelsPerUnit = 10f;
        }
    }
}