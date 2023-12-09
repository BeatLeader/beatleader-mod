using System;
using System.Collections.Generic;

namespace BeatLeader.Models {
    public interface IVirtualPlayersManager {
        IReadOnlyList<IVirtualPlayer> Players { get; }
        IVirtualPlayer PrimaryPlayer { get; }

        event Action<IVirtualPlayer> PrimaryPlayerWasChangedEvent;

        void SetPrimaryPlayer(IVirtualPlayer player);
    }
}
