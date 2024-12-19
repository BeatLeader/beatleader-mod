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

        public ChristmasTreeOrnament Spawn(int id, Transform? parent, Vector3 pos, bool localPosStays) {
            ChristmasTreeOrnament ornament;
            var cache = GetCache(id);
            if (cache.Count > 0) {
                ornament = cache.Pop();
                // True to keep the size
                ornament.transform.SetParent(parent, localPosStays);
            } else {
                // supposing that it was previously loaded (sorry, but I don't have any time to implement it in a proper way)
                var prefab = ChristmasOrnamentLoader.LoadOrnamentPrefabAsync(id).Result;
                var go = Object.Instantiate(prefab, parent, localPosStays);
                ornament = go.AddComponent<ChristmasTreeOrnament>();
                ornament.Setup(_tree, id);
            }
            ornament.transform.localPosition = pos;
            ornament.OrnamentDeinitEvent += Despawn;
            ornament.Init();
            return ornament;
        }

        public void Despawn(ChristmasTreeOrnament ornament) {
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