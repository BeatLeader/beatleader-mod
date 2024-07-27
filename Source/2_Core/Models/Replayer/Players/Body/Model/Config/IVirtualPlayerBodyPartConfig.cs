using System;

namespace BeatLeader.Models {
    public interface IVirtualPlayerBodyPartConfig {
        bool PotentiallyActive { get; set; }
        bool ControlledByMask { get; }
        bool Active { get; }
        float Alpha { get; set; }

        event Action? ConfigUpdatedEvent;
    }
}