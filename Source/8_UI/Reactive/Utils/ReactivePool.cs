using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;

namespace BeatLeader.UI.Reactive {
    internal class ReactivePool<TKey, T> where T : IReactiveComponent, new() {
        public IReadOnlyDictionary<TKey, T> SpawnedComponents => _keyedComponents;

        public bool DetachOnDespawn {
            get => _reactivePool.DetachOnDespawn;
            set => _reactivePool.DetachOnDespawn = value;
        }

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
        public IReadOnlyList<T> SpawnedComponents => _spawnedComponents;
        public bool DetachOnDespawn = true;

        private readonly Stack<T> _reservedComponents = new();
        private readonly List<T> _spawnedComponents = new();

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
            foreach (var comp in _spawnedComponents.ToArray()) {
                DespawnInternal(comp);
            }
        }

        public void DespawnLast() {
            if (_spawnedComponents.Count == 0) return;
            var last = _spawnedComponents.Last();
            Despawn(last);
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
                if (DetachOnDespawn) comp.Use(null);
                _reservedComponents.Push(comp);
            }
            _spawnedComponents.Remove(comp);
        }
    }
}