using System.Threading;
using System.Threading.Tasks;

namespace BeatLeader.Models {
    public interface IReplayHeaderBase {
        IReplayInfo ReplayInfo { get; }
        IReplayMetadata ReplayMetadata { get; }
        
        //TODO: move to the IReplay abstraction usage
        /// <summary>
        /// Loads an IReplay instance. Automatically updates player when needed, so no need to perform additional actions
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        Task<Replay.Replay?> LoadReplayAsync(CancellationToken token);
        
        /// <summary>
        /// Acquires an IPlayer instance from the server
        /// </summary>
        /// <param name="bypassCache">By default every request is cached. If you want to clear the cache and request the player again - pass True</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        Task<IPlayer> LoadPlayerAsync(bool bypassCache, CancellationToken token);
    }
}