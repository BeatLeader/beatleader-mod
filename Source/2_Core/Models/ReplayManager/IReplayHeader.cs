using System;
using System.Threading;
using System.Threading.Tasks;

namespace BeatLeader.Models {
    public interface IReplayHeader {
        FileStatus FileStatus { get; }
        string FilePath { get; }
        IReplayInfo? ReplayInfo { get; }
        
        event Action<FileStatus> StatusChangedEvent;
        
        Task<Replay.Replay?> LoadReplayAsync(CancellationToken token);
        Task<bool> DeleteReplayAsync(CancellationToken token);
    }
}