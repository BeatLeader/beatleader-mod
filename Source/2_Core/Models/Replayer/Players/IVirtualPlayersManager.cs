using System;
using System.Collections.Generic;

namespace BeatLeader.Models {
    public interface IVirtualPlayersManager {
        VirtualPlayerConfig PrimaryPlayerConfig { get; }
        VirtualPlayerConfig PlayerConfig { get; }
        
        IReadOnlyList<IVirtualPlayer> Players { get; }
        IVirtualPlayer PrimaryPlayer { get; }

        event Action<IVirtualPlayer> PrimaryPlayerWasChangedEvent;

        void SetPrimaryPlayer(IVirtualPlayer player);
    }
}