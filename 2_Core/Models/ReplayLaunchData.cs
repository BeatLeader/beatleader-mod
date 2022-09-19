using System;

namespace BeatLeader.Models
{
    public class ReplayLaunchData
    {
        public ReplayLaunchData(Replay replay, Player player = null, ReplayerSettings settings = null, 
            IDifficultyBeatmap difficultyBeatmap = null, EnvironmentInfoSO environmentInfo = null)
        {
            this.replay = replay;
            this.player = player;
            this.settings = settings;
            this.difficultyBeatmap = difficultyBeatmap;
            this.environmentInfo = environmentInfo;
        }

        public Replay replay { get; }
        public Player player { get; set; }
        public IDifficultyBeatmap difficultyBeatmap { get; set; }
        public EnvironmentInfoSO environmentInfo { get; set; }
        protected ReplayerSettings settings { get; }

        public virtual bool overrideSettings => settings != null;
        public virtual ReplayerSettings actualSettings => overrideSettings ? settings : ConfigFileData.Instance.ReplayerSettings;
        public virtual ReplayerSettings actualToWriteSettings => ConfigFileData.Instance.ReplayerSettings;

        public event Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults, ReplayLaunchData> OnReplayFinish;

        public void NotifyReplayDidFinish(StandardLevelScenesTransitionSetupDataSO transitionData, LevelCompletionResults completionResults)
        {
            OnReplayFinish?.Invoke(transitionData, completionResults, this);
        }
    }
}
