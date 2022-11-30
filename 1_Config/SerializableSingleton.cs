using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BeatLeader {
    internal interface ISerializableSingleton {
        void Save();
        void Load();
    }

    internal static class SerializableSingletons {
        private static readonly HashSet<ISerializableSingleton> _registeredSingletons = new();

        public static void RegisterSingleton(ISerializableSingleton singleton) {
            _registeredSingletons.Add(singleton);
        }
        public static bool UnregisterSingleton(ISerializableSingleton singleton) {
            return _registeredSingletons.Remove(singleton);
        }

        public static void SaveAll() {
            foreach (var item in _registeredSingletons) {
                try {
                    item.Save();
                    Plugin.Log.Info("Successfully saved " + item.GetType().Name);
                } catch (Exception ex) {
                    Plugin.Log.Error($"Failed to serialize {item.GetType()}! \r\n{ex}");
                }
            }
        }
    }

    internal abstract class SerializableSingleton<T> : ISerializableSingleton {
        private const string ConfigsPath = @"UserData\BeatLeader\Configs\";

        public SerializableSingleton() {
            SerializableSingletons.RegisterSingleton(this);
        }
        ~SerializableSingleton() {
            SerializableSingletons.UnregisterSingleton(this);
        }

        public static T Instance {
            get {
                LoadInternal(_configPath);
                return _instance;
            }
        }
        public static bool IsSingletonAvailable => _instance != null;

        private static readonly string _configPath = $"{ConfigsPath}{typeof(T).Name}.json";
        private static T _instance;

        public void Save() {
            SaveInternal(_configPath, Instance);
        }
        public void Load() {
            LoadInternal(_configPath);
        }

        private static void LoadInternal(string path) {
            if (_instance != null) return;
            if (TryGetContent(path, out var content)) {
                try {
                    _instance = JsonConvert.DeserializeObject<T>(content);
                } catch (Exception ex) {
                    Plugin.Log.Error($"Failed to load {typeof(T).Name}! \r\n{ex.Message}");
                }
            }
            _instance ??= Activator.CreateInstance<T>();
        }
        private static void SaveInternal(string path, object instance) {
            try {
                File.WriteAllText(path, JsonConvert.SerializeObject(instance, Formatting.Indented));
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to save {typeof(T).Name}! \r\n{ex.Message}");
            }
        }

        private static bool TryGetContent(string path, out string content) {
            return !string.IsNullOrEmpty(content = File.Exists(path) ? File.ReadAllText(path) : string.Empty);
        }
    }
}
