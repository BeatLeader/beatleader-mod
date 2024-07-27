using System;
using System.Collections;
using System.Collections.Generic;

namespace BeatLeader {
    internal class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
        #region Logic

        public ObservableDictionary() {
            _dictionary = new();
        }

        public ObservableDictionary(Dictionary<TKey, TValue> dictionary) {
            _dictionary = dictionary;
        }

        public event Action<TKey, TValue>? ItemAddedEvent;
        public event Action<TKey>? ItemRemovedEvent;
        public event Action? AllItemsRemovedEvent;

        private readonly Dictionary<TKey, TValue> _dictionary;

        #endregion

        #region Adapter

        public TValue this[TKey key] {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _dictionary.Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => _dictionary.Values;

        public Dictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;
        public Dictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;
        
        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        public void Add(TKey key, TValue value) {
            _dictionary.Add(key, value);
            ItemAddedEvent?.Invoke(key, value);
        }

        public bool Remove(TKey key) {
            var result = _dictionary.Remove(key);
            ItemRemovedEvent?.Invoke(key);
            return result;
        }

        public void Clear() {
            _dictionary.Clear();
            AllItemsRemovedEvent?.Invoke();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Add(item);
            ItemAddedEvent?.Invoke(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}