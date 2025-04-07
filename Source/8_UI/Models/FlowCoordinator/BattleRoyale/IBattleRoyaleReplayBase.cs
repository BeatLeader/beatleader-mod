using System.Threading.Tasks;
using BeatLeader.Models;

namespace BeatLeader.UI.Hub {
    public interface IBattleRoyaleReplayBase {
        IReplayHeader ReplayHeader { get; }

        Task<IOptionalReplayData> GetReplayDataAsync(bool bypassCache = false);
    }
}