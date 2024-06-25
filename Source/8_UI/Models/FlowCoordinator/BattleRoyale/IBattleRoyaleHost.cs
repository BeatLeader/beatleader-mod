using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;

namespace BeatLeader.UI.Hub {
    internal interface IBattleRoyaleHost {
        IReadOnlyCollection<IBattleRoyaleReplay> PendingReplays { get; }
        IListFilter<IReplayHeaderBase> ReplayFilter { get; }
        IDifficultyBeatmap? ReplayBeatmap { get; set; }
        bool CanLaunchBattle { get; }

        event Action<IBattleRoyaleReplay, object>? ReplayAddedEvent;
        event Action<IBattleRoyaleReplay, object>? ReplayRemovedEvent;
        event Action<IBattleRoyaleReplay>? ReplayNavigationRequestedEvent;
        event Action? ReplayRefreshRequestedEvent;
        
        event Action<IDifficultyBeatmap?> ReplayBeatmapChangedEvent;
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