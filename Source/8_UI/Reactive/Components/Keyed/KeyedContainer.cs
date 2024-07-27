using System.Collections.Generic;

namespace BeatLeader.UI.Reactive.Components {
    internal class KeyedContainer<TKey> : ReactiveComponent {
        #region Setup

        public IReactiveComponent? DummyView {
            get => _dummyView;
            set {
                _dummyView?.Use(null);
                _dummyView = value;
                _dummyView?.WithRectExpand().Use(ContentTransform);
            }
        }

        public IKeyedControlComponent<TKey>? Control {
            get => _control;
            set {
                if (_control != null) {
                    _control.SelectedKeyChangedEvent -= HandleSelectedKeyChanged;
                }
                _control = value;
                if (_control != null) {
                    _control.SelectedKeyChangedEvent += HandleSelectedKeyChanged;
                }
            }
        }

        public IDictionary<TKey, IReactiveComponent> Items => _items;

        private readonly ObservableDictionary<TKey, IReactiveComponent> _items = new();
        private IKeyedControlComponent<TKey>? _control;
        private IReactiveComponent? _selectedComponent;
        private IReactiveComponent? _dummyView;

        public bool Select(TKey? key) {
            if (_selectedComponent != null) {
                _selectedComponent.Enabled = false;
            }
            var validKey = false;
            if (key != null && _items.TryGetValue(key, out var value)) {
                _selectedComponent = value;
                _selectedComponent.Enabled = true;
                validKey = true;
            }
            if (_dummyView != null) {
                _dummyView.Enabled = !validKey;
            }
            return validKey;
        }

        protected override void OnInitialize() {
            _items.ItemAddedEvent += HandleItemAdded;
            _dummyView = new Label {
                Text = "Unfortunately monke have nothing to show here"
            };
            _dummyView.WithRectExpand().Use(ContentTransform);
        }

        #endregion

        #region Callbacks

        private void HandleSelectedKeyChanged(TKey key) {
            Select(key);
        }

        private void HandleItemAdded(TKey key, IReactiveComponent component) {
            component.WithRectExpand().Use(ContentTransform);
            component.Enabled = false;
            if (_selectedComponent != null) return;
            HandleSelectedKeyChanged(key);
        }

        #endregion
    }
}