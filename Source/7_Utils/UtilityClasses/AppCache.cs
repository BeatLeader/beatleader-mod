using System;
using System.IO;
using IPA.Utilities;
using Newtonsoft.Json;

namespace BeatLeader {
    internal class AppCache<T> where T : new() {
        public AppCache(string path) {
            _path = Path.Combine(basePath, path);
        }

        public T Cache {
            get {
                Load();
                return _cache!;
            }
        }

        private static readonly string basePath = Path.Combine(UnityGame.UserDataPath, "BeatLeader");
        
        private readonly string _path;
        private T? _cache;
        private bool _initialized;

        public void Load() {
            if (_initialized) return;
            try {
                if (!File.Exists(_path)) {
                    _cache = new();
                    _initialized = true;
                    return;
                }
                var content = File.ReadAllText(_path);
                _cache = JsonConvert.DeserializeObject<T>(content);
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to initialize cache ({typeof(T).Name})\n{ex}");
            }
            _cache ??= new();
            _initialized = true;
        }

        public void Save() {
            if (!_initialized) return;
            try {
                var ser = JsonConvert.SerializeObject(_cache, Formatting.Indented);
                File.WriteAllText(_path, ser);
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to save cache ({typeof(T).Name})\n{ex}");
            }
        }
    }
}