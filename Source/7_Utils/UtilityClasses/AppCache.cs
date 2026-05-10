using System;
using System.IO;
using System.Threading.Tasks;
using BeatLeader.Utils;
using IPA.Utilities;
using Newtonsoft.Json;

namespace BeatLeader {
    internal class AppCache<T> where T : new() {
        public AppCache(string path, JsonSerializerSettings? settings = null) {
            _path = Path.Combine(basePath, path);
            _serializerSettings = settings;
        }

        public T Cache {
            get {
                Load();
                return _cache!;
            }
        }

        private static readonly string basePath = Path.Combine(UnityGame.UserDataPath, "BeatLeader");

        private readonly JsonSerializerSettings? _serializerSettings;
        private readonly string _path;
        private T? _cache;

        private Task? _saveTask;
        private Task? _loadTask;
        private bool _initialized;

        public Task WaitForLoading() {
            return _loadTask ?? Task.CompletedTask;
        }

        public void LoadDetached() {
            if (_initialized || _loadTask != null) {
                return;
            }

            _loadTask = Task.Run(() => Load(join: false)).RunCatching();
        }

        public void Load(bool join = true) {
            if (join && _loadTask != null) {
                _loadTask.Wait();
                return;
            }

            if (_initialized) {
                return;
            }

            try {
                if (File.Exists(_path)) {
                    var content = File.ReadAllText(_path);
                    _cache = JsonConvert.DeserializeObject<T>(content, _serializerSettings);
                    
                    Plugin.Log.Debug($"Loaded cache ({typeof(T).Name})");
                }
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to initialize cache ({typeof(T).Name})\n{ex}");
            }

            _cache ??= new();
            _initialized = true;
            _loadTask = null;
        }

        public void SaveDetached() {
            if (!_initialized || _saveTask != null) {
                return;
            }

            _saveTask = Task.Run(() => Save(join: false)).RunCatching();
        }

        public void Save(bool join = true) {
            if (join && _saveTask != null) {
                _saveTask.Wait();
                return;
            }

            if (!_initialized) {
                return;
            }

            try {
                var ser = JsonConvert.SerializeObject(_cache, Formatting.Indented);
                File.WriteAllText(_path, ser);

                Plugin.Log.Debug($"Saved cache ({typeof(T).Name})");
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to save cache ({typeof(T).Name})\n{ex}");
            }

            _saveTask = null;
        }
    }
}