using System;
using BeatLeader.Manager;
using BeatLeader.Utils;
using BeatLeader.Models;
using SiraUtil.Tools.FPFC;
using UnityEngine;
using Zenject;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace BeatLeader.Replayer
{
    public class ReplayerMenuStarter : MonoBehaviour
    {
        [Inject] private readonly ReplayerLauncher _launcher;
        [Inject] private readonly GameScenesManager _scenesManager;
        [Inject] private readonly IFPFCSettings _fPFCSettings;

        public Dictionary<int, ReplayLaunchData> SessionReplays { get; private set; } = new();
        private int _nextReplayIndex = 0;

        private void Awake()
        {
            LeaderboardEvents.ReplayButtonWasPressedAction += NotifyReplayButtonPressed;
        }
        private void OnDestroy()
        {
            LeaderboardEvents.ReplayButtonWasPressedAction -= NotifyReplayButtonPressed;
        }
        public async Task<bool> DownloadAndStartReplayAsync(Score score, ReplayerSettings settings = null)
        {
            Plugin.Log.Notice("Downloading started");
            var downloadResult = await HttpUtils.DownloadReplayAsync(score.replay);
            var replay = downloadResult.value;

            if (downloadResult.isError || replay == null)
            {
                Plugin.Log.Critical("Could not download replay!");
                return false;
            }

            var data = new ReplayLaunchData(replay, score.player, settings);
            data.OnReplayFinish += NotifyLevelDidFinish;

            Plugin.Log.Notice($"Downloading done. player:[{replay.info.playerName}] song:[{replay.info.songName}-{replay.info.difficulty}]" +
                $" environment:[{replay.info.environment}]");

            if (await _launcher.StartReplayAsync(data))
            {
                SessionReplays.Add(_nextReplayIndex, data);
                _nextReplayIndex++;
            }

            return true;
        }
        private void NotifyLevelDidFinish(StandardLevelScenesTransitionSetupDataSO transitioData, LevelCompletionResults completionResults, ReplayLaunchData launchData)
        {
            launchData.OnReplayFinish -= NotifyLevelDidFinish;
            _scenesManager.PopScenes(completionResults.levelEndStateType == 0 ? 0.35f : 1.3f);

            InputManager.EnableCursor(false);
            _fPFCSettings.Enabled = true;
        }
        private void NotifyReplayButtonPressed(Score score, ReplayerSettings settings)
        {
            settings = new();
            settings.loadPlayerEnvironment = true;
            settings.showUI = true;
            settings.useReplayerCamera = true;
            DownloadAndStartReplayAsync(score, settings);
        }
    }
}
