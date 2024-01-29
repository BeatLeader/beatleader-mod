using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace BeatLeader.Models {
    [PublicAPI]
    public class BodySettings {
        [JsonProperty]
        private Dictionary<string, VirtualPlayerBodyConfig> BodyModels { get; set; } = new();

        public void AddOrUpdateConfig(VirtualPlayerBodyConfig config) {
            BodyModels[config.Name] = config;
        }
        
        public VirtualPlayerBodyConfig? GetConfigByNameOrNull(string name) {
            BodyModels.TryGetValue(name, out var model);
            return model;
        }
    }
}