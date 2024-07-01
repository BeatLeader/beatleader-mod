using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    [PublicAPI]
    public class BodySettings {
        [JsonProperty]
        private Dictionary<string, SerializableVirtualPlayerBodyConfig> BodyModels { get; set; } = new();

        public void AddOrUpdateConfig(IVirtualPlayerBodyModel model, IVirtualPlayerBodyConfig config) {
            BodyModels[model.Name] = new SerializableVirtualPlayerBodyConfig(model, config);
        }

        public IVirtualPlayerBodyConfig? GetConfigByNameOrNull(string name) {
            BodyModels.TryGetValue(name, out var model);
            return model;
        }
    }
}