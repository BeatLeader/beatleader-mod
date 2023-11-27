using System;
using System.Collections.Generic;

namespace BeatLeader.Models {
    public interface IVirtualPlayersManager {
        IReadOnlyList<IVirtualPlayer> Players { get; }
        IVirtualPlayer PriorityPlayer { get; }

        event Action<IVirtualPlayer> PriorityPlayerWasChangedEvent;

        void SetPriorityPlayer(IVirtualPlayer player);
    }
}
