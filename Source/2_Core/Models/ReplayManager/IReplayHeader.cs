using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public interface IReplayHeader {
        IReplayInfo ReplayInfo { get; }
        ReplayMetadata ReplayMetadata { get; }
        
        FileStatus FileStatus { get; }
        string FilePath { get; }

        event Action<FileStatus>? StatusChangedEvent;
        
        /// <summary>
        /// Loads a Replay instance. Automatically updates player when needed, so no need to perform additional actions.
        /// </summary>
        Task<Replay.Replay?> LoadReplayAsync(CancellationToken token);
        
        /// <summary>
        /// Acquires a Player instance from the server.
        /// </summary>
        /// <param name="bypassCache">By default, every request is cached. If you want to clear the cache and request the player again - pass True.</param>
        Task<Player> LoadPlayerAsync(bool bypassCache, CancellationToken token);
    }
}