using System;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models.Replay;

namespace BeatLeader.Models {
    public interface IReplayHeader {
        string FilePath { get; }
        ReplayInfo? Info { get; }
        ReplayStatus Status { get; }

        event Action<ReplayStatus> StatusChangedEvent;
        
        Task<Replay.Replay?> LoadReplayAsync(CancellationToken token);
        Task<bool> DeleteReplayAsync(CancellationToken token);
    }
}