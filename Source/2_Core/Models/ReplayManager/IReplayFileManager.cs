using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RReplay = BeatLeader.Models.Replay.Replay;

namespace BeatLeader.Models {
    [PublicAPI]
    public interface IReplayFileManager {
        /// <summary>
        /// Saves recorded replay to the disk.
        /// </summary>
        /// <param name="replay">The replay</param>
        /// <param name="playEndData">Additional replay data</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>A replay header</returns>
        Task<IReplayHeader?> SaveReplayAsync(RReplay replay, PlayEndData playEndData, CancellationToken token);
        
        /// <summary>
        /// Loads the whole replay from the disk.
        /// </summary>
        /// <param name="header">Replay header to load</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>A loaded replay</returns>
        Task<RReplay?> LoadReplayAsync(IReplayHeader header, CancellationToken token);

        /// <summary>
        /// Deletes all available replays from the disk.
        /// </summary>
        /// <returns>Count of successfully deleted replays</returns>
        int DeleteAllReplays();
        
        /// <summary>
        ///  Deletes specified replay from the disk.
        /// </summary>
        /// <param name="header">Replay header to delete</param>
        /// <returns>True if succeed and False if not</returns>
        bool DeleteReplay(IReplayHeader header);
    }
}