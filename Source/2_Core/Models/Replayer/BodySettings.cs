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
        public event Action<BodySettings>? SettingsUpdatedEvent;
        
        [JsonProperty("Configs")]
        private readonly Dictionary<Type, string> _serializedConfigs = new();
        
        internal readonly Dictionary<Type, object> Configs = new();

        public void SetConfig<T>(T config) {
            Configs[typeof(T)] = config!;
            NotifySettingsUpdated();
        }

        public T? GetConfig<T>() {
            if (!Configs.TryGetValue(typeof(T), out var obj)) {
                return default;
            }

            return (T)obj;
        }

        public void NotifySettingsUpdated() {
            SettingsUpdatedEvent?.Invoke(this);
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