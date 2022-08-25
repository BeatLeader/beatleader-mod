using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Replay replay;
        public Player player;
        public ReplayerSettings settings;
        public IDifficultyBeatmap difficultyBeatmap;
        public EnvironmentInfoSO environmentInfo = null;
        public bool overrideSettings => settings != null;

        public event Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults, ReplayLaunchData> OnReplayFinish;

        public void NotifyReplayDidFinish(StandardLevelScenesTransitionSetupDataSO transitionData, LevelCompletionResults completionResults)
        {
            OnReplayFinish?.Invoke(transitionData, completionResults, this);
        }
    }
}
