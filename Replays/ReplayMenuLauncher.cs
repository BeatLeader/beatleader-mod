using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Installers;
using BeatLeader.Utils;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class ReplayMenuLauncher : MonoBehaviour
    {
        [Inject] protected readonly GameScenesManager _gameScenesManager;
        [Inject] protected readonly PlayerDataModel _playerDataModel;

        protected StandardLevelScenesTransitionSetupDataSO _lastTransitionedReplay;

        public event Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> replayFinishedEvent;

        public void StartLevelWithReplay(IDifficultyBeatmap difficulty, IPreviewBeatmapLevel previewBeatmapLevel, Replay replay)
        {
            StandardLevelScenesTransitionSetupDataSO data = replay.CreateTransitionData(_playerDataModel, difficulty, previewBeatmapLevel);
            data.didFinishEvent += HandleMainGameSceneDidFinish;
            _gameScenesManager.PushScenes(data, 0.7f);
        }
        public void HandleMainGameSceneDidFinish(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupData, LevelCompletionResults levelCompletionResults)
        {
            standardLevelScenesTransitionSetupData.didFinishEvent -= HandleMainGameSceneDidFinish;
            _gameScenesManager.PopScenes((levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed || levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared) ? 1.3f : 0.35f, null, delegate (DiContainer container)
            {
                replayFinishedEvent?.Invoke(standardLevelScenesTransitionSetupData, levelCompletionResults);
            });
        }
    }
}
