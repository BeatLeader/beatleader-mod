using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class BsButton : ImageButton {
        private static readonly Color inactiveColor = Color.white;
        private static readonly Color activeColor = Color.white.ColorWithAlpha(0.5f);

        protected override void OnInitialize() {
            Colors = new() {
                ActiveColor = Color.white.ColorWithAlpha(0.5f),
                HoveredColor = Color.white.ColorWithAlpha(0.3f),
                Color = UIStyle.ControlColorSet.Color
            };
            GradientColors1 = new() {
                HoveredColor = activeColor,
                Color = inactiveColor
            };
            GrowOnHover = false;
            HoverLerpMul = 100f;
            Image.GradientColor0 = Color.white;
            Image.Sprite = BundleLoader.Sprites.background;
            Image.PixelsPerUnit = 15f;
            Image.UseGradient = true;
        }
    }
}