using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.DataManager;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Utils;
using HMUI;
using JetBrains.Annotations;
using SiraUtil.Tools.FPFC;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    [PublicAPI]
    public class ReplayerMenuLoader :  MonoBehaviour, IReplayerViewNavigator {
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

        internal async Task StartReplayFromLeaderboardAsync(Replay replay, Player player, Action? finishCallback = null) {
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
                    beatmapHash = LeaderboardState.SelectedLeaderboardKey.Hash;
                    isSecondAttempt = true;
                }
                if (result || isSecondAttempt) break;
            } while (true);

            if (data.BeatmapLevel is null) {
               Plugin.Log.Warn("Beatmap load failed after two attempts; forcing selected beatmap load...");
               Reinit(data, LeaderboardState.SelectedBeatmapLevel, LeaderboardState.SelectedBeatmapKey);
            }

            if (settings.LoadPlayerEnvironment) LoadEnvironment(data, info.environment);

            var abstractReplay = ReplayDataUtils.ConvertToAbstractReplay(replay, player);
            data.Init(abstractReplay, ReplayDataUtils.BasicReplayComparator, settings, data.BeatmapLevel, data.BeatmapKey, data.EnvironmentInfo);

            StartReplay(data, finishCallback);
        }

        public Task NavigateToReplayAsync(FlowCoordinator flowCoordinator, Replay replay, Player player, bool alternative) {
            return alternative ? StartReplayFromLeaderboardAsync(replay, player) : StartReplayAsync(replay, player);
        }
        
        public async Task StartReplayAsync(Replay replay, Player? player = null, ReplayerSettings? settings = null, Action? finishCallback = null) {
            await StartReplayAsync(replay, player, settings, finishCallback, CancellationToken.None);
        }

        public async Task StartReplayAsync(Replay replay, Player? player, ReplayerSettings? settings, Action? finishCallback, CancellationToken token) {
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
                settings, data.BeatmapLevel, data.BeatmapKey, data.EnvironmentInfo
            );
            StartReplay(data, finishCallback);
        }

        public void StartReplay(ReplayLaunchData data, Action? finishCallback) {
            data.ReplayWasFinishedEvent += HandleReplayWasFinished;
            if (!_launcher.StartReplay(data, finishCallback)) return;
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

        private static BeatmapLevel? _cachedDifficultyBeatmap;
        private static BeatmapKey? _cachedBeatmapKey;
        private string? _cachedBeatmapHash;
        private string? _cachedBeatmapCharacteristic;

        public async Task<bool> CanLaunchReplay(ReplayInfo info) {
            (BeatmapLevel? level, BeatmapKey? key) = await LoadBeatmapAsync(
                info.hash, info.mode, info.difficulty,
                default
            );

            return level != null && key != null && SongCoreInterop.ValidateRequirements(level, (BeatmapKey)key);
        }

        public async Task<bool> LoadBeatmapAsync(
            ReplayLaunchData launchData,
            string hash,
            string mode,
            string difficulty,
            CancellationToken token
        ) {
            (BeatmapLevel? level, BeatmapKey? key) = await LoadBeatmapAsync(hash, mode, difficulty, token);
            if (level == null || key == null) return false;
            Reinit(launchData, level, key);
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

        public async Task<(BeatmapLevel?, BeatmapKey?)> LoadBeatmapAsync(
            string hash,
            string mode,
            string difficulty,
            CancellationToken token
        ) {
            if (!Enum.TryParse(difficulty, out BeatmapDifficulty cdifficulty)) return (null, null);
            if (_cachedDifficultyBeatmap is not null
                && _cachedBeatmapHash == hash
                && _cachedBeatmapCharacteristic == mode
                && _cachedBeatmapKey?.difficulty == cdifficulty
            ) {
                return (_cachedDifficultyBeatmap, _cachedBeatmapKey);
            }

            var beatmapLevel = await GetBeatmapLevelByHashAsync(hash, token);
            if (beatmapLevel == null) return (null, null);

            var characteristic = beatmapLevel.GetCharacteristics()
                .FirstOrDefault(x => x.serializedName == mode);
            if (characteristic == null || token.IsCancellationRequested) return (null, null);

            var beatmapKey = beatmapLevel.GetBeatmapKeys()
                .FirstOrDefault(k => k.beatmapCharacteristic == characteristic && k.difficulty == cdifficulty);
            if (beatmapKey == null || token.IsCancellationRequested) return (null, null);

            _cachedDifficultyBeatmap = beatmapLevel;
            _cachedBeatmapKey = beatmapKey;
            _cachedBeatmapHash = hash;
            _cachedBeatmapCharacteristic = mode;
            return (beatmapLevel, beatmapKey);
        }

        private async Task<BeatmapLevel?> GetBeatmapLevelByHashAsync(string hash, CancellationToken token) {
            string fixedHash = _levelsModel._allLoadedBeatmapLevelsRepository._idToBeatmapLevel.Keys.FirstOrDefault(k => k.StartsWith(hash));

            if (fixedHash != null) {
                hash = fixedHash;
            }

            if (await _levelsModel.CheckBeatmapLevelDataExistsAsync(hash, BeatmapLevelDataVersion.Original, token)) {
                return _levelsModel.GetBeatmapLevel(hash);
            } else if (await _levelsModel.CheckBeatmapLevelDataExistsAsync(CustomLevelLoader.kCustomLevelPrefixId + hash, BeatmapLevelDataVersion.Original, token)) {
                return _levelsModel.GetBeatmapLevel(CustomLevelLoader.kCustomLevelPrefixId + hash);
            }

            return null;
        }

        private static void Reinit(ReplayLaunchData data, BeatmapLevel? beatmapLevel = null, BeatmapKey? beatmapKey = null, EnvironmentInfoSO? environment = null) {
            data.Init(data.Replays, data.ReplayComparator, data.Settings, beatmapLevel ?? data.BeatmapLevel, beatmapKey ?? data.BeatmapKey, environment ?? data.EnvironmentInfo);
        }

        #endregion
    }
}