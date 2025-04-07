using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive.Components;

namespace BeatLeader.UI.Hub {
    internal interface IBattleRoyaleHost {
        IReadOnlyCollection<IBattleRoyaleReplay> PendingReplays { get; }
        ITableFilter<IReplayHeader> ReplayFilter { get; }
        BeatmapLevelWithKey ReplayBeatmap { get; set; }
        bool CanLaunchBattle { get; }

        event Action<IBattleRoyaleReplay, object>? ReplayAddedEvent;
        event Action<IBattleRoyaleReplay, object>? ReplayRemovedEvent;
        event Action<IBattleRoyaleReplay>? ReplayNavigationRequestedEvent;
        event Action? ReplayRefreshRequestedEvent;
        
        event Action<BeatmapLevelWithKey> ReplayBeatmapChangedEvent;
        event Action<bool>? HostStateChangedEvent;
        event Action<bool>? CanLaunchBattleStateChangedEvent;
        event Action? BattleLaunchStartedEvent;
        event Action? BattleLaunchFinishedEvent;

        void LaunchBattle();
        void AddReplay(IReplayHeader header, object caller);
        void RemoveReplay(IReplayHeader header, object caller);
        void NavigateTo(IReplayHeader header);
    }
}