using BeatLeader.Models;
using BeatLeader.Replayer.Camera;
using BeatLeader.Replayer.Emulation;
using BeatSaberMarkupLanguage.Attributes;
using System.Collections;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components {
    internal class MainScreenView : ReeUIComponentV2WithContainer {
        [Inject] private readonly IReplayPauseController _pauseController;
        [Inject] private readonly IReplayExitController _exitController;
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController;
        [Inject] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly VRControllersAccessor _controllersAccessor;
        [Inject] private readonly ReplayWatermark _watermark;
        [Inject] private readonly ReplayLaunchData _launchData;
        [Inject] private readonly SongSpeedData _speedData;

        [UIValue("song-info")] private HorizontalBeatmapLevelPreview _songInfo;
        [UIValue("player-info")] private HorizontalMiniProfile _playerInfo;
        [UIValue("toolbar")] private ToolbarWithSettings _toolbar;
        [UIValue("layout-editor")] private LayoutEditor _layoutEditor;

        [UIComponent("container-group")]
        private readonly RectTransform _containerRect;

        public void OpenLayoutEditor() {
            _layoutEditor.SetEditorEnabled(true);
            _pauseController.Pause();
        }

        protected override void OnInstantiate() {
            _playerInfo = Instantiate<HorizontalMiniProfile>(transform);
            _songInfo = Instantiate<HorizontalBeatmapLevelPreview>(transform);
            _toolbar = Instantiate<ToolbarWithSettings>(transform);
            _layoutEditor = Instantiate<LayoutEditor>(transform);

            _playerInfo.SetPlayer(_launchData.Player);
            _songInfo.SetBeatmapLevel(_launchData.DifficultyBeatmap.level);
            _toolbar.Setup(_beatmapTimeController, _pauseController,
                _exitController, _launchData, _speedData, _cameraController,
                _controllersAccessor, _watermark, _layoutEditor);
        }
        protected override void OnInitialize() {
            _layoutEditor.layoutMapsSource = LayoutMapsConfig.Instance;
            _layoutEditor.Setup(_containerRect);
            _layoutEditor.Add(_playerInfo);
            _layoutEditor.Add(_toolbar);
            _layoutEditor.Add(_songInfo);

            CoroutinesHandler.instance.StartCoroutine(MapLayoutCoroutine());
        }

        private IEnumerator MapLayoutCoroutine() {
            yield return new WaitForEndOfFrame();
            _layoutEditor.RefreshLayout();
        }
    }
}
