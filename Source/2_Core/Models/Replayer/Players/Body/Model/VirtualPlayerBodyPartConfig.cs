using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    public class VirtualPlayerBodyPartConfig {
        [JsonConstructor, UsedImplicitly]
        private VirtualPlayerBodyPartConfig() { }

        public VirtualPlayerBodyPartConfig(IVirtualPlayerBodyPartModel model) {
            Id = model.Id;
        }

        [JsonProperty]
        public string Id { get; private set; } = null!;

        [JsonProperty]
        public bool Active {
            get => _active;
            set {
                _active = value;
                NotifyConfigUpdated();
            }
        }

        [JsonProperty]
        public float Alpha {
            get => _alpha;
            set {
                _alpha = value;
                NotifyConfigUpdated();
            }
        }

        public event Action? ConfigUpdatedEvent;

        private bool _active = true;
        private float _alpha;

        private void NotifyConfigUpdated() {
            ConfigUpdatedEvent?.Invoke();
        }
    }
}