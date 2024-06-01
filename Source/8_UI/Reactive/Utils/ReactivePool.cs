using System;
using System.Collections.Generic;
using BeatLeader.Utils;

namespace BeatLeader.UI.Reactive {
    internal class ReactivePool<TKey, T> where T : IReactiveComponent, new() {
        public IReadOnlyDictionary<TKey, T> SpawnedComponents => _keyedComponents;

        private readonly ReactivePool<T> _reactivePool = new();
        private readonly Dictionary<TKey, T> _keyedComponents = new();

        public T Get(TKey key) {
            if (!SpawnedComponents.TryGetValue(key, out var value)) {
                value = Spawn(key);
            }
            return value;
        }

        public T Spawn(TKey key) {
            var comp = _reactivePool.Spawn();
            _keyedComponents.Add(key, comp);
            return comp;
        }

        public void DespawnAll() {
            _reactivePool.DespawnAll();
        }

        public void TryDespawn(TKey key) {
            if (_keyedComponents.ContainsKey(key)) {
                Despawn(key);
            }
        }

        public void Despawn(TKey key) {
            var comp = _keyedComponents[key];
            _keyedComponents.Remove(key);
            _reactivePool.Despawn(comp);
        }

        public void Despawn(T comp) {
            _keyedComponents.RemoveValue(comp);
            _reactivePool.Despawn(comp);
        }
    }

    internal class ReactivePool<T> where T : IReactiveComponent, new() {
        public IReadOnlyCollection<T> SpawnedComponents => _spawnedComponents;

        private readonly Stack<T> _reservedComponents = new();
        private readonly HashSet<T> _spawnedComponents = new();

        public T Spawn() {
            if (!_reservedComponents.TryPop(out var comp)) {
                comp = new();
            }
            if (comp!.IsDestroyed) {
                return Spawn();
            }
            _spawnedComponents.Add(comp);
            comp.Enabled = true;
            return comp;
        }

        public void DespawnAll() {
            foreach (var comp in _spawnedComponents) {
                DespawnInternal(comp);
            }
        }

        public void Despawn(T comp) {
            if (!_spawnedComponents.Contains(comp)) {
                throw new InvalidOperationException("Cannot despawn an item that does not belong to the pool");
            }
            DespawnInternal(comp);
        }

        private void DespawnInternal(T comp) {
            if (!comp.IsDestroyed) {
                comp.Enabled = false;
                comp.Use(null);
                _reservedComponents.Push(comp);
            }
            _spawnedComponents.Remove(comp);
        }
    }
}