using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models.Activity;
using JetBrains.Annotations;
using RReplay = BeatLeader.Models.Replay.Replay;

namespace BeatLeader.Models {
    [PublicAPI]
    public interface IReplayManager {
        event Action<IReplayHeader>? ReplayAddedEvent;
        event Action<IReplayHeader>? ReplayDeletedEvent;
        event Action<string[]?>? ReplaysDeletedEvent;

        IReplayHeader? LastSavedReplay { get; }

        Task<IEnumerable<IReplayHeader>?> LoadReplayHeadersAsync(
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

        internal Task<bool> DeleteReplayAsync(IReplayHeader header, CancellationToken token);

        internal Task<RReplay?> LoadReplayAsync(IReplayHeader header, CancellationToken token);
    }
}