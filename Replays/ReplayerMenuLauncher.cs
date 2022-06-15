using System;
using BeatLeader.Replays.Managers;
using BeatLeader.Utils;
using BeatLeader.Models;
using SiraUtil.Tools.FPFC;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class ReplayerMenuLauncher : MonoBehaviour
    {
        [Inject] protected readonly GameScenesManager _gameScenesManager;
        [Inject] protected readonly DiContainer _diContainer;
        [Inject] protected readonly PlayerDataModel _playerDataModel;
        [Inject] protected readonly IFPFCSettings _fPFCSettings;

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
            data.didFinishEvent += HandleReplayDidFinish;
            _gameScenesManager.PushScenes(data, 0.7f);
            _replay = replay;
            _lastTransitionData = data;
            _startedAsReplay = true;
        }
        public void HandleReplayDidFinish(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupData, LevelCompletionResults levelCompletionResults)
        {
            standardLevelScenesTransitionSetupData.didFinishEvent -= HandleReplayDidFinish;
            _gameScenesManager.PopScenes((levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed || levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared) ? 1.3f : 0.35f, null, delegate (DiContainer container)
            {
                replayFinishedEvent?.Invoke(standardLevelScenesTransitionSetupData, levelCompletionResults);
            });
            InputManager.EnableCursor(false);
            _startedAsReplay = false;
            _lastTransitionData = null;
            _replay = null;
            _fPFCSettings.Enabled = true;
        }
    }
}
