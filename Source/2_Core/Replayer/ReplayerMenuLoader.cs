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
using BeatLeader.UI.Hub;
using BeatLeader.Utils;
using JetBrains.Annotations;
using ModestTree;
using SiraUtil.Tools.FPFC;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    [PublicAPI]
    public class ReplayerMenuLoader : MonoBehaviour {
        #region Injection

        [Inject] private readonly ReplayerLauncher _launcher = null!;
        [Inject] private readonly GameScenesManager _scenesManager = null!;
        [Inject] private readonly IFPFCSettings _fpfcSettings = null!;
        [Inject] private readonly BeatmapLevelsModel _levelsModel = null!;
        [Inject] private readonly IReplayManager _replayManager = null!;

        #endregion

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

        #region StartReplayFromLeaderboard

        internal async Task StartReplayFromLeaderboardAsync(Replay replay, Player player) {
            var info = replay.info;
            var beatmapHash = info.hash;
            //try to load the beatmap, first attempt with replay hash, then with leaderboard hash if it fails
            var beatmap = await LoadBeatmapForLeaderboardAsync(beatmapHash, info.mode, info.difficulty);
            //if the beatmap still fails to load, force load the selected beatmap
            if (beatmap == null) {
                Plugin.Log.Warn("Beatmap load failed after two attempts; forcing selected beatmap load...");
                beatmap = LeaderboardState.SelectedBeatmap;
            }
            //start the replay
            StartReplayer(
                beatmap,
                replay,
                player,
                null,
                ReplayerSettings.UserSettings,
                CancellationToken.None
            );
        }

        private async Task<IDifficultyBeatmap?> LoadBeatmapForLeaderboardAsync(string beatmapHash, string mode, string difficulty) {
            var beatmap = await LoadBeatmapAsync(beatmapHash, mode, difficulty, CancellationToken.None);
            //if fails try to load with selected beatmap hash
            if (beatmap == null) {
                Plugin.Log.Warn("Failed to load the map by hash; attempting to use leaderboard hash...");
                beatmapHash = LeaderboardState.SelectedBeatmapKey.Hash;
                beatmap = await LoadBeatmapAsync(beatmapHash, mode, difficulty, CancellationToken.None);
            }
            //
            return beatmap;
        }

        #endregion

        #region StartReplay

        public async Task<bool> StartReplayAsync(
            Replay replay,
            IPlayer? player = null,
            IOptionalReplayData? optionalData = null,
            ReplayerSettings? settings = null,
            CancellationToken token = default
        ) {
            settings ??= ReplayerSettings.UserSettings;
            //loading beatmap
            var info = replay.info;
            var beatmap = await LoadBeatmapAsync(info.hash, info.mode, info.difficulty, token);
            if (beatmap == null) return false;
            //starting
            StartReplayer(
                beatmap,
                replay,
                player,
                optionalData,
                settings,
                token
            );
            return true;
        }

        public async Task<bool> StartBattleRoyaleAsync(
            IReadOnlyCollection<IBattleRoyaleReplayBase> replays,
            ReplayerSettings? settings,
            CancellationToken token
        ) {
            if (replays.Count == 0) return false;
            settings ??= ReplayerSettings.UserSettings;
            //loading beatmap
            var info = replays.First().ReplayHeader.ReplayInfo;
            var beatmap = await LoadBeatmapAsync(info.SongHash, info.SongMode, info.SongDifficulty, token);
            if (beatmap == null) return false;
            //loading replays
            var replayDatas = new List<ReplayData>();
            var tasks = replays.Select(x => LoadAndAppendReplay(x, replayDatas)).ToArray();
            await Task.WhenAll(tasks);
            //checking is everything okay
            if (tasks.Any(static x => !x.Result)) return false;
            //starting
            StartReplayer(beatmap, replayDatas, settings, token);
            return true;
        }

        private static async Task<bool> LoadAndAppendReplay(IBattleRoyaleReplayBase battleReplay, ICollection<ReplayData> collection) {
            var header = battleReplay.ReplayHeader;
            //loading replay
            var replay = await header.LoadReplayAsync(CancellationToken.None);
            if (replay == null) return false;
            //loading player
            var player = await header.LoadPlayerAsync(false, CancellationToken.None);
            //creating data
            var data = new ReplayData {
                replay = replay,
                player = player,
                optionalData = await battleReplay.GetReplayDataAsync()
            };
            collection.Add(data);
            return true;
        }

        #endregion

        #region StartReplayer

        private struct ReplayData {
            public Replay replay;
            public IPlayer? player;
            public IOptionalReplayData? optionalData;
        }

        private void StartReplayer(
            IDifficultyBeatmap beatmap,
            Replay replay,
            IPlayer? player,
            IOptionalReplayData? optionalData,
            ReplayerSettings settings,
            CancellationToken token
        ) {
            StartReplayer(
                beatmap,
                new ReplayData {
                    replay = replay,
                    player = player,
                    optionalData = optionalData
                }.Yield(),
                settings,
                token
            );
        }

        private void StartReplayer(
            IDifficultyBeatmap beatmap,
            IEnumerable<ReplayData> replays,
            ReplayerSettings settings,
            CancellationToken token
        ) {
            var data = new ReplayLaunchData();
            var list = new List<IReplay>();
            //loading replays
            foreach (var replayData in replays) {
                //adding extra info
                var info = replayData.replay.info;
                Plugin.Log.Info("Attempting to load replay:\r\n" + info);
                ReplayManager.SaturateReplayInfo(info, null);
                //loading environment
                if (settings.LoadPlayerEnvironment) {
                    LoadEnvironment(data, info.environment);
                }
                //converting
                var creplay = ReplayDataUtils.ConvertToAbstractReplay(
                    replayData.replay,
                    replayData.player,
                    replayData.optionalData
                );
                list.Add(creplay);
            }
            //initializing data
            data.Init(
                list,
                ReplayDataUtils.BasicReplayComparator,
                settings,
                beatmap,
                data.EnvironmentInfo
            );
            //starting
            StartReplayer(data);
        }

        public void StartReplayer(ReplayLaunchData data) {
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
            return await LoadBeatmapAsync(
                info.hash,
                info.mode,
                info.difficulty,
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