using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.Reactive {
    internal static class ReactiveUtils {
        #region Sprites

        public static Texture2D? CreateTexture(byte[] bytes) {
            if (bytes.Length == 0) return null;
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false, false);
            return texture.LoadImage(bytes) ? texture : null;
        }

        public static Sprite? CreateSprite(byte[] image) {
            return CreateTexture(image) is { } texture ? CreateSprite(texture) : null;
        }

        public static Sprite? CreateSprite(Texture2D texture) {
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(texture.width / 2f, texture.height / 2f)
            );
        }

        #endregion

        #region Canvas

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

        #endregion
    }
}