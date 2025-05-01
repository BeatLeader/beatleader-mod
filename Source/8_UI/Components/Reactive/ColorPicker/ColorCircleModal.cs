using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ColorCircleModal : ModalBase {
        public ColorCircle ColorCircle { get; } = new();

        protected override GameObject Construct() {
            return new Background {
                LayoutModifier = new YogaModifier {
                    Size = new() { x = 54f, y = 59f }
                },
                
                Children = {
                    new DialogHeader {
                        Text = "Select Color"
                    }.AsFlexItem(basis: 6f),
                    //content
                    ColorCircle.AsFlexItem(size: 54f)
                }
            }.AsBlurBackground().AsFlexGroup(direction: FlexDirection.Column).Use();
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            this.WithJumpAnimation();
        }
    }
}