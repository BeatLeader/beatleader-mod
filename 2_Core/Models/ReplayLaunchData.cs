using System;

namespace BeatLeader.Models
{
    public class ReplayLaunchData
    {
        public ReplayLaunchData(
            Replay replay,
            Player? player = null,
            IDifficultyBeatmap difficultyBeatmap = null, 
            EnvironmentInfoSO environmentInfo = null,
            ReplayerSettings settings = null)
        {
            Replay = replay;
            Player = player;
            DifficultyBeatmap = difficultyBeatmap;
            OverrideEnvironmentInfo = environmentInfo;
            _settings = settings ?? ConfigDefaults.ReplayerSettings;
        }

        public Replay Replay { get; }
        public Player? Player { get; }
        public IDifficultyBeatmap DifficultyBeatmap { get; private set; }
        public EnvironmentInfoSO OverrideEnvironmentInfo { get; private set; }

        public virtual ReplayerSettings ActualSettings => _settings;
        public virtual ReplayerSettings ActualToWriteSettings => _settings;

        public event Action<StandardLevelScenesTransitionSetupDataSO, ReplayLaunchData> ReplayWasFinishedEvent;

        protected ReplayerSettings _settings;

        public void OverrideWith(
            IDifficultyBeatmap beatmap = null, 
            EnvironmentInfoSO environment = null,
            ReplayerSettings settings = null)
        {
            DifficultyBeatmap = beatmap ?? DifficultyBeatmap;
            OverrideEnvironmentInfo = environment ?? OverrideEnvironmentInfo;
            _settings = settings ?? _settings;
        }
        public void HandleReplayDidFinish(StandardLevelScenesTransitionSetupDataSO transitionData)
        {
            ReplayWasFinishedEvent?.Invoke(transitionData, this);
        }
    }
}
