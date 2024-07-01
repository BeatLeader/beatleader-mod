using System;
using System.Collections;
using System.Collections.Generic;

namespace BeatLeader {
    internal class ObservableCollectionAdapter<T> : ICollection<T>, IReadOnlyCollection<T> {
        #region Adapter

        public ObservableCollectionAdapter(
            ICollection<T> collection,
            Action<T>? addedCallback = null,
            Action<T>? removedCallback = null,
            Action<IEnumerable<T>>? allRemovedCallback = null
        ) {
            this.collection = collection;
            ItemAddedEvent += addedCallback;
            ItemRemovedEvent += removedCallback;
            AllItemsRemovedEvent += allRemovedCallback;
        }

        public event Action<T>? ItemAddedEvent;
        public event Action<T>? ItemRemovedEvent;
        public event Action<IEnumerable<T>>? AllItemsRemovedEvent;

        public readonly ICollection<T> collection;

        //mono does not have spans, so no safe stack array allocations(
        private readonly List<T> _removalBuffer = new();

        #endregion

        #region Collection

        public int Count => collection.Count;

        public bool IsReadOnly => collection.IsReadOnly;

        public IEnumerator<T> GetEnumerator() {
            return collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)collection).GetEnumerator();
        }

        public void Add(T item) {
            var count = Count;
            collection.Add(item);
            if (count == Count) return;
            ItemAddedEvent?.Invoke(item);
        }

        public void Clear() {
            _removalBuffer.AddRange(collection);
            collection.Clear();
            AllItemsRemovedEvent?.Invoke(_removalBuffer);
            _removalBuffer.Clear();
        }

        public bool Contains(T item) {
            return collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            collection.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            var res = collection.Remove(item);
            if (res) ItemRemovedEvent?.Invoke(item);
            return res;
        }

        #endregion
    }
}