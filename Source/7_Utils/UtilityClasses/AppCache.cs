using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

        private readonly TaskCompletionSource<byte> _completionSource = new();
        private readonly string _path;
        private readonly JsonSerializerSettings? _serializerSettings;
        private T? _cache;
        private bool _initialized;
        private bool _isLoading;

        public Task WaitForLoading() {
            return _completionSource.Task;
        }

        public void LoadDetached() {
            if (_initialized || _isLoading) {
                return;
            }

            new Thread(Load) { IsBackground = true }.Start();
            Plugin.Log.Info($"Got past Task.Run {typeof(T)}");
        }

        public void Load() {
            if (_initialized || _isLoading) {
                return;
            }

            _isLoading = true;
            Plugin.Log.Info($"Thread {Thread.CurrentThread.ManagedThreadId} {typeof(T)}");

            try {
                if (File.Exists(_path)) {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var content = File.ReadAllText(_path);
                    Plugin.Log.Info($"Reading file took: {stopwatch.Elapsed} {typeof(T)}");
                    stopwatch.Restart();

                    _cache = JsonConvert.DeserializeObject<T>(content, _serializerSettings);

                    Plugin.Log.Info($"Parsing json took: {stopwatch.Elapsed} {typeof(T)}");
                }
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to initialize cache ({typeof(T).Name})\n{ex}");
            }

            _cache ??= new();

            _initialized = true;
            _isLoading = false;
            _completionSource.SetResult(0);
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