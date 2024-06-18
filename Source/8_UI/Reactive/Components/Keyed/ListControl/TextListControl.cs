using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class TextListControl<TKey> : ListControl<TKey, string, TextKeyedControlComponentCell<TKey>> { }
    
    internal class TextKeyedControlComponentCell<TKey> : KeyedControlComponentCell<TKey, string> {
        public Label Label => _label;

        private Button _button = null!;
        private Label _label = null!;

        public override void OnInit(TKey key, string param) {
            _label.Text = param;
        }

        public override void OnCellStateChange(bool selected) {
            _button.Click(selected);
        }

        protected override GameObject Construct() {
            return new Button {
                GrowOnHover = false,
                Sticky = true,
                Children = {
                    new Label()
                        .WithRectExpand()
                        .Bind(ref _label)
                }
            }.WithStateListener(_ => SelectSelf()).Bind(ref _button).Use();
        }
    }
}