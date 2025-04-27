using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.DataManager;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Models.Replay;
using BeatLeader.UI.Hub;
using BeatLeader.Utils;
using BeatSaber.BeatAvatarSDK;
using BeatSaber.BeatAvatarSDK;
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
            BattleRoyaleReplayData? optionalData = null,
            ReplayerSettings? settings = null,
            Action? finishCallback = null,
            CancellationToken token = default
        ) {
            settings ??= ReplayerSettings.UserSettings;

            var info = replay.info;
            var beatmap = await LoadBeatmapAsync(info.hash, info.mode, info.difficulty, token);

            if (!beatmap.HasValue) {
                return false;
            }

            if (!optionalData.HasValue) {
                var data = await LoadOptionalDataAsync(replay.info, player);
                optionalData = data;
            }

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
            IReadOnlyCollection<IBattleRoyaleReplay> replays,
            ReplayerSettings? settings = null,
            Action? finishCallback = null,
            CancellationToken token = default
        ) {
            if (replays.Count == 0) {
                return false;
            }

            settings ??= ReplayerSettings.UserSettings;

            var info = replays.First().ReplayHeader.ReplayInfo;
            var beatmap = await LoadBeatmapAsync(info.SongHash, info.SongMode, info.SongDifficulty, token);

            if (!beatmap.HasValue) {
                return false;
            }

            var replayDatas = new List<ReplayData>();
            var tasks = replays.Select(x => LoadAndAppendReplay(x, replayDatas));

            // wait for all tasks and return if at least one of them failed
            foreach (var task in tasks) {
                if (!await task) {
                    return false;
                }
            }

            StartReplayer(beatmap, replayDatas, settings, finishCallback, token);
            return true;
        }

        private static async Task<bool> LoadAndAppendReplay(IBattleRoyaleReplay royaleReplay, ICollection<ReplayData> collection) {
            var header = royaleReplay.ReplayHeader;
            
            var replay = await header.LoadReplayAsync(CancellationToken.None);

            if (replay == null) {
                return false;
            }

            var player = await header.LoadPlayerAsync(false, CancellationToken.None);

            var data = new ReplayData {
                replay = replay,
                player = player,
                optionalData = await LoadOptionalDataAsync(replay.info, player)
            };

            collection.Add(data);
            return true;
        }

        #endregion

        #region StartReplayer

        private struct ReplayData {
            public Replay replay;
            public IPlayer? player;
            public BattleRoyaleReplayData? optionalData;
        }

        private void StartReplayer(
            BeatmapLevelWithKey beatmap,
            Replay replay,
            IPlayer? player,
            BattleRoyaleReplayData? optionalData,
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
            InputUtils.OverrideUsesFPFC = InputUtils.HasFpfcArg && _fpfcSettings.Ignore ? _fpfcSettings.Enabled : null;
        }

        public async Task StartLastReplayAsync() {
            if (Instance == null || ReplayManager.LastSavedReplay is not { } header) {
                return;
            }

            var replay = await header.LoadReplayAsync(CancellationToken.None);

            await StartReplayAsync(replay!, ProfileManager.Profile);
        }

        private void HandleReplayWasFinished(StandardLevelScenesTransitionSetupDataSO transitionData, ReplayLaunchData launchData) {
            launchData.ReplayWasFinishedEvent -= HandleReplayWasFinished;
            _scenesManager.PopScenes(0.3f);

            _fpfcSettings.Enabled = InputUtils.OverrideUsesFPFC ?? InputUtils.HasFpfcArg;
            InputUtils.OverrideUsesFPFC = null;
            InputUtils.EnableCursor(!InputUtils.HasFpfcArg);
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
                CancellationToken.None
            ) is var beatmap && SongCoreInterop.ValidateRequirements(beatmap);
        }

        public async Task<bool> LoadBeatmapAsync(
            ReplayLaunchData launchData,
            string hash,
            string mode,
            string difficulty,
            CancellationToken token
        ) {
            var beatmap = await LoadBeatmapAsync(hash, mode, difficulty, token);
            if (!beatmap.HasValue) {
                return false;
            }

            launchData.BeatmapLevel = beatmap;
            return true;
        }

        public bool LoadEnvironment(ReplayLaunchData launchData, string environmentName) {
            if (environmentName == "Multiplayer") { 
               Plugin.Log.Notice("[ReplayerLoader] Map was played in MP. Skipping \"Multiplayer\" environment");
               return false; 
            }

            var environment = Resources.FindObjectsOfTypeAll<EnvironmentInfoSO>()
                .FirstOrDefault(x => x.environmentName == environmentName);

            if (environment == null) {
                Plugin.Log.Error("[ReplayerLoader] Failed to load specified environment");
                return false;
            }

            Plugin.Log.Notice("[ReplayerLoader] Applied specified environment: " + environmentName);

            launchData.EnvironmentInfo = environment;
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

        public async Task<BeatmapLevel?> GetBeatmapLevelByHashAsync(string hash, CancellationToken token) {
            if (hash.Length == 40) {
                var fixedHash = _levelsModel._allLoadedBeatmapLevelsRepository._idToBeatmapLevel.Keys.FirstOrDefault(k => k.StartsWith(hash));

                if (fixedHash != null) {
                    hash = fixedHash;
                }
            }

            if (await _levelsModel.CheckBeatmapLevelDataExistsAsync(hash, BeatmapLevelDataVersion.Original, token)) {
                return _levelsModel.GetBeatmapLevel(hash);
            }
            if (await _levelsModel.CheckBeatmapLevelDataExistsAsync(CustomLevelLoader.kCustomLevelPrefixId + hash, BeatmapLevelDataVersion.Original, token)) {
                return _levelsModel.GetBeatmapLevel(CustomLevelLoader.kCustomLevelPrefixId + hash);
            }

            return null;
        }

        #endregion

        #region Static Tools

        private static async Task<BattleRoyaleReplayData> LoadOptionalDataAsync(IReplayInfo? replayInfo, IPlayer? player) {
            Color? accentColor = null;
            AvatarData? avatarData = null;

            if (player != null) {
                avatarData = await player.GetBeatAvatarAsync(false, CancellationToken.None);
            }

            if (replayInfo != null) {
                var colorSeed = $"{replayInfo.Timestamp}{replayInfo.PlayerID}{replayInfo.SongName}".GetHashCode();
                accentColor = ColorUtils.RandomColor(rand: new(colorSeed));
            }

            return new BattleRoyaleReplayData(avatarData, accentColor);
        }

        #endregion
    }
}