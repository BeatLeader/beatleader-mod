using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerUIPanel : ReactiveComponent {
        #region UI Components

        private readonly LayoutEditor _layoutEditor = new();
        private LayoutGrid _layoutGrid = null!;

        private readonly BeatmapLevelPreviewEditorComponent _songInfo = new();
        private readonly PlayerListEditorComponent _playerList = new();
        private readonly ToolbarEditorComponent _toolbar = new();

        #endregion

        #region LayoutEditor

        public void SwitchPartialDisplayMode() {
            _layoutEditor.PartialDisplayModeActive = !_layoutEditor.PartialDisplayModeActive;
        }

        #endregion

        #region Setup

        private IReplayPauseController _pauseController = null!;

        public void Setup(
            IReplayPauseController pauseController,
            IReplayFinishController finishController,
            IReplayTimeController timeController,
            IVirtualPlayersManager playersManager,
            ICameraController cameraController,
            IVirtualPlayerBodySpawner bodySpawner,
            ReplayLaunchData launchData,
            IReplayWatermark watermark
        ) {
            _pauseController = pauseController;
            _songInfo.SetBeatmapLevel(launchData.DifficultyBeatmap.level);
            _toolbar.Setup(
                pauseController,
                finishController,
                timeController,
                playersManager,
                cameraController,
                bodySpawner,
                launchData,
                _layoutEditor,
                watermark
            );
            _playerList.Setup(playersManager, timeController);
            var settings = launchData.Settings.UISettings.LayoutEditorSettings ??= new();
            _layoutEditor.Setup(settings);
            _layoutEditor.SetEditorActive(false, false);
        }

        protected override void Construct(RectTransform rect) {
            _songInfo.Use(rect);
            _toolbar.Use(rect);
            _playerList.Use(rect);
            _layoutEditor.WithRectExpand().Use(rect);

            var editorWindow = new LayoutEditorWindow();
            editorWindow.Use(rect);
            _layoutGrid = rect.gameObject.AddComponent<LayoutGrid>();
            _layoutEditor.AdditionalComponentHandler = _layoutGrid;
            _layoutEditor.StateChangedEvent += HandleLayoutEditorStateChanged;
            _layoutEditor.AddComponent(editorWindow);
            _layoutEditor.AddComponent(_toolbar);
            _layoutEditor.AddComponent(_songInfo);
            _layoutEditor.AddComponent(_playerList);
        }

        protected override void OnStart() {
            _layoutEditor.RefreshComponents();
        }

        #endregion

        #region Callbacks

        private void HandleLayoutEditorStateChanged(bool active) {
            _layoutGrid.enabled = active;
            if (active) _pauseController.Pause();
            _pauseController.LockUnpause = active;
            _layoutGrid.Refresh();
        }

        #endregion
    }
}