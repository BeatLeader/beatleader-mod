using BeatLeader.API.Methods;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class ReplayPanel : ReeUIComponentV2 {
        #region Components

        [UIValue("settings-panel"), UsedImplicitly]
        private ReplayerSettingsPanel _settingsPanel;

        private void Awake() {
            _settingsPanel = Instantiate<ReplayerSettingsPanel>(transform);
        }

        #endregion

        #region Initialize/Dispose

        protected override void OnInitialize() {
            InitializePlayButton();

            DownloadReplayRequest.AddProgressListener(OnDownloadProgressChanged);
            ReplayerMenuLoader.AddStateListener(OnLoaderStateChanged);
            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapChanged);
        }

        protected override void OnDispose() {
            DownloadReplayRequest.RemoveProgressListener(OnDownloadProgressChanged);
            ReplayerMenuLoader.RemoveStateListener(OnLoaderStateChanged);
            LeaderboardState.RemoveSelectedBeatmapListener(OnSelectedBeatmapChanged);
        }

        #endregion

        #region SetScore

        public void SetScore(Score score) {
            ReplayerMenuLoader.NotifyScoreWasSelected(score);
        }

        #endregion

        #region Events

        private void OnSelectedBeatmapChanged(bool selectedAny, LeaderboardKey leaderboardKey, IDifficultyBeatmap beatmap) {
            if (!SongCoreInterop.TryGetBeatmapRequirements(beatmap, out var requirements)
                || !SongCoreInterop.TryGetCapabilities(out var capabilities)) return;

            bool interactable = true;
            foreach (var item in requirements) {
                if (!capabilities.Contains(item)) {
                    interactable = false;
                    break;
                }
            }

            _buttonShouldBeInteractable = interactable;
        }

        private void OnLoaderStateChanged(ReplayerMenuLoader.LoaderState state, Score score, Replay replay1) {
            _playButton.interactable = state is not ReplayerMenuLoader.LoaderState.Downloading;

            switch (state) {
                case ReplayerMenuLoader.LoaderState.DownloadRequired:
                case ReplayerMenuLoader.LoaderState.ReadyToPlay:
                    _playButton.SetButtonText("Watch Replay");
                    break;
            }
        }

        private void OnDownloadProgressChanged(float uploadProgress, float downloadProgress, float overallProgress) {
            _playButton.SetButtonText($"{downloadProgress * 100:F0}<size=60%>%");
        }

        #endregion

        #region Play button

        [UIComponent("play-button"), UsedImplicitly]
        private Button _playButton;

        private bool _buttonShouldBeInteractable;

        private void InitializePlayButton() {
            _playButton.onClick.AddListener(ReplayerMenuLoader.NotifyPlayButtonWasPressed);
        }

        #endregion

        #region SetActive

        public void SetActive(bool value) {
            Active = value;
            _playButton.interactable = _buttonShouldBeInteractable;
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