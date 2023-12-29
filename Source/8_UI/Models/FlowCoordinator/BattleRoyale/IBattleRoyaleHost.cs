using System;
using System.Collections.Generic;
using BeatLeader.Models;

namespace BeatLeader.UI.Hub.Models {
    internal interface IBattleRoyaleHost {
        IReadOnlyCollection<IReplayHeaderBase> PendingReplays { get; }
        IReplayFilter ReplayFilter { get; }
        IBeatmapReplayFilterData? FilterData { get; set; }
        bool CanLaunchBattle { get; }
        
        event Action<IReplayHeaderBase, object>? ReplayAddedEvent; 
        event Action<IReplayHeaderBase, object>? ReplayRemovedEvent;
        event Action<IReplayHeaderBase>? ReplayNavigationRequestedEvent;
        
        event Action<bool>? HostStateChangedEvent;
        event Action<bool>? CanLaunchBattleStateChangedEvent;
        event Action? BattleLaunchStartedEvent;
        event Action? BattleLaunchFinishedEvent;

        void LaunchBattle();
        void AddReplay(IReplayHeaderBase header, object caller);
        void RemoveReplay(IReplayHeaderBase header, object caller);
        void NavigateTo(IReplayHeaderBase header);
    }
}