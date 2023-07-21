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
        event Action<string[]?>? ReplaysDeletedEvent;

        IReplayHeader? CachedReplay { get; }
        
        Task<IList<IReplayHeader>?> LoadReplayHeadersAsync(
            CancellationToken token,
            Action<IReplayHeader>? loadCallback = null,
            bool makeArray = true
        );

        Task<IReplayHeader?> SaveReplayAsync(
            RReplay replay,
            PlayEndData playEndData,
            CancellationToken token
        );

        Task<string[]?> DeleteAllReplaysAsync(CancellationToken token);
    }
}