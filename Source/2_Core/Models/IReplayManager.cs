using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BeatLeader.Models {
    public interface IReplayManager {
        Task<IEnumerable<IReplayHeader>> LoadReplayHeadersAsync(CancellationToken token);
        Task<Replay.Replay?> LoadReplayAsync(IReplayHeader header, CancellationToken token);
        Task<bool> DeleteReplayAsync(IReplayHeader header, CancellationToken token);
    }
}