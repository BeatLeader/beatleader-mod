using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reactive;

namespace BeatLeader.Models {
    /// <summary>
    /// A settings class that is designed to hold configs of different types.
    /// Such approach allows us to remember settings from another mods without overriding own settings.
    /// </summary>
    [PublicAPI]
    public class BodySettings {
        public event Action<BodySettings, object>? ConfigUpdatedEvent;
        
        [JsonProperty("Configs")]
        private readonly Dictionary<Type, string> _serializedConfigs = new();
        
        internal readonly Dictionary<Type, object> Configs = new();

        public void SetConfig<T>(T config) {
            Configs[typeof(T)] = config!;
            NotifyConfigUpdated(config!);
        }

        public T? GetConfig<T>() {
            if (!Configs.TryGetValue(typeof(T), out var obj)) {
                return default;
            }

            return (T)obj;
        }

        public T RequireConfig<T>() where T : new() {
            if (!Configs.TryGetValue(typeof(T), out var obj)) {
                var instance = new T();

                SetConfig(instance);
                return instance;
            }

            return (T)obj;
        }

        public void NotifyConfigUpdated(object config) {
            ConfigUpdatedEvent?.Invoke(this, config);
        }

        [OnSerializing]
        internal void OnSerializing(StreamingContext context) {
            foreach (var (key, value) in Configs) {
                _serializedConfigs[key] = JsonConvert.SerializeObject(value);
            }
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context) {
            foreach (var (key, value) in _serializedConfigs) {
                Configs[key] = JsonConvert.DeserializeObject(value, key)!;
            }
        }
    }
}