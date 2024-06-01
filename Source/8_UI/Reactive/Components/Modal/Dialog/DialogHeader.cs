using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class DialogHeader : ReactiveComponent {
        public string Text {
            get => _label.Text;
            set => _label.Text = value;
        }

        private Label _label = null!;
        
        protected override GameObject Construct() {
            return new Image {
                Sprite = BundleLoader.Sprites.backgroundTop,
                Color = (Color.white * 0.9f).ColorWithAlpha(1f),
                Children = {
                    new Label().Bind(ref _label).WithRectExpand()
                }
            }.AsBlurBackground().Use();
        }
    }
}