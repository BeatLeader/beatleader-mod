using BeatLeader.Models;
using BeatLeader.Replayer.Camera;
using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components {
    internal class MainScreenView : ReeUIComponentV2WithContainer {
        #region Injection

        [Inject] private readonly IReplayPauseController _pauseController;
        [Inject] private readonly IReplayExitController _exitController;
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController;
        [Inject] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly ReplayerControllersManager _controllersManager;
        [Inject] private readonly ReplayWatermark _watermark;
        [Inject] private readonly ReplayLaunchData _launchData;
        [Inject] private readonly SongSpeedData _speedData;

        #endregion

        #region Events

        public event Action LayoutBuiltEvent;

        #endregion

        #region UI Components

        [UIValue("song-info")] private HorizontalBeatmapLevelPreview _songInfo;
        [UIValue("player-info")] private HorizontalMiniProfile _playerInfo;
        [UIValue("toolbar")] private ToolbarWithSettings _toolbar;
        [UIValue("layout-editor")] private LayoutEditor _layoutEditor;

        [UIComponent("container-group")]
        private readonly RectTransform _containerRect;

        #endregion

        #region Setup

        protected override void OnInstantiate() {
            _playerInfo = Instantiate<HorizontalMiniProfile>(transform);
            _songInfo = Instantiate<HorizontalBeatmapLevelPreview>(transform);
            _toolbar = Instantiate<ToolbarWithSettings>(transform);
            _layoutEditor = Instantiate<LayoutEditor>(transform);

            _playerInfo.SetPlayer(_launchData.Player);
            _songInfo.SetBeatmapLevel(_launchData.DifficultyBeatmap.level);
            _toolbar.Setup(_beatmapTimeController, _pauseController,
                _exitController, _launchData, _speedData, _cameraController,
                _controllersManager, _watermark, _layoutEditor);
        }

        protected override void OnInitialize() {
            var configInstance = LayoutEditorConfig.Instance;
            _layoutEditor.gridCellSize = configInstance.CellSize;
            _layoutEditor.gridLineThickness = configInstance.LineThickness;
            _layoutEditor.layoutMapsSource = configInstance;

            _layoutEditor.Setup(_containerRect);
            _layoutEditor.Add(_playerInfo);
            _layoutEditor.Add(_toolbar);
            _layoutEditor.Add(_songInfo);

            CoroutinesHandler.instance.StartCoroutine(MapLayoutCoroutine());
        }

        #endregion

        #region Layout

        public void OpenLayoutEditor() {
            _layoutEditor.SetEditorEnabled(true);
            _pauseController.Pause();
        }

        private IEnumerator MapLayoutCoroutine() {
            yield return new WaitForEndOfFrame();
            _layoutEditor.RefreshLayout();
            LayoutBuiltEvent?.Invoke();
        }

        #endregion
    }
}
