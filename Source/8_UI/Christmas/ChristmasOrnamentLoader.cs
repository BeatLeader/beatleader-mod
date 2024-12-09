using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatLeader.Utils;
using BGLib.UnityExtension;
using UnityEngine;

namespace BeatLeader {
    internal static class ChristmasOrnamentLoader {
        private static readonly Dictionary<int, GameObject?> prefabs = new();
        private static readonly Dictionary<int, Task> tasks = new();

        public static async Task<GameObject> LoadOrnamentInstanceAsync(int bundleId) {
            await EnsureOrnamentPrefabLoaded(bundleId);
            var prefab = prefabs[bundleId];
            if (prefab == null) {
                prefab = BundleLoader.MonkeyPrefab;
            }
            //TODO: remove
            prefab.transform.localScale *= 0.2f;
            return UnityEngine.Object.Instantiate(prefab, null, false);
        }

        public static async Task EnsureOrnamentPrefabLoaded(int id) {
            if (!tasks.TryGetValue(id, out var task)) {
                if (prefabs.ContainsKey(id)) {
                    return;
                }
                task = LoadOrnamentPrefabAsync(id);
                tasks[id] = task;
            }
            await task;
        }

        private static async Task LoadOrnamentPrefabAsync(int id) {
            prefabs[id] = null;

            Plugin.Log.Info($"Loading ornament bundle {id}.");

            var path = $"https://cdn.assets.beatleader.xyz/project_tree_ornament_{id}.bundle";
            var res = await WebUtils.HttpClient.GetAsync(path);

            if (!res.IsSuccessStatusCode) {
                Plugin.Log.Error($"Failed to download ornament from {path}: {res.StatusCode}");
                return;
            }

            try {
                using (var stream = await res.Content.ReadAsStreamAsync()) {
                    var bundle = await AssetBundle.LoadFromStreamAsync(stream);

                    var prefab = await bundle.LoadAllAssetsAsync<GameObject>();
                    if (prefab == null) {
                        throw new Exception("Prefab is null");
                    }
                    prefabs[id] = (GameObject)prefab;
                    Plugin.Log.Info($"Loaded ornament {id}.");

                    bundle.Unload(false);
                }
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to load ornament: {ex}");
            }
            
            tasks.Remove(id);
        }
    }
}