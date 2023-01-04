using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections;
using UnityEngine;

namespace BeatLeader.Components {
    internal class MainScreenView : ReeUIComponentV2 {
        #region Events

        public event Action? LayoutBuiltEvent;

        #endregion

        #region UI Components

        [UIValue("song-info")]
        private HorizontalBeatmapLevelPreview _songInfo = null!;

        [UIValue("player-info")]
        private HorizontalMiniProfile _playerInfo = null!;

        [UIValue("toolbar")]
        private ToolbarWithSettings _toolbar = null!;

        [UIValue("layout-editor")] 
        private LayoutEditor _layoutEditor = null!;

        [UIComponent("container-group")]
        private readonly RectTransform _containerRect = null!;

        #endregion

        #region Setup

        private IReplayPauseController _pauseController = null!;
        private bool _isInitialized;

        public void Setup(
            IReplayPauseController pauseController,
            IReplayFinishController finishController,
            IBeatmapTimeController beatmapTimeController,
            IVirtualPlayersManager playersManager,
            IViewableCameraController cameraController,
            ReplayLaunchData launchData,
            IReplayWatermark? watermark = null) {
            OnDispose();
            _pauseController = pauseController;
            _songInfo.SetBeatmapLevel(launchData.DifficultyBeatmap!.level);
            _toolbar.Setup(beatmapTimeController, pauseController,
                finishController, playersManager, cameraController,
                launchData, watermark, _layoutEditor);

            if (!launchData.IsBattleRoyale) {
                var player = launchData.Replays[0].Key;
                if (player != null) _playerInfo.SetPlayer(player);
            }
            _isInitialized = true;
        }

        protected override void OnInstantiate() {
            _playerInfo = Instantiate<HorizontalMiniProfile>(transform);
            _songInfo = Instantiate<HorizontalBeatmapLevelPreview>(transform);
            _toolbar = Instantiate<ToolbarWithSettings>(transform);
            _layoutEditor = Instantiate<LayoutEditor>(transform);
            _layoutEditor.EditModeChangedEvent += HandleLayoutEditorEditModeChanged;
        }

        protected override void OnInitialize() {
            var configInstance = LayoutEditorConfig.Instance;
            _layoutEditor.gridCellSize = configInstance.CellSize;
            _layoutEditor.gridLineThickness = configInstance.LineThickness;
            _layoutEditor.layoutMapsSource = configInstance;

            _layoutEditor.Setup(_containerRect);
            _layoutEditor.Add(_playerInfo, _toolbar, _songInfo);

            CoroutinesHandler.instance.StartCoroutine(MapLayoutCoroutine());
        }

        #endregion

        #region Layout

        public void OpenLayoutEditor() {
            if (!_isInitialized) return;
            _layoutEditor.SetEditorEnabled(true);
            _pauseController.Pause();
        }

        private IEnumerator MapLayoutCoroutine() {
            yield return new WaitForEndOfFrame();
            _layoutEditor.MapLayout();
            LayoutBuiltEvent?.Invoke();
        }

        #endregion

        #region Callbacks

        private void HandleLayoutEditorEditModeChanged(bool enabled) {
            _pauseController.LockUnpause = enabled;
        }

        #endregion
    }
}
