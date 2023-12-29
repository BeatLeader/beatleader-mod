using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RReplay = BeatLeader.Models.Replay.Replay;

namespace BeatLeader.Models {
    [PublicAPI]
    public interface IReplayManager {
        event Action<IReplayHeader>? ReplayAddedEvent;
        event Action<IReplayHeader>? ReplayDeletedEvent;

        IReplayHeader? CachedReplay { get; }
        IReadOnlyList<IReplayHeader> Replays { get; }

        Task LoadReplayHeadersAsync(CancellationToken token);
        Task<IReplayHeader?> SaveReplayAsync(RReplay replay, PlayEndData playEndData, CancellationToken token);
    }
}