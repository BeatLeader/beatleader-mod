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
using System.Text;
using System.Net;
using System.IO;

namespace BeatLeader.Replayer
{
    public class ReplayerMenuLoader : MonoBehaviour
    {
        [Inject] private readonly ReplayerLauncher _launcher;
        [Inject] private readonly GameScenesManager _scenesManager;
        [Inject] private readonly IFPFCSettings _fPFCSettings;

        public readonly Dictionary<int, ReplayLaunchData> SessionReplays = new();
        private int _nextReplayIndex = 0;

        private void Awake()
        {
            LeaderboardEvents.ReplayButtonWasPressedAction += NotifyReplayButtonPressed;
        }
        private void OnDestroy()
        {
            LeaderboardEvents.ReplayButtonWasPressedAction -= NotifyReplayButtonPressed;
        }
        public static async Task<RequestResult<Replay>> DownloadReplayAsync(string link)
        {
            var client = new WebClient();
            var data = await client.DownloadDataTaskAsync(link);

            var readStream = new MemoryStream(data);

            int arrayLength = (int)readStream.Length;
            byte[] buffer = new byte[arrayLength];

            readStream.Read(buffer, 0, arrayLength);

            bool flag = false;
            if (!(flag = ReplayDecoder.TryDecode(buffer, out var replay)))
                Plugin.Log.Critical($"An exception occurred during attemping to decode replay!");
            return new RequestResult<Replay>(!flag, replay);
        }
        public async Task<bool> DownloadAndStartReplayAsync(Score score, ReplayerSettings settings = null)
        {
            Plugin.Log.Notice("[Loader] Download started...");
            var downloadResult = await DownloadReplayAsync(score.replay);
            var replay = downloadResult.value;

            if (downloadResult.isError || replay == null)
            {
                Plugin.Log.Error("[Loader] Download error!");
                return false;
            }

            var data = new ReplayLaunchData(replay, score.player, settings);
            data.OnReplayFinish += NotifyLevelDidFinish;

            Plugin.Log.Notice("[Loader] Download done, replay data:");
            string line = string.Empty;
            line += $"Player: {replay.info.playerName}\r\n";
            line += $"Song: {replay.info.songName}\r\n";
            line += $"Difficulty: {replay.info.difficulty}\r\n";
            line += $"Modifiers: {replay.info.modifiers}\r\n";
            line += $"Environment: {replay.info.environment}";
            Plugin.Log.Info(line);

            if (await _launcher.StartReplayAsync(data))
            {
                SessionReplays.Add(_nextReplayIndex, data);
                _nextReplayIndex++;
            }

            return true;
        }
        private void NotifyLevelDidFinish(StandardLevelScenesTransitionSetupDataSO transitioтData,
            LevelCompletionResults completionResults, ReplayLaunchData launchData)
        {
            launchData.OnReplayFinish -= NotifyLevelDidFinish;
            _scenesManager.PopScenes(completionResults.levelEndStateType == 0 ? 0.35f : 1.3f);

            InputManager.EnableCursor(false);
            _fPFCSettings.Enabled = true;
        }
        private void NotifyReplayButtonPressed(Score score, ReplayerSettings settings)
        {
            //TODO: remove these lines after creating leaderboard ui
            settings = new();
            settings.loadPlayerEnvironment = true;
            settings.showUI = true;
            settings.useReplayerCamera = true;
            DownloadAndStartReplayAsync(score, settings);
        }
    }
}