using System;

namespace BeatLeader.Models {
    public interface IReplayHeader : IReplayHeaderBase {
        FileStatus FileStatus { get; }
        string FilePath { get; }

        event Action<FileStatus> StatusChangedEvent;

        bool DeleteReplay();
    }
}