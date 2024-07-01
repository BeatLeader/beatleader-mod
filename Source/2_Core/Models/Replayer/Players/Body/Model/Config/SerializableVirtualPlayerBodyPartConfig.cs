using System;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    internal class SerializableVirtualPlayerBodyPartConfig : IVirtualPlayerBodyPartConfig {
        #region Impl

        public bool PotentiallyActive {
            get => _potentiallyActive;
            set {
                _potentiallyActive = value;
                NotifyConfigUpdated();
            }
        }

        public float Alpha {
            get => _alpha;
            set {
                _alpha = value;
                NotifyConfigUpdated();
            }
        }

        [JsonIgnore]
        public bool ControlledByMask { get; private set; }

        [JsonIgnore]
        public bool Active => ControlledByMask ? !_maskEnabled : PotentiallyActive;

        public event Action? ConfigUpdatedEvent;

        #endregion

        #region Logic

        private SerializableVirtualPlayerBodyConfig? _bodyConfig;
        private bool _maskEnabled;
        private float _alpha;
        private bool _potentiallyActive = true;

        public void SetMaskEnabled(bool? enabled) {
            ControlledByMask = enabled != null;
            _maskEnabled = enabled ?? false;
            NotifyConfigUpdated();
        }

        public void SetBodyConfig(SerializableVirtualPlayerBodyConfig config) {
            _bodyConfig = config;
        }

        private void NotifyConfigUpdated() {
            ConfigUpdatedEvent?.Invoke();
            _bodyConfig?.NotifyConfigUpdated(this);
        }

        #endregion
    }
}