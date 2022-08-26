using BeatLeader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;
using static BeatmapLevelsModel;

namespace BeatLeader.Replayer
{
    public class ReplayerLauncher : MonoBehaviour
    {
        [Inject] private readonly BeatmapLevelsModel _levelsModel;
        [Inject] private readonly GameScenesManager _gameScenesManager;
        [Inject] private readonly PlayerDataModel _playerDataModel;

        public static ReplayLaunchData LaunchData { get; private set; }
        public static bool IsStartedAsReplay { get; private set; }

        public async Task<bool> StartReplayAsync(ReplayLaunchData data, Action<bool> callback = null)
        {
            return await StartReplayAsync(data, new CancellationToken(), callback);
        }
        public async Task<bool> StartReplayAsync(ReplayLaunchData data, CancellationToken token, Action<bool> callback)
        {
            bool loadResult = await AssignDataAsync(data, token);
            if (!loadResult) return false;

            var environmentInfo = GetEnvironmentByLaunchData(data);
            var transitionData = data.replay.CreateTransitionData(_playerDataModel, data.difficultyBeatmap, environmentInfo.value);
            transitionData.didFinishEvent += ResetData;

            if (token.IsCancellationRequested)
            {
                callback?.Invoke(false);
                return false;
            }

            IsStartedAsReplay = true;
            LaunchData = data;

            _gameScenesManager.PushScenes(transitionData, 0.7f, null);
            callback?.Invoke(true);

            return loadResult;
        }

        private async Task<bool> AssignDataAsync(ReplayLaunchData data, CancellationToken token)
        {
            if (data.difficultyBeatmap != null) return true;

            var loadingResult = await GetBeatmapDifficultyByReplayInfoAsync(data.replay.info, token);
            if (loadingResult.isError || token.IsCancellationRequested) return false;

            data.difficultyBeatmap = loadingResult.value;
            return true;
        }
        private async Task<RequestResult<IDifficultyBeatmap>> GetBeatmapDifficultyByReplayInfoAsync(ReplayInfo info, CancellationToken token)
        {
            RequestResult<IDifficultyBeatmap> generateError() => new RequestResult<IDifficultyBeatmap>(true, null);

            var beatmapLevelResult = await GetBeatmapLevelByHashAsync(info.hash, token);
            if (beatmapLevelResult.isError || token.IsCancellationRequested)
                return generateError();

            if (!Enum.TryParse(info.difficulty, out BeatmapDifficulty difficulty))
                return generateError();

            var characteristic = beatmapLevelResult.beatmapLevel.beatmapLevelData
                .difficultyBeatmapSets.Select(x => x.beatmapCharacteristic)
                .FirstOrDefault(x => x.serializedName == info.mode);

            if (characteristic == null || token.IsCancellationRequested)
                return generateError();

            var difficultyBeatmap = beatmapLevelResult.beatmapLevel.
                beatmapLevelData.GetDifficultyBeatmap(characteristic, difficulty);

            if (difficultyBeatmap == null || token.IsCancellationRequested)
                return generateError();

            return new RequestResult<IDifficultyBeatmap>(false, difficultyBeatmap);
        }
        private async Task<GetBeatmapLevelResult> GetBeatmapLevelByHashAsync(string hash, CancellationToken token)
        {
            return await _levelsModel.GetBeatmapLevelAsync("custom_level_" + hash, token);
        }

        private RequestResult<EnvironmentInfoSO> GetEnvironmentByLaunchData(ReplayLaunchData data)
        {
            EnvironmentInfoSO environment = null;

            if (data.overrideSettings && data.settings.loadPlayerEnvironment)
            {
                environment = ReplayDataHelper.GetEnvironmentByName(data.replay.info.environment);
                if (environment == null) Plugin.Log.Critical("[Launcher] Failed to parse player environment!");
            }
            else if (data.environmentInfo != null)
                environment = data.environmentInfo;

            return new RequestResult<EnvironmentInfoSO>(environment == null, environment);
        }
        private void ResetData(StandardLevelScenesTransitionSetupDataSO transitionData, LevelCompletionResults completionResults)
        {
            transitionData.didFinishEvent -= ResetData;
            LaunchData.NotifyReplayDidFinish(transitionData, completionResults);
            LaunchData = null;
            IsStartedAsReplay = false;
        }
    }
}
