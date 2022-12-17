using BeatLeader.Replayer.Emulation;
using System;
using System.Collections.Generic;

namespace BeatLeader.Models {
    public interface IVirtualPlayersManager {
        IReadOnlyList<VirtualPlayer> Players { get; }
        VirtualPlayer PriorityPlayer { get; }

        event Action<VirtualPlayer> PriorityPlayerWasChangedEvent;

        void Spawn(Player player, Replay replay);
        void Despawn(VirtualPlayer player);
    }
}
