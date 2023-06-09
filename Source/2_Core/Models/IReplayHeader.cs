using System;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models.Activity;
using BeatLeader.Models.Replay;

namespace BeatLeader.Models {
    public interface IReplayHeader {
        string FilePath { get; }
        FileStatus FileStatus { get; }
        
        ReplayInfo? ReplayInfo { get; }
        PlayEndData.LevelEndType ReplayFinishType { get; }

        event Action<FileStatus> StatusChangedEvent;
        
        Task<Replay.Replay?> LoadReplayAsync(CancellationToken token);
        Task<bool> DeleteReplayAsync(CancellationToken token);
    }
}