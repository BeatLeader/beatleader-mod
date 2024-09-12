using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class IconSegmentedControl<TKey> : SegmentedControl<TKey, Sprite, IconKeyedControlComponentCell<TKey>> { }

    internal class IconKeyedControlComponentCell<TKey> : KeyedControlComponentCell<TKey, Sprite> {
        public ImageButton Button => _button;

        private ImageButton _button = null!;

        public override void OnInit(TKey key, Sprite param) {
            Button.Image.Sprite = param;
        }

        public override void OnCellStateChange(bool selected) {
            Button.Click(selected);
        }

        protected override GameObject Construct() {
            return new ImageButton {
                Image = {
                    PreserveAspect = true
                },
                Latching = true,
                Colors = UIStyle.ButtonColorSet,
                OnStateChanged = _ => SelectSelf()
            }.Bind(ref _button).Use();
        }
    }
}