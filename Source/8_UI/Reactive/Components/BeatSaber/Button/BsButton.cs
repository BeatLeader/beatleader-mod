using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class BsButton : ImageButton {
        private static readonly Color inactiveColor = Color.white;
        private static readonly Color activeColor = Color.white.ColorWithAlpha(0.5f);
        
        protected override void Construct(RectTransform rect) {
            //underline
            new Image {
                Sprite = BundleLoader.Sprites.backgroundUnderline,
                PixelsPerUnit = 12f,
                ImageType = UnityEngine.UI.Image.Type.Sliced,
                Color = Color.white.ColorWithAlpha(0.5f)
            }.WithRectExpand().Use(rect);
            base.Construct(rect);
        }

        protected override void OnInitialize() {
            Colors = new StateColorSet {
                ActiveColor = Color.white.ColorWithAlpha(0.5f),
                HoveredColor = Color.white.ColorWithAlpha(0.3f),
                Color = UIStyle.ControlColorSet.Color
            };
            GradientColors1 = new StateColorSet {
                HoveredColor = activeColor,
                Color = inactiveColor
            };
            GrowOnHover = false;
            HoverLerpMul = 100f;
            Image.GradientColor0 = Color.white;
            Image.Sprite = BundleLoader.Sprites.background;
            Image.PixelsPerUnit = 12f;
            Image.UseGradient = true;
        }
    }
}