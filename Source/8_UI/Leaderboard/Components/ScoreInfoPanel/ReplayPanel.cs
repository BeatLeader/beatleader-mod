using System;
using BeatLeader.API.Methods;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using BeatLeader.Models.Replay;
using BeatLeader.Replayer;
using TMPro;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class ReplayPanel : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("download-text"), UsedImplicitly]
        private TMP_Text _downloadText = null!;

        [UIComponent("play-button"), UsedImplicitly]
        private Button _playButton = null!;
        
        [UIComponent("play-button"), UsedImplicitly]
        private TMP_Text _playButtonText = null!;

        [UIValue("settings-panel"), UsedImplicitly]
        private ReplayerSettingsPanel _settingsPanel = null!;

        #endregion

        #region Events

        public event Action<bool>? DownloadStateChangedEvent;

        private void RefreshDownloadState(bool state) {
            DownloadStateChangedEvent?.Invoke(state);
        }
        
        #endregion

        #region Initialize/Dispose

        private IReplayerStarter? _replayerStarter;
        
        public void Setup(IReplayerStarter starter) {
            _replayerStarter = starter;
        }
        
        protected override void OnInstantiate() {
            _settingsPanel = Instantiate<ReplayerSettingsPanel>(transform);
        }

        protected override void OnInitialize() {
            InitializePlayButton();

            DownloadReplayRequest.AddProgressListener(OnDownloadProgressChanged);
            DownloadReplayRequest.AddStateListener(OnDownloadRequestStateChanged);
            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapChanged);
        }

        protected override void OnDispose() {
            DownloadReplayRequest.RemoveProgressListener(OnDownloadProgressChanged);
            LeaderboardState.RemoveSelectedBeatmapListener(OnSelectedBeatmapChanged);
        }

        #endregion

        #region SetScore

        private Score? _score;

        public void SetScore(Score score) {
            _score = score;
        }

        #endregion

        #region StartReplay

        private void StartReplay(Replay replay) {
            _replayerStarter!.StartReplay(replay, _score!.Player);
        }

        #endregion
        
        #region Callbacks

        private bool _blockIncomingEvents = true;
        private bool _isDownloading;

        private void OnSelectedBeatmapChanged(bool selectedAny, LeaderboardKey leaderboardKey, BeatmapKey key, BeatmapLevel level) {
            _buttonCanBeInteractable = SongCoreInterop.ValidateRequirements(level, key);
        }

        private void OnDownloadProgressChanged(float uploadProgress, float downloadProgress, float overallProgress) {
            if (_blockIncomingEvents) return;
            _downloadText.text = $"<alpha=#66>Downloading: {downloadProgress * 100:F0}%";
        }

        private void OnDownloadRequestStateChanged(API.RequestState state, Replay result, string failReason) {
            if (_blockIncomingEvents) return;
            _isDownloading = state is API.RequestState.Started;
            RefreshPlayButtonText(_isDownloading);
            RefreshDownloadState(_isDownloading);
            switch (state) {
                case API.RequestState.Finished:
                    _downloadText.text = "<alpha=#66>Finished!";
                    if (PluginConfig.EnableReplayCaching) ReplayerCache.TryWriteReplay(_score!.id, result);
                    SetPlayButtonInteractable(false);
                    StartReplay(result);
                    return;
                case API.RequestState.Failed:
                    _downloadText.text = FormatFailString(failReason);
                    return;
            }
        }

        private void OnPlayButtonClicked() {
            if (_isDownloading) {
                _blockIncomingEvents = true;
                _isDownloading = false;
                _downloadText.gameObject.SetActive(false);
                RefreshPlayButtonText(false);
                RefreshDownloadState(false);
                return;
            }
            if (_score is null) {
                _downloadText.text = FormatFailString("Score is unavailable!");
                return;
            }
            if (ReplayerCache.TryReadReplay(_score.id, out var storedReplay)) {
                StartReplay(storedReplay);
                return;
            }
            RefreshDownloadState(true);
            _blockIncomingEvents = false;
            _downloadText.gameObject.SetActive(true);
            DownloadReplayRequest.SendRequest(_score.replay);
            SendViewReplayRequest.SendRequest(_score.id);
        }

        #endregion

        #region Format

        private static string FormatFailString(string failReason) {
            return $"<color=red>Fail: {failReason}</color>";
        }

        #endregion

        #region PlayButton

        private bool _buttonCanBeInteractable = true;

        private void InitializePlayButton() {
            _playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private void RefreshPlayButtonText(bool downloading) {
            _playButtonText.text = downloading ? "<bll>ls-cancel</bll>" : "<bll>ls-watch-replay</bll>";
        }

        private void SetPlayButtonInteractable(bool interactable) {
            _playButton.interactable = interactable;
        }

        #endregion

        #region SetActive

        public void SetActive(bool value) {
            Active = value;
            _downloadText.gameObject.SetActive(false);
            SetPlayButtonInteractable(_buttonCanBeInteractable);
            RefreshPlayButtonText(false);
        }

        #endregion

        #region Active

        private bool _active = true;

        [UIValue("active"), UsedImplicitly]
        private bool Active {
            get => _active;
            set {
                if (_active.Equals(value)) return;
                _active = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}