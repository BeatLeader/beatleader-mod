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

            if (data.Replays.Count == 0) return false;

            var beatmapDiff = data.DifficultyBeatmap;
            if (beatmapDiff == null) {
                var loadingResult = await GetBeatmapDifficultyByReplayInfoAsync(data.MainReplay.info, token);
                if (loadingResult.isError) return false;
                beatmapDiff = loadingResult.value;
            }

            if (token.IsCancellationRequested) return false;
            var environmentInfo = await GetEnvironmentByLaunchData(data);

            if (token.IsCancellationRequested) return false;
            data.Init(data.Replays, data.Settings, beatmapDiff, data.EnvironmentInfo);

            if (token.IsCancellationRequested) return false;
            var transitionData = data.CreateTransitionData(_playerDataModel);
            transitionData.didFinishEvent += ResetData;

            IsStartedAsReplay = true;
            LaunchData = data;
            if (token.IsCancellationRequested) return false;

            _gameScenesManager.PushScenes(transitionData, 0.7f, null);
            ReplayWasStartedEvent?.Invoke(data);
            Plugin.Log.Notice("[Launcher] Starting replay...");

            return true;
        }

        private async Task<RequestResult<IDifficultyBeatmap>> GetBeatmapDifficultyByReplayInfoAsync(ReplayInfo info, CancellationToken token) {
            static RequestResult<IDifficultyBeatmap> GenerateError() => new(true, null);

            var beatmapLevelResult = await GetBeatmapLevelByHashAsync(info.hash, token);
            if (beatmapLevelResult.isError || token.IsCancellationRequested)
                return GenerateError();

            if (!Enum.TryParse(info.difficulty, out BeatmapDifficulty difficulty))
                return GenerateError();

            var characteristic = beatmapLevelResult.beatmapLevel.beatmapLevelData
                .difficultyBeatmapSets.Select(x => x.beatmapCharacteristic)
                .FirstOrDefault(x => x.serializedName == info.mode);

            if (characteristic == null || token.IsCancellationRequested)
                return GenerateError();

            var difficultyBeatmap = beatmapLevelResult.beatmapLevel.
                beatmapLevelData.GetDifficultyBeatmap(characteristic, difficulty);

            if (difficultyBeatmap == null || token.IsCancellationRequested)
                return GenerateError();

            return new(false, difficultyBeatmap);
        }
        private async Task<GetBeatmapLevelResult> GetBeatmapLevelByHashAsync(string hash, CancellationToken token) {
            return await _levelsModel.GetBeatmapLevelAsync(CustomLevelLoader.kCustomLevelPrefixId + hash, token);
        }
        private Task<RequestResult<EnvironmentInfoSO>> GetEnvironmentByLaunchData(ReplayLaunchData data) {
            if (data.Replays.Count > 1)
                return Task.FromResult(new RequestResult<EnvironmentInfoSO>(true, null));
            EnvironmentInfoSO environment = null;

            try {
                if (data.Settings.LoadPlayerEnvironment) {
                    environment = ReplayDataHelper.GetEnvironmentByName(data.Replays[0].Value.info.environment);
                    if (environment == null)
                        Plugin.Log.Error("[Launcher] Failed to parse player environment!");
                } else if (data.EnvironmentInfo != null) {
                    environment = data.EnvironmentInfo;
                }
            } catch (Exception ex) {
                Plugin.Log.Error("[Launcher] Failed to load player environment! \r\n" + ex);
            }

            return Task.FromResult(new RequestResult<EnvironmentInfoSO>(environment == null, environment));
        }
        
        private static void ResetData(StandardLevelScenesTransitionSetupDataSO transitionData, LevelCompletionResults completionResults) {
            transitionData.didFinishEvent -= ResetData;
            LaunchData.FinishReplay(transitionData);
            ReplayWasFinishedEvent?.Invoke(LaunchData);

            LaunchData = null;
            IsStartedAsReplay = false;
        }
    }
}
