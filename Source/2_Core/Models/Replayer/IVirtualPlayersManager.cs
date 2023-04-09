using BeatLeader.Replayer.Emulation;
using System;
using System.Collections.Generic;
using BeatLeader.Models.AbstractReplay;

namespace BeatLeader.Models {
    public interface IVirtualPlayersManager {
        IReadOnlyList<VirtualPlayer> Players { get; }
        VirtualPlayer? PriorityPlayer { get; }

        event Action<VirtualPlayer> PriorityPlayerWasChangedEvent;

        void Spawn(IReplay replay);
        void Despawn(VirtualPlayer player);
    }
}
