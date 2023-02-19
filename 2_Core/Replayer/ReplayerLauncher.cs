using BeatLeader.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    public class ReplayerLauncher : MonoBehaviour {
        [Inject] private readonly BeatmapLevelsModel _levelsModel = null!;
        [Inject] private readonly GameScenesManager _gameScenesManager = null!;
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;

        public static ReplayLaunchData? LaunchData { get; private set; }
        public static bool IsStartedAsReplay { get; private set; }

        public static event Action<ReplayLaunchData>? ReplayWasStartedEvent;
        public static event Action<ReplayLaunchData>? ReplayWasFinishedEvent;

        public async Task<bool> StartReplayAsync(ReplayLaunchData data) {
            return await StartReplayAsync(data, new CancellationToken());
        }
        public async Task<bool> StartReplayAsync(ReplayLaunchData data, CancellationToken token) {
            Plugin.Log.Notice("[Launcher] Loading replay data...");

            if (data.Replays.Count == 0) return false;

            var beatmapDiff = data.DifficultyBeatmap;
            beatmapDiff ??= await GetBeatmapDifficultyByReplayInfoAsync(data.MainReplay.info, token);
            if (beatmapDiff == null) return false;

            if (token.IsCancellationRequested) return false;
            var environmentInfo = await GetEnvironmentByLaunchData(data);

            if (token.IsCancellationRequested) return false;
            data.Init(data.Replays, data.Settings, beatmapDiff!, environmentInfo);

            if (token.IsCancellationRequested) return false;
            var transitionData = data.CreateTransitionData(_playerDataModel);
            transitionData.didFinishEvent += HandleLevelFinish;

            IsStartedAsReplay = true;
            LaunchData = data;
            if (token.IsCancellationRequested) return false;

            _gameScenesManager.PushScenes(transitionData, 0.7f, null);
            ReplayWasStartedEvent?.Invoke(data);
            Plugin.Log.Notice("[Launcher] Starting replay...");

            return true;
        }

        private async Task<IDifficultyBeatmap?> GetBeatmapDifficultyByReplayInfoAsync(ReplayInfo info, CancellationToken token) {
            var beatmapLevel = await GetBeatmapLevelByHashAsync(info.hash, token);
            if (beatmapLevel == null || token.IsCancellationRequested
                || !Enum.TryParse(info.difficulty, out BeatmapDifficulty difficulty)) return null;

            var characteristic = beatmapLevel.beatmapLevelData
                .difficultyBeatmapSets.Select(x => x.beatmapCharacteristic)
                .FirstOrDefault(x => x.serializedName == info.mode);
            if (characteristic == null || token.IsCancellationRequested) return null;

            var difficultyBeatmap = beatmapLevel.beatmapLevelData
                .GetDifficultyBeatmap(characteristic, difficulty);
            if (difficultyBeatmap == null || token.IsCancellationRequested) return null;

            return difficultyBeatmap;
        }
        private async Task<IBeatmapLevel?> GetBeatmapLevelByHashAsync(string hash, CancellationToken token) {
            return (await _levelsModel.GetBeatmapLevelAsync(CustomLevelLoader.kCustomLevelPrefixId + hash, token)).beatmapLevel;
        }
        private Task<EnvironmentInfoSO?> GetEnvironmentByLaunchData(ReplayLaunchData data) {
            if (data.IsBattleRoyale) return Task.FromResult<EnvironmentInfoSO?>(null);
            var environment = default(EnvironmentInfoSO?);
            try {
                if (data.Settings.LoadPlayerEnvironment) {
                    environment = ReplayDataHelper.GetEnvironmentByName(data.MainReplay.info.environment);
                    if (environment == null) throw new ArgumentException();
                    Plugin.Log.Warn(environment.environmentName + " " + data.MainReplay.info.environment);
                } else if (data.EnvironmentInfo != null) {
                    environment = data.EnvironmentInfo;
                }
            } catch (Exception ex) {
                Plugin.Log.Error("[Launcher] Failed to load player environment:\r\n" + ex);
            }
            return Task.FromResult(environment);
        }

        private static void HandleLevelFinish(
            StandardLevelScenesTransitionSetupDataSO transitionData, 
            LevelCompletionResults completionResults
            ) {
            transitionData.didFinishEvent -= HandleLevelFinish;
            LaunchData!.FinishReplay(transitionData);
            ReplayWasFinishedEvent?.Invoke(LaunchData);

            LaunchData = null;
            IsStartedAsReplay = false;
        }
    }
}
