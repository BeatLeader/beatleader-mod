using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class TextListControl<TKey> : ListControl<TKey, string, TextKeyedControlComponentCell<TKey>> { }

    internal class TextKeyedControlComponentCell<TKey> : KeyedControlComponentCell<TKey, string> {
        public Label Label => _button.Label;

        private TextButton _button = null!;

        public override void OnInit(TKey key, string param) {
            Label.Text = param;
        }

        public override void OnCellStateChange(bool selected) {
            _button.Click(selected);
        }

        protected override GameObject Construct() {
            return new TextButton {
                GrowOnHover = false,
                Sticky = true,
                Colors = UIStyle.TextColorSet,
                Label = {
                    FontSizeMin = 2f,
                    FontSizeMax = 5f,
                    EnableAutoSizing = true
                }
            }.WithStateListener(_ => SelectSelf()).Bind(ref _button).Use();
        }
    }
}