using BeatLeader.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;
using static BeatmapLevelsModel;

namespace BeatLeader.Replayer {
    public class ReplayerLauncher : MonoBehaviour {
        [Inject] private readonly BeatmapLevelsModel _levelsModel;
        [Inject] private readonly GameScenesManager _gameScenesManager;
        [Inject] private readonly PlayerDataModel _playerDataModel;

        public static ReplayLaunchData LaunchData { get; private set; }
        public static bool IsStartedAsReplay { get; private set; }

        public static event Action<ReplayLaunchData> ReplayWasStartedEvent;
        public static event Action<ReplayLaunchData> ReplayWasFinishedEvent;

        public async Task<bool> StartReplayAsync(ReplayLaunchData data) {
            return await StartReplayAsync(data, new CancellationToken());
        }
        public async Task<bool> StartReplayAsync(ReplayLaunchData data, CancellationToken token) {
            Plugin.Log.Notice("[Launcher] Loading replay data...");

            if (data.DifficultyBeatmap == null) {
                var loadingResult = await GetBeatmapDifficultyByReplayInfoAsync(data.Replay.info, token);
                data.OverrideWith(loadingResult.value);
                if (loadingResult.isError) return false;
            }

            var environmentInfo = GetEnvironmentByLaunchData(data);
            var transitionData = data.Replay.CreateTransitionData(_playerDataModel, data.DifficultyBeatmap, environmentInfo.value);
            transitionData.didFinishEvent += ResetData;

            IsStartedAsReplay = true;
            LaunchData = data;

            _gameScenesManager.PushScenes(transitionData, 0.7f, null);
            ReplayWasStartedEvent?.Invoke(data);
            Plugin.Log.Notice("[Launcher] Starting replay...");

            return true;
        }

        private async Task<RequestResult<IDifficultyBeatmap>> GetBeatmapDifficultyByReplayInfoAsync(ReplayInfo info, CancellationToken token) {
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
        private async Task<GetBeatmapLevelResult> GetBeatmapLevelByHashAsync(string hash, CancellationToken token) {
            return await _levelsModel.GetBeatmapLevelAsync(CustomLevelLoader.kCustomLevelPrefixId + hash, token);
        }

        private RequestResult<EnvironmentInfoSO> GetEnvironmentByLaunchData(ReplayLaunchData data) {
            EnvironmentInfoSO environment = null;

            if (data.ActualSettings.LoadPlayerEnvironment) {
                environment = ReplayDataHelper.GetEnvironmentByName(data.Replay.info.environment);
                if (environment == null) Plugin.Log.Error("[Launcher] Failed to parse player environment!");
            } else if (data.OverrideEnvironmentInfo != null)
                environment = data.OverrideEnvironmentInfo;

            return new RequestResult<EnvironmentInfoSO>(environment == null, environment);
        }
        private static void ResetData(StandardLevelScenesTransitionSetupDataSO transitionData, LevelCompletionResults completionResults) {
            transitionData.didFinishEvent -= ResetData;
            LaunchData.HandleReplayDidFinish(transitionData);
            ReplayWasFinishedEvent?.Invoke(LaunchData);

            LaunchData = null;
            IsStartedAsReplay = false;
        }
    }
}
