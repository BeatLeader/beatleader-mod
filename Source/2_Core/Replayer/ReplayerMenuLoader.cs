using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API.Methods;
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
        #region LoadData

        internal class LoadData {
            public enum Type {
                Score,
                File
            }

            public LoadData(Score score) {
                this.score = score;
                type = Type.Score;
            }

            public LoadData(string filePath) {
                this.filePath = filePath;
                type = Type.File;
            }

            public readonly Type type;
            public readonly Score? score;
            public readonly string? filePath;
        }

        #endregion

        #region Input Events

        private static event Action<LoadData>? DataWasSelectedEvent;
        private static event Action? PlayButtonWasPressed;
        private static event Action? PlayLastButtonWasPressed;

        internal static void NotifyDataWasSelected(LoadData data) {
            DataWasSelectedEvent?.Invoke(data);
        }

        internal static void NotifyPlayButtonWasPressed() {
            PlayButtonWasPressed?.Invoke();
        }

        internal static void NotifyPlayLastButtonWasPressed() {
            PlayLastButtonWasPressed?.Invoke();
        }

        #endregion

        #region State

        internal delegate void StateChangedDelegate(LoaderState state, Score? score, Replay? replay);

        private static event StateChangedDelegate? StateChangedEvent;

        private static LoaderState State { get; set; } = LoaderState.Uninitialized;
        private static Score? Score { get; set; }
        private static Replay? Replay { get; set; }

        internal static void AddStateListener(StateChangedDelegate handler) {
            StateChangedEvent += handler;
            handler?.Invoke(State, Score, Replay);
        }

        internal static void RemoveStateListener(StateChangedDelegate handler) {
            StateChangedEvent -= handler;
        }

        private static void SetState(LoaderState state) {
            State = state;
            StateChangedEvent?.Invoke(State, Score, Replay);
        }

        internal enum LoaderState {
            Uninitialized,
            DownloadRequired,
            Downloading,
            ReadyToPlay,
            Started,
        }

        #endregion

        #region Events Subscription

        private void Awake() {
            DataWasSelectedEvent += OnDataWasSelected;
            PlayButtonWasPressed += OnPlayButtonWasPressed;
            PlayLastButtonWasPressed += OnPlayLastButtonWasPressed;
            DownloadReplayRequest.AddStateListener(OnDownloadRequestStateChanged);
        }

        private void OnDestroy() {
            DataWasSelectedEvent -= OnDataWasSelected;
            PlayButtonWasPressed -= OnPlayButtonWasPressed;
            PlayLastButtonWasPressed -= OnPlayLastButtonWasPressed;
            DownloadReplayRequest.RemoveStateListener(OnDownloadRequestStateChanged);
        }

        #endregion

        #region Events

        private int _downloadReplayScoreId = -1;

        private void OnDataWasSelected(LoadData data) {
            var type = data.type;
            switch (type) {
                case LoadData.Type.Score:
                    Score = data.score;
                    var storedReplayAvailable = ReplayerCache
                        .TryReadReplay(Score!.id, out var storedReplay);
                    Replay = storedReplayAvailable ? storedReplay : default;
                    SetState(storedReplayAvailable ? LoaderState.ReadyToPlay : LoaderState.DownloadRequired);
                    break;
                case LoadData.Type.File:
                    Score = null;
                    if (!FileManager.TryReadReplay(data.filePath!, out var replay)) {
                        throw new ArgumentException("Unable to read the replay at " + data.filePath);
                    }
                    Replay = replay;
                    SetState(LoaderState.ReadyToPlay);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, string.Empty);
            }
        }

        private void OnDownloadRequestStateChanged(API.RequestState requestState, Replay result, string failReason) {
            if (State is LoaderState.Uninitialized || requestState is not API.RequestState.Finished || _downloadReplayScoreId != Score!.id) return;

            if (PluginConfig.EnableReplayCaching) {
                ReplayerCache.TryWriteReplay(Score.id, result);
            }

            Replay = result;
            SetState(LoaderState.ReadyToPlay);
            StartReplay(Score.player);
        }

        private void OnPlayButtonWasPressed() {
            switch (State) {
                case LoaderState.ReadyToPlay:
                    SetState(LoaderState.Started);
                    StartReplay(Score?.player);
                    break;
                case LoaderState.DownloadRequired:
                    _downloadReplayScoreId = Score!.id;
                    SetState(LoaderState.Downloading);
                    DownloadReplayRequest.SendRequest(Score.replay);
                    SendViewReplayRequest.SendRequest(Score.id);
                    break;
            }
        }

        private async void OnPlayLastButtonWasPressed() {
            if (ReplayManager.Instance.LastSavedReplay is not { } header) return;
            var replay = await header.LoadReplayAsync(default);
            _ = StartReplayAsync(replay!, ProfileManager.Profile);
        }

        #endregion

        #region StartReplay

        [Inject] private readonly ReplayerLauncher _launcher = null!;
        [Inject] private readonly GameScenesManager _scenesManager = null!;
        [Inject] private readonly IFPFCSettings _fpfcSettings = null!;
        [Inject] private readonly BeatmapLevelsModel _levelsModel = null!;

        private void StartReplay(Player? player) {
            _ = StartReplayAsync(Replay!, player);
        }

        public async Task StartReplayAsync(Replay replay, Player? player = null, ReplayerSettings? settings = null) {
            await StartReplayAsync(replay, player, settings, CancellationToken.None);
        }

        public async Task StartReplayAsync(Replay replay, Player? player, ReplayerSettings? settings, CancellationToken token) {
            settings ??= ReplayerSettings.UserSettings;
            var data = new ReplayLaunchData();
            var info = replay.info;
            Plugin.Log.Info("Attempting to load replay:\r\n" + info);
            await LoadBeatmapAsync(data, info.hash, info.mode, info.difficulty, token);
            if (settings.LoadPlayerEnvironment) LoadEnvironment(data, info.environment);
            var creplay = ReplayDataUtils.ConvertToAbstractReplay(replay, player);
            data.Init(creplay, ReplayDataUtils.BasicReplayComparator,
                settings, data.DifficultyBeatmap, data.EnvironmentInfo);
            StartReplay(data);
        }

        public void StartReplay(ReplayLaunchData data) {
            data.ReplayWasFinishedEvent += HandleReplayWasFinished;
            if (!_launcher.StartReplay(data, static () => SetState(LoaderState.ReadyToPlay))) return;
            InputUtils.forceFPFC = InputUtils.containsFPFCArg && _fpfcSettings.Ignore ? _fpfcSettings.Enabled : null;
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

        private async Task<IDifficultyBeatmap?> LoadBeatmapAsync(
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
            return (await _levelsModel.GetBeatmapLevelAsync(CustomLevelLoader.kCustomLevelPrefixId + hash, token)).beatmapLevel;
        }

        private static void Reinit(ReplayLaunchData data, IDifficultyBeatmap? beatmap = null, EnvironmentInfoSO? environment = null) {
            data.Init(data.Replays, data.ReplayComparator, data.Settings, beatmap ?? data.DifficultyBeatmap, environment ?? data.EnvironmentInfo);
        }

        #endregion
    }
}