using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ImageButton : ColoredButton {
        #region UI Components

        public Image Image { get; private set; } = null!;

        #endregion

        #region Color

        protected override void ApplyColor(Color color, float t) {
            Image.Color = color;
        }

        #endregion

        #region Setup

        private RectTransform _childrenContainerTransform = null!;

        protected override void Construct(RectTransform rect) {
            //background
            Image = new Image {
                Name = "Background"
            }.WithRectExpand();
            Image.Use(rect);
            //content
            _childrenContainerTransform = new GameObject("Content").AddComponent<RectTransform>();
            _childrenContainerTransform.SetParent(rect, false);
            _childrenContainerTransform.WithRectExpand();
            base.Construct(rect);
        }

        protected override void AppendReactiveChild(ReactiveComponentBase comp) {
            comp.Use(_childrenContainerTransform);
        }

        protected override void OnInitialize() {
            Image.Material = BundleLoader.UIAdditiveGlowMaterial;
        }

        #endregion
    }
}