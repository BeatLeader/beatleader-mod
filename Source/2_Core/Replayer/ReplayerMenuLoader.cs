using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.DataManager;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Models.Replay;
using BeatLeader.Utils;
using IPA.Utilities;
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
        [Inject] private readonly IReplayManager _replayManager = null!;

        public async Task StartReplayAsync(Replay replay, IPlayer? player = null, ReplayerSettings? settings = null) {
            await StartReplayAsync(replay, player, settings, CancellationToken.None);
        }

        public async Task StartReplayAsync(Replay replay, IPlayer? player, ReplayerSettings? settings, CancellationToken token) {
            settings ??= ReplayerSettings.UserSettings;
            var data = new ReplayLaunchData();
            var info = replay.info;
            
            Plugin.Log.Info("Attempting to load replay:\r\n" + info);
            ReplayManager.SaturateReplayInfo(info, null);
            await LoadBeatmapAsync(data, info.hash, info.mode, info.difficulty, token);
            if (settings.LoadPlayerEnvironment) LoadEnvironment(data, info.environment);
            
            var tablePlayer = player is null ? null : TablePlayer.CreateFromPlayer(player, ColorUtils.RandomColor());
            var creplay = ReplayDataUtils.ConvertToAbstractReplay(replay, tablePlayer);
            data.Init(creplay, ReplayDataUtils.BasicReplayComparator, settings, data.DifficultyBeatmap, data.EnvironmentInfo);
            
            StartReplay(data);
        }

        public async Task StartReplaysAsync(IReadOnlyDictionary<Replay, IPlayer?> replays, ReplayerSettings? settings, CancellationToken token) {
            if (replays.Count == 0) return;
            
            settings ??= ReplayerSettings.UserSettings;
            var data = new ReplayLaunchData();
            var abstractReplays = new List<IReplay>(replays.Count);
            var info = replays.First().Key.info;
            
            await LoadBeatmapAsync(data, info.hash, info.mode, info.difficulty, token);
            if (settings.LoadPlayerEnvironment) LoadEnvironment(data, info.environment);
            foreach (var (replay, player) in replays) {
                Plugin.Log.Info("Attempting to load replay:\r\n" + replay.info);
                var tablePlayer = player is null ? null : TablePlayer.CreateFromPlayer(player, ColorUtils.RandomColor());
                abstractReplays.Add(ReplayDataUtils.ConvertToAbstractReplay(replay, tablePlayer));
            }
            data.Init(abstractReplays, ReplayDataUtils.BasicReplayComparator,
                settings, data.DifficultyBeatmap, data.EnvironmentInfo);
            StartReplay(data);
        }

        public void StartReplay(ReplayLaunchData data) {
            data.ReplayWasFinishedEvent += HandleReplayWasFinished;
            if (!_launcher.StartReplay(data)) return;
            InputUtils.forceFPFC = InputUtils.containsFPFCArg && _fpfcSettings.Ignore ? _fpfcSettings.Enabled : null;
        }

        public async Task StartLastReplayAsync() {
            if (Instance is null) return;
            if (_replayManager.CachedReplay is not { } header) return;
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
            return await LoadBeatmapAsync(info.hash, info.mode, info.difficulty,
                default) is { } beatmap && SongCoreInterop.ValidateRequirements(beatmap);
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