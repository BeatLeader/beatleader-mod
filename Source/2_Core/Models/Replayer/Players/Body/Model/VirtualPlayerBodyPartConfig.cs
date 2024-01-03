using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatLeader.Models {
    public class VirtualPlayerBodyPartConfig {
        public VirtualPlayerBodyPartConfig(IVirtualPlayerBodyPartModel model) {
            Id = model.Id;
            _availableSegments = model.Segments.ToHashSet();
        }

        public IReadOnlyCollection<IVirtualPlayerBodyPartSegmentModel> AvailableSegments => _availableSegments;
        public IReadOnlyCollection<IVirtualPlayerBodyPartSegmentModel> ActiveSegments => _activeSegments;
        public string Id { get; }

        public event Action? ConfigUpdatedEvent;

        private readonly HashSet<IVirtualPlayerBodyPartSegmentModel> _availableSegments;
        private readonly HashSet<IVirtualPlayerBodyPartSegmentModel> _activeSegments = new();

        public void SetSegmentActive(IVirtualPlayerBodyPartSegmentModel model, bool active) {
            if (!_availableSegments.Contains(model)) {
                throw new InvalidOperationException("Unable to manage the model which does not belong to the part");
            }
            if (active) {
                _activeSegments.Add(model);
            } else {
                _activeSegments.Remove(model);
            }
            NotifyConfigUpdated();
        }

        private void NotifyConfigUpdated() {
            ConfigUpdatedEvent?.Invoke();
        }
    }
}