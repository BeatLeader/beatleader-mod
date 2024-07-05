using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class TextListControl<TKey> : ListControl<TKey, string, TextKeyedControlComponentCell<TKey>> { }

    internal class TextKeyedControlComponentCell<TKey> : KeyedControlComponentCell<TKey, string>, IPreviewableCell {
        public bool UsedAsPreview {
            set {
                _button.Interactable = !value;
            }
        }

        public Label Label => _button.Label;

        private TextButton _button = null!;

        public override void OnInit(TKey key, string param) {
            Label.Text = param;
        }

        public override void OnCellStateChange(bool selected) {
            _button.Click(selected);
        }

        protected override GameObject Construct() {
            var colorSet = UIStyle.TextColorSet;
            colorSet.DisabledColor = colorSet.Color;
            return new TextButton {
                GrowOnHover = false,
                Sticky = true,
                Colors = colorSet,
                Label = {
                    FontSizeMin = 2f,
                    FontSizeMax = 5f,
                    EnableAutoSizing = true
                }
            }.WithStateListener(_ => SelectSelf()).Bind(ref _button).Use();
        }
    }
}