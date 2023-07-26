using System.Threading;
using System.Threading.Tasks;
using RReplay = BeatLeader.Models.Replay.Replay;

namespace BeatLeader.Models {
    public interface IReplayFileManager {
        Task<RReplay?> LoadReplayAsync(IReplayHeader header, CancellationToken token);

        bool DeleteReplay(IReplayHeader header);
    }
}