using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ChristmasOrnamentPool {
        public ChristmasOrnamentPool(ChristmasTree tree) {
            _tree = tree;
        }

        private readonly Dictionary<int, Stack<ChristmasTreeOrnament>> _groupedOrnaments = new Dictionary<int, Stack<ChristmasTreeOrnament>>();
        private readonly ChristmasTree _tree;

        public async Task PreloadAsync(int bundleId) {
            await ChristmasOrnamentLoader.EnsureOrnamentPrefabLoaded(bundleId);
        }

        public ChristmasTreeOrnament Spawn(int id) {
            ChristmasTreeOrnament ornament;
            var cache = GetCache(id);
            if (cache.Count > 0) {
                ornament = cache.Pop();
            } else {
                // supposing that it was previously loaded (sorry, but I don't have any time to implement it in a proper way)
                var prefab = ChristmasOrnamentLoader.LoadOrnamentPrefabAsync(id).Result;
                var go = Object.Instantiate(prefab);
                ornament = go.AddComponent<ChristmasTreeOrnament>();
                ornament.Setup(_tree, id);
            }

            ornament.OrnamentDeinitEvent += Despawn;
            ornament.Init();
            return ornament;
        }

        public void Despawn(ChristmasTreeOrnament ornament) {
            ornament.OrnamentDeinitEvent -= Despawn;
            var cache = GetCache(ornament.BundleId);
            cache.Push(ornament);
        }

        private Stack<ChristmasTreeOrnament> GetCache(int id) {
            if (!_groupedOrnaments.TryGetValue(id, out var cache)) {
                cache = new Stack<ChristmasTreeOrnament>();
                _groupedOrnaments[id] = cache;
            }

            return cache;
        }
    }
}