using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public interface IVirtualPlayersManager {
        IReadOnlyList<IVirtualPlayer> Players { get; }
        IVirtualPlayer PrimaryPlayer { get; }

        event Action<IVirtualPlayer>? PrimaryPlayerWasChangedEvent;

        void SetPrimaryPlayer(IVirtualPlayer player);
    }
}