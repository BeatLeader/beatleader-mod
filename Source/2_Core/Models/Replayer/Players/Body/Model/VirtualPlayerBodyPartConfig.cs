using System;

namespace BeatLeader.Models {
    public class VirtualPlayerBodyPartConfig {
        public VirtualPlayerBodyPartConfig(IVirtualPlayerBodyPartModel model) {
            Id = model.Id;
            _supportsAlpha = model.HasAlphaSupport;
        }

        public string Id { get; }

        public float Alpha {
            get => _supportsAlpha ? _alpha : throw new InvalidOperationException("The bound model does not support alpha");
            set {
                _alpha = value;
                NotifyConfigUpdated();
            }
        }

        public event Action? ConfigUpdatedEvent;

        private readonly bool _supportsAlpha;
        private float _alpha;

        private void NotifyConfigUpdated() {
            ConfigUpdatedEvent?.Invoke();
        }
    }
}