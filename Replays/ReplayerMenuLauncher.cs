using System;
using BeatLeader.Manager;
using BeatLeader.Utils;
using BeatLeader.Models;
using BeatLeader.Replays.Managers;
using SiraUtil.Tools.FPFC;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class ReplayerMenuLauncher : MonoBehaviour
    {
        [Inject] protected readonly StandardLevelDetailViewController _levelDetailViewController;
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

        public void Awake()
        {
            LeaderboardEvents.ReplayButtonWasPressedAction += StartLevelWithReplay;
        }
        public void StartLevelWithReplay(Score score)
        {
            var replayTask = StartCoroutine(HttpUtils.DownloadReplay(score.replay, 1, (Replay result) => 
            {
                Plugin.Log.Notice($"Downloaded replay of [{result.info.playerID}] for [{result.info.songName}-{result.info.difficulty}]");
                StartLevelWithReplay(result);
            }));
        }
        public void StartLevelWithReplay(Replay replay, IDifficultyBeatmap difficulty = null, IPreviewBeatmapLevel previewBeatmapLevel = null)
        {
            if (replay == null) return;
            if (difficulty == null) difficulty = _levelDetailViewController.selectedDifficultyBeatmap;
            if (previewBeatmapLevel == null) previewBeatmapLevel = _levelDetailViewController.beatmapLevel;

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
