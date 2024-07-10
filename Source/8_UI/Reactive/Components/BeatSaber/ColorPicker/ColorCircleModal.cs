using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ColorCircleModal : AnimatedModalComponentBase {
        public ColorCircle ColorCircle { get; } = new();

        protected override GameObject Construct() {
            return new Image {
                Children = {
                    new DialogHeader {
                        Text = "Select Color"
                    }.AsFlexItem(basis: 6f),
                    //content
                    ColorCircle.AsFlexItem(size: 54f)
                }
            }.AsBlurBackground().AsFlexGroup(
                direction: FlexDirection.Column
            ).WithSizeDelta(54f, 59f).Use();
        }
    }
}