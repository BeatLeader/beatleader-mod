using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class TextButton : ColoredButton {
        #region UI Components

        public Label Label { get; private set; } = null!;

        #endregion

        #region Color

        protected override void ApplyColor(Color color) {
            if (Colors != null) {
                Label.Color = color;
            }
        }

        protected override void OnInteractableChange(bool interactable) {
            UpdateColor();
        }

        #endregion

        #region Setup

        protected override void Construct(RectTransform rect) {
            //label
            Label = new Label {
                Name = "Label"
            }.WithRectExpand();
            Label.Use(rect);
            //adding touchable to allow raycasts
            rect.gameObject.AddComponent<Touchable>();
            base.Construct(rect);
        }

        #endregion
    }
}