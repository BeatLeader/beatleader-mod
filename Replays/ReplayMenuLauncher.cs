using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPA.Utilities;
using System.Threading.Tasks;
using BeatLeader.Installers;
using HMUI;
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

        protected static StandardLevelScenesTransitionSetupDataSO _lastTransitionData;
        protected static Replay _replay;
        protected static bool _startedAsReplay;

        public event Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> replayFinishedEvent;
        public static StandardLevelScenesTransitionSetupDataSO transitionData => _lastTransitionData;
        public static Replay replay => _replay;
        public static bool isStartedAsReplay => _startedAsReplay;

        public void StartLevelWithReplay(IDifficultyBeatmap difficulty, IPreviewBeatmapLevel previewBeatmapLevel, Replay replay)
        {
            StandardLevelScenesTransitionSetupDataSO data = replay.CreateTransitionData(_playerDataModel, difficulty, previewBeatmapLevel);
            data.didFinishEvent += HandleMainGameSceneDidFinish;
            _gameScenesManager.PushScenes(data, 0.7f);
            _replay = replay;
            _lastTransitionData = data;
            Debug.LogWarning(replay.info.version);
            _startedAsReplay = true;
        }
        public void HandleMainGameSceneDidFinish(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupData, LevelCompletionResults levelCompletionResults)
        {
            standardLevelScenesTransitionSetupData.didFinishEvent -= HandleMainGameSceneDidFinish;
            _gameScenesManager.PopScenes((levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed || levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared) ? 1.3f : 0.35f, null, delegate (DiContainer container)
            {
                replayFinishedEvent?.Invoke(standardLevelScenesTransitionSetupData, levelCompletionResults);
            });
        }
        public static void NotifyReplayHasEnded()
        {
            _startedAsReplay = false;
            _lastTransitionData = null;
            _replay = null;
        }
    }
}
