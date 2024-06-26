using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class NamedRail : ReactiveComponent {
        public Label Label => _label;

        public ILayoutItem? Component {
            get => _component;
            set {
                if (_component != null) {
                    _container.Children.Remove(_component);
                }
                _component = value;
                if (_component != null) {
                    _component.AsFlexItem();
                    _container.Children.Add(_component);
                }
            }
        }

        private ILayoutItem? _component;
        private Label _label = null!;
        private Dummy _container = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new Label {
                        Text = "Oops, text is missing"
                    }.AsFlexItem(size: "auto", alignSelf: Align.Center).Bind(ref _label),
                }
            }.AsFlexGroup(
                justifyContent: Justify.SpaceBetween
            ).Bind(ref _container).Use();
        }
        
        protected override void OnInitialize() {
            this.AsFlexItem();
        }
    }
}