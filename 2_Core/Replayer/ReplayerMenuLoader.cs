using System;
using System.Threading.Tasks;
using BeatLeader.API.Methods;
using BeatLeader.DataManager;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Utils;
using JetBrains.Annotations;
using SiraUtil.Tools.FPFC;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    public class ReplayerMenuLoader : MonoBehaviour {
        #region Input Events

        private static event Action<Score> ScoreWasSelectedEvent;
        private static event Action PlayButtonWasPressed;
        private static event Action PlayLastButtonWasPressed;

        public static void NotifyScoreWasSelected(Score score) {
            ScoreWasSelectedEvent?.Invoke(score);
        }

        public static void NotifyPlayButtonWasPressed() {
            PlayButtonWasPressed?.Invoke();
        }

        public static void NotifyPlayLastButtonWasPressed() {
            PlayLastButtonWasPressed?.Invoke();
        }

        #endregion

        #region State

        public delegate void StateChangedDelegate(LoaderState state, Score? score, Replay? replay);

        private static event StateChangedDelegate? StateChangedEvent;

        public static LoaderState State { get; private set; } = LoaderState.Uninitialized;
        public static Score? Score { get; private set; }
        public static Replay? Replay { get; private set; }

        public static void AddStateListener(StateChangedDelegate handler) {
            StateChangedEvent += handler;
            handler?.Invoke(State, Score, Replay);
        }

        public static void RemoveStateListener(StateChangedDelegate handler) {
            StateChangedEvent -= handler;
        }

        private static void SetState(LoaderState state) {
            State = state;
            StateChangedEvent?.Invoke(State, Score, Replay);
        }

        public enum LoaderState {
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

        [Inject, UsedImplicitly]
        private readonly ReplayerLauncher _launcher = null!;

        [Inject, UsedImplicitly]
        private readonly GameScenesManager _scenesManager = null!;

        [Inject, UsedImplicitly]
        private readonly IFPFCSettings _fpfcSettings = null!;

        private void StartReplay(Player player) { 
            StartReplayAsync(Replay!, player, ConfigFileData.Instance.ReplayerSettings);
        }

        public async void StartReplayAsync(Replay replay, Player? player = null, ReplayerSettings? settings = null) {
            settings ??= ConfigFileData.Instance.ReplayerSettings;
            var data = new ReplayLaunchData();
            data.Init(replay, settings, player);
            await StartReplayAsync(data);
        }

        public async Task StartReplayAsync(ReplayLaunchData data) {
            data.ReplayWasFinishedEvent += HandleReplayWasFinished;

            Plugin.Log.Notice("[Loader] Download completed!");
            Plugin.Log.Info(data.ToString());

            if (await _launcher.StartReplayAsync(data)) {
                ScoreSaberInterop.RecordingEnabled = false;
                BeatSaviorInterop.ScoreSubmissionEnabled = false;
                //InputUtils.forceFPFC = !_fpfcSettings.Ignore;
            }
        }

        private void HandleReplayWasFinished(StandardLevelScenesTransitionSetupDataSO transitionData, ReplayLaunchData launchData) {
            launchData.ReplayWasFinishedEvent -= HandleReplayWasFinished;
            _scenesManager.PopScenes(0.3f);

            InputUtils.forceFPFC = null;
            InputUtils.EnableCursor(!InputUtils.containsFPFCArg);
            _fpfcSettings.Enabled = InputUtils.containsFPFCArg;
            ScoreSaberInterop.RecordingEnabled = true;
            BeatSaviorInterop.MarkScoreSubmissionToEnable();
        }

        #endregion
    }
}