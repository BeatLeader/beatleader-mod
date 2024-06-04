using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class AeroButton : ImageButton {
        private static readonly Color hoveredGradientColor = Color.white.ColorWithAlpha(0.5f);
        private static readonly Color defaultGradientColor = Color.white;

        protected override void OnInteractableChange(bool interactable) {
            Image.UseGradient = interactable;
            UpdateColor();
        }

        protected override void OnInitialize() {
            Image.Sprite = BundleLoader.Sprites.background;
            Image.ImageType = UnityEngine.UI.Image.Type.Sliced;
            Image.PixelsPerUnit = 12f;
            Image.GradientDirection = ImageView.GradientDirection.Vertical;
            Colors = UIStyle.InputColorSet;
            GradientColors0 = new() {
                Color = defaultGradientColor
            };
            GradientColors1 = new() {
                Color = defaultGradientColor,
                HoveredColor = hoveredGradientColor
            };
        }
    }
}