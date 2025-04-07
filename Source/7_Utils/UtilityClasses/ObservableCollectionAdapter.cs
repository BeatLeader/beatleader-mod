using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BeatLeader {
    internal class ObservableSet<T> : ObservableCollectionAdapter<T> {
        public ObservableSet(
            Action<T>? addedCallback = null,
            Action<T>? removedCallback = null,
            Action<IEnumerable<T>>? allRemovedCallback = null
        ) : base(
            new HashSet<T>(),
            addedCallback,
            removedCallback,
            allRemovedCallback
        ) { }
    }

    internal class ObservableCollectionAdapter<T> : ICollection<T>, IReadOnlyCollection<T> {
        #region Adapter

        public ObservableCollectionAdapter(
            ICollection<T> collection,
            Action<T>? addedCallback = null,
            Action<T>? removedCallback = null,
            Action<IEnumerable<T>>? allRemovedCallback = null
        ) {
            _collection = collection;
            ItemAddedEvent += addedCallback;
            ItemRemovedEvent += removedCallback;
            AllItemsRemovedEvent += allRemovedCallback;
        }

        public event Action<T>? ItemAddedEvent;
        public event Action<T>? ItemRemovedEvent;
        public event Action<IEnumerable<T>>? AllItemsRemovedEvent;
        
        public ICollection<T> Collection => _pointToEmptyArray ? Array.Empty<T>() : _collection;
        
        private readonly ICollection<T> _collection;
        private bool _pointToEmptyArray;

        #endregion

        #region Collection
        
        public int Count => Collection.Count;
        
        public bool IsReadOnly => Collection.IsReadOnly;

        public IEnumerator<T> GetEnumerator() {
            return Collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)Collection).GetEnumerator();
        }

        public void Add(T item) {
            var count = Count;
            Collection.Add(item);
            if (count == Count) return;
            ItemAddedEvent?.Invoke(item);
        }

        public void Clear() {
            _pointToEmptyArray = true;

            AllItemsRemovedEvent?.Invoke(Collection);
            Collection.Clear();

            _pointToEmptyArray = false;
        }

        public bool Contains(T item) {
            return Collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            Collection.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            var res = Collection.Remove(item);
            if (res) ItemRemovedEvent?.Invoke(item);
            return res;
        }

        #endregion
    }
}