using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;

namespace BeatLeader.UI.Hub {
    internal interface IBattleRoyaleHost {
        IReadOnlyCollection<IBattleRoyaleQueuedReplay> PendingReplays { get; }
        IListFilter<IReplayHeaderBase> ReplayFilter { get; }
        IDifficultyBeatmap? ReplayBeatmap { get; set; }
        bool CanLaunchBattle { get; }

        event Action<IBattleRoyaleQueuedReplay, object>? ReplayAddedEvent;
        event Action<IBattleRoyaleQueuedReplay, object>? ReplayRemovedEvent;
        event Action<IBattleRoyaleQueuedReplay>? ReplayNavigationRequestedEvent;
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