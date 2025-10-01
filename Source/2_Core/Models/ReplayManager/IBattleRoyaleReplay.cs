using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models.AbstractReplay;
using JetBrains.Annotations;

namespace BeatLeader.Models { 
    [PublicAPI]
    public interface IBattleRoyaleReplay {
        IReplayHeader ReplayHeader { get; }

        /// <summary>
        /// Loads battle royale data.
        /// </summary>
        /// <param name="bypassCache">Determines if the cached value should be omitted.</param>
        /// <returns>A replay data.</returns>
        Task<BattleRoyaleReplayData> GetBattleRoyaleDataAsync(bool bypassCache, CancellationToken token);
    }
}