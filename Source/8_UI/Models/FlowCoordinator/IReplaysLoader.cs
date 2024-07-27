using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatLeader.Models;

namespace BeatLeader.UI.Hub.Models {
    internal interface IReplaysLoader {
        IReadOnlyList<IReplayHeader> LoadedReplays { get; }
        
        event Action<IReplayHeader>? ReplayLoadedEvent;
        event Action<IReplayHeader>? ReplayRemovedEvent;
        event Action? AllReplaysRemovedEvent;

        event Action? ReplaysLoadStartedEvent;
        event Action? ReplaysLoadFinishedEvent;
        
        void StartReplaysLoad();
        void CancelReplaysLoad();
        Task WaitForReplaysLoad();
    }
}