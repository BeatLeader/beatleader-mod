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
            Action? allRemovedCallback = null
        ) {
            this.collection = collection;
            ItemAddedEvent += addedCallback;
            ItemRemovedEvent += removedCallback;
            AllItemsRemovedEvent += allRemovedCallback;
        }

        public event Action<T>? ItemAddedEvent; 
        public event Action<T>? ItemRemovedEvent; 
        public event Action? AllItemsRemovedEvent; 
        
        public readonly ICollection<T> collection;

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
            collection.Add(item);
            ItemAddedEvent?.Invoke(item);
        }

        public void Clear() {
            collection.Clear();
            AllItemsRemovedEvent?.Invoke();
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