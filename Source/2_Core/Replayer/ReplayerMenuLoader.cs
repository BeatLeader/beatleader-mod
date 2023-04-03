using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API.Methods;
using BeatLeader.DataManager;
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
        #region Input Events

        private static event Action<Score>? ScoreWasSelectedEvent;
        private static event Action? PlayButtonWasPressed;
        private static event Action? PlayLastButtonWasPressed;

        internal static void NotifyScoreWasSelected(Score score) {
            ScoreWasSelectedEvent?.Invoke(score);
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
            ReadyToPlay
        }

        #endregion

        #region Events Subscription

        private void Awake() {
            ScoreWasSelectedEvent += OnScoreWasSelected;
            PlayButtonWasPressed += OnPlayButtonWasPressed;
            PlayLastButtonWasPressed += OnPlayLastButtonWasPressed;
            DownloadReplayRequest.AddStateListener(OnDownloadRequestStateChanged);
        }

        private void OnDestroy() {
            ScoreWasSelectedEvent -= OnScoreWasSelected;
            PlayButtonWasPressed -= OnPlayButtonWasPressed;
            PlayLastButtonWasPressed -= OnPlayLastButtonWasPressed;
            DownloadReplayRequest.RemoveStateListener(OnDownloadRequestStateChanged);
        }

        #endregion

        #region Events

        private int _downloadReplayScoreId = -1;

        private void OnScoreWasSelected(Score score) {
            Score = score;
            var storedReplayAvailable = ReplayerCache.TryReadReplay(score.id, out var storedReplay);
            Replay = storedReplayAvailable ? storedReplay : default!;
            SetState(storedReplayAvailable ? LoaderState.ReadyToPlay : LoaderState.DownloadRequired);
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
                    StartReplay(Score!.player);
                    break;
                case LoaderState.DownloadRequired:
                    _downloadReplayScoreId = Score!.id;
                    SetState(LoaderState.Downloading);
                    DownloadReplayRequest.SendRequest(Score.replay);
                    SendViewReplayRequest.SendRequest(Score.id);
                    break;
            }
        }

        private void OnPlayLastButtonWasPressed() {
            if (FileManager.TryReadReplay(FileManager.LastSavedReplay, out var storedReplay)) {
                Replay = storedReplay;
                StartReplay(ProfileManager.Profile);
            }
        }

        #endregion

        #region StartReplay

        [Inject] private readonly ReplayerLauncher _launcher = null!;
        [Inject] private readonly GameScenesManager _scenesManager = null!;
        [Inject] private readonly IFPFCSettings _fpfcSettings = null!;
        [Inject] private readonly BeatmapLevelsModel _levelsModel = null!;

        private void StartReplay(Player player) {
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
            var creplay = ReplayDataHelper.ConvertToAbstractReplay(replay, player);
            data.Init(creplay, ReplayDataHelper.BasicReplayComparator,
                settings, data.DifficultyBeatmap, data.EnvironmentInfo);
            StartReplay(data);
        }
        
        public void StartReplay(ReplayLaunchData data) {
            data.ReplayWasFinishedEvent += HandleReplayWasFinished;
            if (!_launcher.StartReplay(data)) return;
            //InputUtils.forceFPFC = !_fpfcSettings.Ignore;
        }

        private void HandleReplayWasFinished(StandardLevelScenesTransitionSetupDataSO transitionData, ReplayLaunchData launchData) {
            launchData.ReplayWasFinishedEvent -= HandleReplayWasFinished;
            _scenesManager.PopScenes(0.3f);

            InputUtils.forceFPFC = null;
            InputUtils.EnableCursor(!InputUtils.containsFPFCArg);
            _fpfcSettings.Enabled = InputUtils.containsFPFCArg;
        }

        #endregion

        #region ReplayTools
        
        public async Task<bool> LoadBeatmapAsync(ReplayLaunchData launchData,
            string hash, string mode, string difficulty, CancellationToken token) {
            var beatmapLevel = await GetBeatmapLevelByHashAsync(hash, token);
            if (beatmapLevel == null || token.IsCancellationRequested
                || !Enum.TryParse(difficulty, out BeatmapDifficulty cdifficulty)) return false;

            var characteristic = beatmapLevel.beatmapLevelData
                .difficultyBeatmapSets.Select(static x => x.beatmapCharacteristic)
                .FirstOrDefault(x => x.serializedName == mode);
            if (characteristic == null || token.IsCancellationRequested) return false;

            var difficultyBeatmap = beatmapLevel.beatmapLevelData
                .GetDifficultyBeatmap(characteristic, cdifficulty);
            if (difficultyBeatmap == null || token.IsCancellationRequested) return false;

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

        private async Task<IBeatmapLevel?> GetBeatmapLevelByHashAsync(string hash, CancellationToken token) {
            return (await _levelsModel.GetBeatmapLevelAsync(CustomLevelLoader.kCustomLevelPrefixId + hash, token)).beatmapLevel;
        }

        private static void Reinit(ReplayLaunchData data, IDifficultyBeatmap? beatmap = null, EnvironmentInfoSO? environment = null) {
            data.Init(data.Replays, data.ReplayComparator, data.Settings, beatmap ?? data.DifficultyBeatmap, environment ?? data.EnvironmentInfo);
        }

        #endregion
    }
}