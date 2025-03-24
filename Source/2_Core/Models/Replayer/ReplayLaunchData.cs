using BeatLeader.Models.AbstractReplay;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace BeatLeader.Models {
    //TODO: make an abstraction and remove that reinitialization
    [PublicAPI]
    public class ReplayLaunchData {
        public IReadOnlyList<IReplay> Replays { get; protected set; } = null!;
        public BeatmapLevelWithKey BeatmapLevel { get; protected set; }
        public ReplayerSettings Settings { get; protected set; } = null!;
        public EnvironmentInfoSO? EnvironmentInfo { get; protected set; }

        public IReplayComparator ReplayComparator { get; protected set; } = null!;
        public IVirtualPlayerBodySpawner? BodySpawner { get; protected set; }

        public IReplay MainReplay => Replays[0];
        public bool IsBattleRoyale => Replays.Count > 1;

        public event Action<StandardLevelScenesTransitionSetupDataSO, ReplayLaunchData>? ReplayWasFinishedEvent;

        public void Init(
            IReplay replay,
            IReplayComparator comparator,
            ReplayerSettings settings,
            BeatmapLevelWithKey beatmapLevel = default,
            EnvironmentInfoSO? environmentInfo = null
        ) {
            Init(new[] { replay }, comparator, settings, beatmapLevel, environmentInfo);
        }

        public void Init(
            IReadOnlyList<IReplay> replays,
            IReplayComparator comparator,
            ReplayerSettings settings,
            BeatmapLevelWithKey beatmapLevel = default,
            EnvironmentInfoSO? environmentInfo = null
        ) {
            Replays = replays;
            ReplayComparator = comparator;
            BeatmapLevel = beatmapLevel;
            EnvironmentInfo = environmentInfo;
            Settings = settings;
        }

        public void FinishReplay(StandardLevelScenesTransitionSetupDataSO transitionData) {
            ReplayWasFinishedEvent?.Invoke(transitionData, this);
        }

        public override string ToString() {
            var line = string.Empty;
            line += "ReplayerMode: ";
            if (IsBattleRoyale) {
                line += "BattleRoyale\r\n";
                line += $"PlayersCount: {Replays.Count}\r\n";
            } else line += "Default\r\n";
            line += MainReplay.ReplayData.ToString();
            return line;
        }
    }
}