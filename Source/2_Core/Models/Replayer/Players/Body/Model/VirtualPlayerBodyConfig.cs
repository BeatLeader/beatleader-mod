using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BeatLeader.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    public class VirtualPlayerBodyConfig {
        #region Serialization

        [JsonConstructor, UsedImplicitly]
        private VirtualPlayerBodyConfig() { }
        
        [OnDeserialized]
        private void OnDeserialize(StreamingContext context) {
            BindConfigs();
        }

        #endregion

        public VirtualPlayerBodyConfig(IVirtualPlayerBodyModel model) {
            Name = model.Name;
            _bodyParts.AddRange(model.Parts.Select(static x => new VirtualPlayerBodyPartConfig(x)));
            BindConfigs();
        }

        ~VirtualPlayerBodyConfig() {
            UnbindConfigs();
        }

        [JsonIgnore]
        public IReadOnlyCollection<VirtualPlayerBodyPartConfig> BodyParts => _bodyParts;

        [JsonProperty]
        public string Name { get; private set; } = null!;

        public event Action? ConfigUpdatedEvent;
        
        [JsonProperty("BodyParts"), UsedImplicitly]
        private HashSet<VirtualPlayerBodyPartConfig> _bodyParts = new();

        private void NotifyConfigUpdated() {
            ConfigUpdatedEvent?.Invoke();
        }

        private void BindConfigs() {
            _bodyParts.ForEach(x => x.ConfigUpdatedEvent += NotifyConfigUpdated);
        }

        private void UnbindConfigs() {
            _bodyParts.ForEach(x => x.ConfigUpdatedEvent -= NotifyConfigUpdated);
        }
    }
}