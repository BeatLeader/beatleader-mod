using System.Threading.Tasks;
using BeatLeader.Models;

namespace BeatLeader.UI.Hub {
    public interface IBattleRoyaleReplayBase {
        IReplayHeaderBase ReplayHeader { get; }

        Task<IOptionalReplayData> GetReplayDataAsync(bool bypassCache = false);
    }
}