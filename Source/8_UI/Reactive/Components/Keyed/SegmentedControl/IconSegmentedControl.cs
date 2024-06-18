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
                GrowOnHover = false,
                HoverLerpMul = float.MaxValue,
                Sticky = true,
                Colors = UIStyle.ButtonColorSet
            }.WithStateListener(_ => SelectSelf()).Bind(ref _button).Use();
        }
    }
}