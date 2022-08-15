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
    //TODO: full rewrite
    public class ReplayerMenuLauncher : MonoBehaviour
    {
        [Inject] private readonly StandardLevelDetailViewController _levelDetailViewController;
        [Inject] private readonly GameScenesManager _gameScenesManager;
        [Inject] private readonly PlayerDataModel _playerDataModel;
        [Inject] private readonly IFPFCSettings _fPFCSettings;

        private static StandardLevelScenesTransitionSetupDataSO _transitionData;
        private static Replay _replay;
        private static Score _score;
        private static bool _startedAsReplay;

        public event Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> OnReplayFinish;
        public static StandardLevelScenesTransitionSetupDataSO TransitionData => _transitionData;
        public static Replay Replay => _replay;
        public static Score Score => _score;
        public static bool IsStartedAsReplay => _startedAsReplay;

        public void Awake()
        {
            LeaderboardEvents.ReplayButtonWasPressedAction += StartLevelWithReplay;
        }
        public void OnDestroy()
        {
            LeaderboardEvents.ReplayButtonWasPressedAction -= StartLevelWithReplay;
        }
        public void StartLevelWithReplay(Score score)
        {
            StartCoroutine(HttpUtils.DownloadReplay(score.replay, 1, (Replay result) => 
            {
                Plugin.Log.Notice($"Downloaded replay of [{result.info.playerName}] for [{result.info.songName}-{result.info.difficulty}]");
                StartLevelWithReplay(result, score);
            }));
        }
        public void StartLevelWithReplay(Replay replay, Score score, IDifficultyBeatmap difficulty = null, IPreviewBeatmapLevel previewBeatmapLevel = null)
        {
            if (replay == null) return;
            difficulty ??= _levelDetailViewController.selectedDifficultyBeatmap;
            previewBeatmapLevel ??= _levelDetailViewController.beatmapLevel;

            StandardLevelScenesTransitionSetupDataSO data = replay.CreateTransitionData(_playerDataModel, difficulty, previewBeatmapLevel);
            data.didFinishEvent += HandleReplayDidFinish;
            _gameScenesManager.PushScenes(data, 0.7f);
            _replay = replay;
            _score = score;
            _transitionData = data;
            _startedAsReplay = true;
        }
        public void HandleReplayDidFinish(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupData, LevelCompletionResults levelCompletionResults)
        {
            standardLevelScenesTransitionSetupData.didFinishEvent -= HandleReplayDidFinish;
            _gameScenesManager.PopScenes((levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed || levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared) ? 1.3f : 0.35f, null, delegate (DiContainer container)
            {
                OnReplayFinish?.Invoke(standardLevelScenesTransitionSetupData, levelCompletionResults);
            });
            InputManager.EnableCursor(false);
            _startedAsReplay = false;
            _transitionData = null;
            _replay = null;
            _fPFCSettings.Enabled = true;
        }
    }
}
