using System;

namespace BeatLeader.Models {
    internal class WeakRefWrapper<T> {
        public WeakRefWrapper(T value) {
            Object = value;
        }
        
        ~WeakRefWrapper() {
            ObjectDestroyedEvent?.Invoke();
        }
        
        public T Object { get; }

        public event Action? ObjectDestroyedEvent;
    }
}