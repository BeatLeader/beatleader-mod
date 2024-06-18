using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    [PublicAPI]
    public class BodySettings {
        [JsonProperty]
        private Dictionary<string, SerializableVirtualPlayerBodyConfig> BodyModels { get; set; } = new();

        public void AddOrUpdateConfig(IVirtualPlayerBodyModel model, IVirtualPlayerBodyConfig config) {
            var serializable = config as SerializableVirtualPlayerBodyConfig;
            BodyModels[model.Name] = serializable ?? new SerializableVirtualPlayerBodyConfig(model, config);
        }

        public IVirtualPlayerBodyConfig? GetConfigByNameOrNull(string name) {
            BodyModels.TryGetValue(name, out var model);
            return model;
        }
    }
}