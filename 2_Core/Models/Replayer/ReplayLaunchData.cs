using System;
using System.Collections.Generic;

namespace BeatLeader.Models {
    public class ReplayLaunchData {
        public IReadOnlyList<KeyValuePair<Player, Replay>> Replays { get; protected set; } = null!;
        public IDifficultyBeatmap? DifficultyBeatmap { get; protected set; }
        public EnvironmentInfoSO? EnvironmentInfo { get; protected set; }
        public ReplayerSettings Settings { get; protected set; } = null!;

        public Replay MainReplay => Replays[0].Value;
        public bool IsBattleRoyale => Replays.Count > 1;

        public event Action<StandardLevelScenesTransitionSetupDataSO, ReplayLaunchData>? ReplayWasFinishedEvent;

        public void Init(Replay replay, ReplayerSettings settings, Player? player = null,
            IDifficultyBeatmap? difficultyBeatmap = null, EnvironmentInfoSO? environmentInfo = null) {
            Init(new List<KeyValuePair<Player, Replay>>() { new(player, replay) }, 
                settings, difficultyBeatmap, environmentInfo);
        }

        public void Init(IReadOnlyList<KeyValuePair<Player, Replay>> replays, ReplayerSettings settings,
            IDifficultyBeatmap? difficultyBeatmap = null, EnvironmentInfoSO? environmentInfo = null) {
            Replays = replays;
            DifficultyBeatmap = difficultyBeatmap;
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
            line += MainReplay.info.ToString();
            return line;
        }
    }
}