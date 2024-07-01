using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    //TODO: split into replay loader and metadata manager, keep cached replay here
    public interface IReplayManager {
        event Action<IReplayHeader>? ReplayAddedEvent;
        event Action<IReplayHeader>? ReplayDeletedEvent;
        event Action? AllReplaysDeletedEvent;

        IReplayHeader? CachedReplay { get; }
        IReadOnlyList<IReplayHeader> Replays { get; }
        IReplayMetadataManager MetadataManager { get; }

        Task LoadReplayHeadersAsync(CancellationToken token);
    }
}