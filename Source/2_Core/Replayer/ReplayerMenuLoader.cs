﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.DataManager;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Utils;
using JetBrains.Annotations;
using SiraUtil.Tools.FPFC;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    [PublicAPI]
    public class ReplayerMenuLoader : MonoBehaviour {
        #region Init

        public static ReplayerMenuLoader? Instance { get; private set; }

        private void Awake() {
            if (Instance is not null) {
                DestroyImmediate(this);
                return;
            }
            Instance = this;
        }

        private void OnDestroy() {
            if (Instance == this) Instance = null;
        }

        #endregion

        #region StartReplay

        [Inject] private readonly ReplayerLauncher _launcher = null!;

        [Inject] private readonly GameScenesManager _scenesManager = null!;

        [Inject] private readonly IFPFCSettings _fpfcSettings = null!;

        [Inject] private readonly BeatmapLevelsModel _levelsModel = null!;

        internal async Task StartReplayFromLeaderboardAsync(Replay replay, Player player) {
            var settings = ReplayerSettings.UserSettings;
            var data = new ReplayLaunchData();
            var info = replay.info;

            Plugin.Log.Info("Attempting to load replay:\r\n" + info);
            ReplayManager.ValidateReplayInfo(info, null);

            var beatmapHash = info.hash;
            var isSecondAttempt = false;
            do {
                var result = await LoadBeatmapAsync(data, beatmapHash, info.mode, info.difficulty, CancellationToken.None);
                if (!result) {
                    Plugin.Log.Warn("Failed to load the map by hash; attempting to use leaderboard hash...");
                    beatmapHash = LeaderboardState.SelectedBeatmapKey.Hash;
                    isSecondAttempt = true;
                }
                if (result || isSecondAttempt) break;
            } while (true);

            if (data.DifficultyBeatmap is null) {
                Plugin.Log.Warn("Beatmap load failed after two attempts; forcing selected beatmap load...");
                Reinit(data, LeaderboardState.SelectedBeatmap);
            }

            if (settings.LoadPlayerEnvironment) LoadEnvironment(data, info.environment);

            var abstractReplay = ReplayDataUtils.ConvertToAbstractReplay(replay, player);
            data.Init(abstractReplay, ReplayDataUtils.BasicReplayComparator, settings, data.DifficultyBeatmap, data.EnvironmentInfo);

            StartReplay(data);
        }

        public async Task StartReplayAsync(Replay replay, Player? player = null, ReplayerSettings? settings = null) {
            await StartReplayAsync(replay, player, settings, CancellationToken.None);
        }

        public async Task StartReplayAsync(Replay replay, Player? player, ReplayerSettings? settings, CancellationToken token) {
            settings ??= ReplayerSettings.UserSettings;
            var data = new ReplayLaunchData();
            var info = replay.info;
            Plugin.Log.Info("Attempting to load replay:\r\n" + info);
            ReplayManager.ValidateReplayInfo(info, null);
            await LoadBeatmapAsync(data, info.hash, info.mode, info.difficulty, token);
            if (settings.LoadPlayerEnvironment) LoadEnvironment(data, info.environment);
            var creplay = ReplayDataUtils.ConvertToAbstractReplay(replay, player);
            data.Init(
                creplay, ReplayDataUtils.BasicReplayComparator,
                settings, data.DifficultyBeatmap, data.EnvironmentInfo
            );
            StartReplay(data);
        }

        public void StartReplay(ReplayLaunchData data) {
            data.ReplayWasFinishedEvent += HandleReplayWasFinished;
            if (!_launcher.StartReplay(data)) return;
            InputUtils.forceFPFC = InputUtils.containsFPFCArg && _fpfcSettings.Ignore ? _fpfcSettings.Enabled : null;
        }

        public async Task StartLastReplayAsync() {
            if (Instance is null) return;
            if (ReplayManager.Instance.CachedReplay is not { } header) return;
            var replay = await header.LoadReplayAsync(default);
            await StartReplayAsync(replay!, ProfileManager.Profile);
        }

        private void HandleReplayWasFinished(StandardLevelScenesTransitionSetupDataSO transitionData, ReplayLaunchData launchData) {
            launchData.ReplayWasFinishedEvent -= HandleReplayWasFinished;
            _scenesManager.PopScenes(0.3f);

            _fpfcSettings.Enabled = InputUtils.forceFPFC ?? InputUtils.containsFPFCArg;
            InputUtils.forceFPFC = null;
            InputUtils.EnableCursor(!InputUtils.containsFPFCArg);
        }

        #endregion

        #region ReplayTools

        private static IDifficultyBeatmap? _cachedDifficultyBeatmap;
        private string? _cachedBeatmapHash;
        private string? _cachedBeatmapCharacteristic;

        public async Task<bool> CanLaunchReplay(ReplayInfo info) {
            return await LoadBeatmapAsync(
                info.hash, info.mode, info.difficulty,
                default
            ) is { } beatmap && SongCoreInterop.ValidateRequirements(beatmap);
        }

        public async Task<bool> LoadBeatmapAsync(
            ReplayLaunchData launchData,
            string hash,
            string mode,
            string difficulty,
            CancellationToken token
        ) {
            if (await LoadBeatmapAsync(hash, mode, difficulty, token) is not { } difficultyBeatmap) return false;
            Reinit(launchData, difficultyBeatmap);
            return true;
        }

        public bool LoadEnvironment(ReplayLaunchData launchData, string environmentName) {
            var environment = Resources.FindObjectsOfTypeAll<EnvironmentInfoSO>()
                .FirstOrDefault(x => x.environmentName == environmentName);
            if (environment == null) {
                Plugin.Log.Error($"[Loader] Failed to load specified environment");
                return false;
            }
            Plugin.Log.Notice($"[Loader] Applied specified environment: " + environmentName);
            Reinit(launchData, environment: environment);
            return true;
        }

        public async Task<IDifficultyBeatmap?> LoadBeatmapAsync(
            string hash,
            string mode,
            string difficulty,
            CancellationToken token
        ) {
            if (!Enum.TryParse(difficulty, out BeatmapDifficulty cdifficulty)) return null;
            if (_cachedDifficultyBeatmap is not null
                && _cachedBeatmapHash == hash
                && _cachedBeatmapCharacteristic == mode
                && _cachedDifficultyBeatmap.difficulty == cdifficulty
            ) {
                return _cachedDifficultyBeatmap;
            }

            var beatmapLevel = await GetBeatmapLevelByHashAsync(hash, token);
            if (beatmapLevel == null) return null;

            var characteristic = beatmapLevel.beatmapLevelData
                .difficultyBeatmapSets.Select(static x => x.beatmapCharacteristic)
                .FirstOrDefault(x => x.serializedName == mode);
            if (characteristic == null || token.IsCancellationRequested) return null;

            var difficultyBeatmap = beatmapLevel.beatmapLevelData
                .GetDifficultyBeatmap(characteristic, cdifficulty);
            if (difficultyBeatmap == null || token.IsCancellationRequested) return null;

            _cachedDifficultyBeatmap = difficultyBeatmap;
            _cachedBeatmapHash = hash;
            _cachedBeatmapCharacteristic = mode;
            return difficultyBeatmap;
        }

        private async Task<IBeatmapLevel?> GetBeatmapLevelByHashAsync(string hash, CancellationToken token) {
            var error = false;
            Start: ;
            var result = await _levelsModel.GetBeatmapLevelAsync(error ? hash : CustomLevelLoader.kCustomLevelPrefixId + hash, token);
            if (!result.isError || error) return result.beatmapLevel;
            error = true;
            goto Start;
        }

        private static void Reinit(ReplayLaunchData data, IDifficultyBeatmap? beatmap = null, EnvironmentInfoSO? environment = null) {
            data.Init(data.Replays, data.ReplayComparator, data.Settings, beatmap ?? data.DifficultyBeatmap, environment ?? data.EnvironmentInfo);
        }

        #endregion
    }
}