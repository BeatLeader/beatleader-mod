using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ImageButton : ColoredButton {
        #region UI Components

        public Image Image { get; } = Lazy<Image>();

        #endregion

        #region Color

        protected override void ApplyColor(Color color, float t) {
            Image.Color = color;
        }

        #endregion

        #region Setup

        protected override void Construct(RectTransform rect) {
            Image.Apply(rect);
            base.Construct(rect);
        }

        #endregion
    }
}