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
using BeatSaber.BeatAvatarSDK;
using HMUI;
using JetBrains.Annotations;
using ModestTree;
using SiraUtil.Tools.FPFC;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    [PublicAPI]
    public class ReplayerMenuLoader : MonoBehaviour, IReplayerViewNavigator {
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

        #region NavigateToReplay

        Task IReplayerViewNavigator.NavigateToReplayAsync(
            FlowCoordinator flowCoordinator, 
            Replay replay,
            Player player,
            bool alternative
        ) {
            return alternative ? 
                StartReplayFromLeaderboardAsync(replay, player) : 
                StartReplayAsync(replay, player);
        }

        #endregion

        #region StartReplayFromLeaderboard

        internal async Task StartReplayFromLeaderboardAsync(Replay replay, Player player, Action? finishCallback = null) {
            var info = replay.info;
            var beatmapHash = info.hash;
            //try to load the beatmap, first attempt with replay hash, then with leaderboard hash if it fails
            var beatmap = await LoadBeatmapForLeaderboardAsync(beatmapHash, info.mode, info.difficulty);
            //if the beatmap still fails to load, force load the selected beatmap
            if (!beatmap.HasValue) {
                Plugin.Log.Warn("Beatmap load failed after two attempts; forcing selected beatmap load...");
                beatmap = new(LeaderboardState.SelectedBeatmapLevel, LeaderboardState.SelectedBeatmapKey);
            }
            var optionalData = await LoadOptionalDataAsync(replay.info, player);
            //start the replay
            StartReplayer(
                beatmap,
                replay,
                player,
                optionalData,
                ReplayerSettings.UserSettings,
                finishCallback,
                CancellationToken.None
            );
        }

        private async Task<BeatmapLevelWithKey> LoadBeatmapForLeaderboardAsync(string beatmapHash, string mode, string difficulty) {
            var beatmap = await LoadBeatmapAsync(beatmapHash, mode, difficulty, CancellationToken.None);
            //if fails try to load with selected beatmap hash
            if (!beatmap.HasValue) {
                Plugin.Log.Warn("Failed to load the map by hash; attempting to use leaderboard hash...");
                beatmapHash = LeaderboardState.SelectedLeaderboardKey.Hash;
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
            Optional<IOptionalReplayData?> optionalData = default,
            ReplayerSettings? settings = null,
            Action? finishCallback = null,
            CancellationToken token = default
        ) {
            settings ??= ReplayerSettings.UserSettings;
            //loading beatmap
            var info = replay.info;
            var beatmap = await LoadBeatmapAsync(info.hash, info.mode, info.difficulty, token);
            if (!beatmap.HasValue) return false;
            //loading data
            if (!optionalData.HasValue) {
                var data = await LoadOptionalDataAsync(replay.info, player);
                optionalData.SetValueIfNotSet(data);
            }
            //starting
            StartReplayer(
                beatmap,
                replay,
                player,
                optionalData.Value,
                settings,
                finishCallback,
                token
            );
            return true;
        }

        public async Task<bool> StartBattleRoyaleAsync(
            IReadOnlyCollection<IBattleRoyaleReplayBase> replays,
            ReplayerSettings? settings = null,
            Action? finishCallback = null,
            CancellationToken token = default
        ) {
            if (replays.Count == 0) return false;
            settings ??= ReplayerSettings.UserSettings;
            //loading beatmap
            var info = replays.First().ReplayHeader.ReplayInfo;
            var beatmap = await LoadBeatmapAsync(info.SongHash, info.SongMode, info.SongDifficulty, token);
            if (!beatmap.HasValue) return false;
            //loading replays
            var replayDatas = new List<ReplayData>();
            var tasks = replays.Select(x => LoadAndAppendReplay(x, replayDatas)).ToArray();
            await Task.WhenAll(tasks);
            //checking is everything okay
            if (tasks.Any(static x => !x.Result)) return false;
            //starting
            StartReplayer(beatmap, replayDatas, settings, finishCallback, token);
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
            BeatmapLevelWithKey beatmap,
            Replay replay,
            IPlayer? player,
            IOptionalReplayData? optionalData,
            ReplayerSettings settings,
            Action? finishCallback,
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
                finishCallback,
                token
            );
        }

        private void StartReplayer(
            BeatmapLevelWithKey beatmap,
            IEnumerable<ReplayData> replays,
            ReplayerSettings settings,
            Action? finishCallback,
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
            StartReplayer(data, finishCallback);
        }

        public void StartReplayer(ReplayLaunchData data, Action? finishCallback) {
            data.ReplayWasFinishedEvent += HandleReplayWasFinished;
            if (!_launcher.StartReplay(data, finishCallback)) return;
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

        private static BeatmapLevelWithKey _cachedBeatmap;
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
            var beatmap = await LoadBeatmapAsync(hash, mode, difficulty, token);
            if (!beatmap.HasValue) return false;
            Reinit(launchData, beatmap);
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

        public async Task<BeatmapLevelWithKey> LoadBeatmapAsync(
            string hash,
            string mode,
            string difficulty,
            CancellationToken token
        ) {
            if (!Enum.TryParse(difficulty, out BeatmapDifficulty cdifficulty)) {
                return default;
            }

            if (_cachedBeatmap is { HasValue: true }
                && _cachedBeatmapHash == hash
                && _cachedBeatmapCharacteristic == mode
                && _cachedBeatmap.Key.difficulty == cdifficulty
            ) {
                return _cachedBeatmap;
            }

            var beatmapLevel = await GetBeatmapLevelByHashAsync(hash, token);
            if (beatmapLevel == null) return default;

            var characteristic = beatmapLevel.GetCharacteristics()
                .FirstOrDefault(x => x.serializedName == mode);
            if (characteristic == null || token.IsCancellationRequested) return default;

            var beatmapKey = beatmapLevel.GetBeatmapKeys()
                .FirstOrDefault(k => k.beatmapCharacteristic == characteristic && k.difficulty == cdifficulty);
            if (beatmapKey == null || token.IsCancellationRequested) return default;

            _cachedBeatmap = new(beatmapLevel, beatmapKey);
            _cachedBeatmapHash = hash;
            _cachedBeatmapCharacteristic = mode;
            return _cachedBeatmap;
        }

        private async Task<BeatmapLevel?> GetBeatmapLevelByHashAsync(string hash, CancellationToken token) {
            var fixedHash = _levelsModel._allLoadedBeatmapLevelsRepository._idToBeatmapLevel.Keys.FirstOrDefault(k => k.StartsWith(hash));

            if (fixedHash != null) {
                hash = fixedHash;
            }

            if (await _levelsModel.CheckBeatmapLevelDataExistsAsync(hash, BeatmapLevelDataVersion.Original, token)) {
                return _levelsModel.GetBeatmapLevel(hash);
            }
            if (await _levelsModel.CheckBeatmapLevelDataExistsAsync(CustomLevelLoader.kCustomLevelPrefixId + hash, BeatmapLevelDataVersion.Original, token)) {
                return _levelsModel.GetBeatmapLevel(CustomLevelLoader.kCustomLevelPrefixId + hash);
            }

            return null;
        }

        private static void Reinit(ReplayLaunchData data, BeatmapLevelWithKey? beatmap = null, EnvironmentInfoSO? environment = null) {
            data.Init(data.Replays, data.ReplayComparator, data.Settings, beatmap ?? data.BeatmapLevel, environment ?? data.EnvironmentInfo);
        }

        #endregion

        #region Static Tools

        public static async Task<IOptionalReplayData> LoadOptionalDataAsync(IReplayInfo? replayInfo, IPlayer? player) {
            Color? accentColor = null;
            AvatarData? avatarData = null;
            if (player != null) {
                var avatar = await player.GetBeatAvatarAsync();
                avatarData = avatar.ToAvatarData();
            }
            if (replayInfo != null) {
                var colorSeed = $"{replayInfo.Timestamp}{replayInfo.PlayerID}{replayInfo.SongName}".GetHashCode();
                accentColor = ColorUtils.RandomColor(rand: new(colorSeed));
            }
            return new OptionalReplayData(avatarData, accentColor);
        }

        #endregion
    }
}