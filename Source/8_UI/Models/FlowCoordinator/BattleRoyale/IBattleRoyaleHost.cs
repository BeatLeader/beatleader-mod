using System;
using System.Collections.Generic;
using BeatLeader.Models;
using Reactive.Components;

namespace BeatLeader.UI.Hub {
    internal interface IBattleRoyaleHost {
        IReadOnlyCollection<BattleRoyaleReplay> PendingReplays { get; }
        ITableFilter<IReplayHeader> ReplayFilter { get; }
        BeatmapLevelWithKey ReplayBeatmap { get; set; }
        bool CanLaunchBattle { get; }

        event Action<BattleRoyaleReplay, object>? ReplayAddedEvent;
        event Action<BattleRoyaleReplay, object>? ReplayRemovedEvent;
        event Action<BattleRoyaleReplay>? ReplayNavigationRequestedEvent;
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