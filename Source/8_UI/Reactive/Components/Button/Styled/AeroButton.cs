using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class AeroButton : ImageButton {
        public new static readonly Color DefaultColor = Color.black.ColorWithAlpha(0.5f);
        public static readonly Color DefaultHoverColor = Color.magenta;
        public static readonly Color DefaultDisabledColor = Color.white.ColorWithAlpha(0.2f);
        
        private static readonly Color hoveredGradientColor = Color.white.ColorWithAlpha(0.5f);
        private static readonly Color defaultGradientColor = Color.white;

        protected override void OnInteractableChange(bool interactable) {
            Image.UseGradient = interactable;
            if (interactable) {
                UpdateColor(0f);
            } else {
                Image.Color = DefaultDisabledColor;
            }
        }

        protected override void ApplyColor(Color color, float t) {
            Image.GradientColor1 = LerpColor(t, defaultGradientColor, hoveredGradientColor);
            Image.Color = color;
        }

        protected override void OnButtonStateChange(bool state) {
            UpdateColor(1f);
        }

        protected override void OnInitialize() {
            Image.Sprite = BundleLoader.Sprites.background;
            Image.Color = DefaultColor;
            Image.GradientColor0 = defaultGradientColor;
            Image.GradientColor1 = defaultGradientColor;
            Image.UseGradient = true;
            Image.ImageType = UnityEngine.UI.Image.Type.Sliced;
            Image.PixelsPerUnit = 12f;
            Image.GradientDirection = ImageView.GradientDirection.Vertical;
            Color = DefaultColor;
            HoverColor = DefaultHoverColor;
        }
    }
}