using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    [PublicAPI]
    public class BodySettings {
        [JsonProperty]
        internal Dictionary<string, SerializableVirtualPlayerBodyConfig> BodyModels { get; set; } = new();

        public void AddOrUpdateConfig(IVirtualPlayerBodyModel model, IVirtualPlayerBodyConfig config) {
            var serializable = config as SerializableVirtualPlayerBodyConfig;
            BodyModels[model.Name] = serializable ?? SerializableVirtualPlayerBodyConfig.Clone(model, config);
        }

        public IVirtualPlayerBodyConfig GetConfigByModel(IVirtualPlayerBodyModel bodyModel) {
            if (!BodyModels.TryGetValue(bodyModel.Name, out var config)) {
                config = SerializableVirtualPlayerBodyConfig.Create(bodyModel);
                config.SetBodyModel(bodyModel);
            }
            config.SetBodyModel(bodyModel);
            return config;
        }
    }
}