using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleViewHeader : ReactiveComponent {
        public string Text {
            get => _label.Text;
            set => _label.Text = value;
        }

        private Label _label = null!;

        protected override GameObject Construct() {
            return new Background {
                FlexItem = {
                    Size = new() { y = 8f, x = 100.pct },
                    Position = new() { top = 0f },
                    PositionType = PositionType.Absolute
                },

                Children = {
                    new Label {
                        FontSize = 6f,
                        Text = "Battle Setup"
                    }.WithRectExpand().Bind(ref _label)
                }
            }.AsBlurBackground().Use();
        }
    }
}