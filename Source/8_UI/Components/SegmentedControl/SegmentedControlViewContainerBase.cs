using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatLeader.Components {
    /// <summary>
    /// Abstraction for segmented control container view
    /// </summary>
    internal interface ISegmentedControlView {
        void SetActive(bool active);
        void Setup(Transform? transform);
    }

    /// <summary>
    /// Abstraction for segmented control container view with Key and Value
    /// </summary>
    internal interface ISegmentedControlView<out TKey, out TValue> : ISegmentedControlView {
        TKey Key { get; }
        TValue Value { get; }
    }

    /// <summary>
    /// Abstraction for view source
    /// </summary>
    /// <typeparam name="TKey">Item key</typeparam>
    /// <typeparam name="TValue">Item value</typeparam>
    internal interface ISegmentedControlDataSource<TKey, TValue> {
        IReadOnlyDictionary<TKey, TValue> Items { get; }

        void OnItemSelect(TKey key);
    }

    internal abstract class SegmentedControlContainerBase<T, TKey, TValue> : LayoutComponentBase<T>, ISegmentedControlDataSource<TKey, TValue> where T : ReeUIComponentV3<T> {
        public TKey? SelectedKey {
            get => _selectedKey;
            set {
                _selectedKey = value;
                Refresh();
            }
        }

        public IReadOnlyDictionary<TKey, TValue> Items => _keyNValues;

        private readonly Dictionary<TKey, ISegmentedControlView<TKey, TValue>> _views = new();
        private readonly Dictionary<TKey, TValue> _keyNValues = new();
        private ISegmentedControlView<TKey, TValue>? _selectedView;
        private TKey? _selectedKey;
        
        public void AddViewsFromChildren() {
            _views.Clear();
            foreach (var view in ContentTransform.GetReeComponentsInChildren<ISegmentedControlView<TKey, TValue>>()) {
                AddView(view);
            }
        }

        public void AddView(ISegmentedControlView<TKey, TValue> view) {
            _views.Add(view.Key, view);
            _keyNValues.Add(view.Key, view.Value);
            view.Setup(ContentTransform);
            view.SetActive(false);
            Refresh();
        }

        public void RemoveView(ISegmentedControlView<TKey, TValue> view) {
            if (view.Key?.Equals(SelectedKey) ?? false) {
                _selectedKey = default;
            }
            _views.Remove(view.Key);
            _keyNValues.Remove(view.Key);
            view.Setup(null);
            view.SetActive(false);
            Refresh();
        }
        
        public void Refresh() {
            if (_views.Count is 0) return;
            var view = SelectedKey is { } v ? _views[v] : _views.Values.First();
            SwitchView(view);
        }

        void ISegmentedControlDataSource<TKey, TValue>.OnItemSelect(TKey key) {
            SelectedKey = key;
        }
        
        private void SwitchView(ISegmentedControlView<TKey, TValue> segmentedControlView) {
            if (_selectedView is { } selView) {
                selView.SetActive(false);
            }
            segmentedControlView.SetActive(true);
            _selectedView = segmentedControlView;
        }
    }
}