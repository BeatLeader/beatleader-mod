using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class DialogHeader : ReactiveComponent {
        public string Text {
            get => _label.Text;
            set => _label.Text = value;
        }

        private Label _label = null!;

        protected override GameObject Construct() {
            return new Background {
                Sprite = BundleLoader.Sprites.backgroundTop,
                Color = (Color.white * 0.9f).ColorWithAlpha(1f),

                Children = {
                    new Label()
                        .AsFlexItem()
                        .Bind(ref _label)
                }
            }.AsFlexGroup(justifyContent: Justify.Center).AsBlurBackground().Use();
        }
    }
}