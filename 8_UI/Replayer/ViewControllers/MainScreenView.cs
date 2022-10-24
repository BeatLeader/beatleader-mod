using BeatLeader.Models;
using BeatLeader.Replayer.Camera;
using BeatLeader.Replayer.Emulation;
using BeatSaberMarkupLanguage.Attributes;
using Zenject;

namespace BeatLeader.Components {
    internal class MainScreenView : ReeUIComponentV2WithContainer {
        [Inject] private readonly IReplayPauseController _pauseController;
        [Inject] private readonly IReplayExitController _exitController;
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController;
        [Inject] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly VRControllersProvider _controllersProvider;
        [Inject] private readonly ReplayWatermark _watermark;
        [Inject] private readonly ReplayLaunchData _launchData;
        [Inject] private readonly SongSpeedData _speedData;

        [UIValue("song-info")] private HorizontalBeatmapLevelPreview _songInfo;
        [UIValue("player-info")] private HorizontalMiniProfile _playerInfo;
        [UIValue("toolbar")] private Toolbar _toolbar;
        [UIValue("layout-editor")] private LayoutEditor _layoutEditor;
        [UIValue("settings-modal")] private SettingsModal _settingsModal;

        protected override void OnInstantiate() {
            _playerInfo = Instantiate<HorizontalMiniProfile>(transform);
            _songInfo = Instantiate<HorizontalBeatmapLevelPreview>(transform);
            _settingsModal = Instantiate<SettingsModal>(transform);
            _toolbar = Instantiate<Toolbar>(transform);
            _layoutEditor = InstantiateInContainer<LayoutEditor>(Container, transform);

            _playerInfo.SetPlayer(_launchData.Player);
            _songInfo.SetBeatmapLevel(_launchData.DifficultyBeatmap.level);
            _settingsModal.Setup(_beatmapTimeController, _speedData, 
                _cameraController, _controllersProvider, _watermark, _layoutEditor, _launchData);
            _toolbar.Setup(_launchData.Replay, _pauseController, _exitController, _beatmapTimeController);
        }
        protected override void OnInitialize() {
            _layoutEditor.TryAddObject(_playerInfo);
            _layoutEditor.TryAddObject(_songInfo);
            _layoutEditor.TryAddObject(_toolbar);

            _toolbar.SettingsButtonClickedEvent += _settingsModal.ShowModal;
            _layoutEditor.EditModeChangedEvent += x => _settingsModal.HideModalImmediate();
        }
    }
}
