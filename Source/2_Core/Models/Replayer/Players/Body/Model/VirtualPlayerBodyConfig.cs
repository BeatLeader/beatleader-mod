using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;

namespace BeatLeader.Models {
    public class VirtualPlayerBodyConfig {
        public VirtualPlayerBodyConfig(IVirtualPlayerBodyModel model) {
            _availableBodyParts = model.Parts.Select(static x => new VirtualPlayerBodyPartConfig(x)).ToHashSet();
            _availableBodyParts.ForEach(x => x.ConfigUpdatedEvent += NotifyConfigUpdated);
            _activeBodyParts.AddRange(_availableBodyParts);
        }

        ~VirtualPlayerBodyConfig() {
            _availableBodyParts.ForEach(x => x.ConfigUpdatedEvent -= NotifyConfigUpdated);
        }

        public IReadOnlyCollection<VirtualPlayerBodyPartConfig> AvailableBodyParts => _availableBodyParts;
        public IReadOnlyCollection<VirtualPlayerBodyPartConfig> ActiveBodyParts => _activeBodyParts;

        public event Action? ConfigUpdatedEvent;
        
        private readonly HashSet<VirtualPlayerBodyPartConfig> _availableBodyParts;
        private readonly HashSet<VirtualPlayerBodyPartConfig> _activeBodyParts = new();

        public bool IsPartActive(VirtualPlayerBodyPartConfig config) {
            return _activeBodyParts.Contains(config);
        } 
        
        public void SetBodyPartActive(VirtualPlayerBodyPartConfig partConfig, bool active) {
            if (!_availableBodyParts.Contains(partConfig)) {
                throw new InvalidOperationException("Unable to manage the part which does not belong to the model");
            }
            if (active) {
                _activeBodyParts.Add(partConfig);
            } else {
                _activeBodyParts.Remove(partConfig);
            }
            NotifyConfigUpdated();
        }

        private void NotifyConfigUpdated() {
            ConfigUpdatedEvent?.Invoke();
        }
    }
}