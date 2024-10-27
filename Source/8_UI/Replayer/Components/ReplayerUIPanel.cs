using BeatLeader.Components;
using BeatLeader.Models;
using Reactive;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerUIPanel : ReactiveComponent {
        #region UI Components

        private readonly LayoutEditor _layoutEditor = new();

        private readonly BeatmapLevelPreviewEditorComponent _songInfo = new();
        private readonly PlayerListEditorComponent _playerList = new();
        private readonly ToolbarEditorComponent _toolbar = new();

        #endregion

        #region LayoutEditor

        public void SwitchViewMode() {
            var mode = _layoutEditor.Mode;
            _layoutEditor.Mode = mode switch {
                LayoutEditorMode.View => LayoutEditorMode.ViewAll,
                LayoutEditorMode.ViewAll => LayoutEditorMode.View,
                _ => mode
            };
        }

        #endregion

        #region Setup

        private IReplayPauseController _pauseController = null!;
        private bool _started;

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
            _songInfo.SetBeatmapLevel(launchData.BeatmapLevel.Level);
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
            // Applying immediately only if the ui is loaded
            if (_started) {
                _layoutEditor.LoadLayoutFromSettings();
            }
        }

        protected override void Construct(RectTransform rect) {
            _songInfo.Use(rect);
            _toolbar.Use(rect);
            _playerList.Use(rect);
            _layoutEditor.WithRectExpand().Use(rect);

            var editorWindow = new LayoutEditorWindow();
            editorWindow.Use(rect);
            _layoutEditor.ModeChangedEvent += HandleLayoutEditorModeChanged;
            _layoutEditor.AddComponent(_toolbar);
            _layoutEditor.AddComponent(_songInfo);
            _layoutEditor.AddComponent(_playerList);
            _layoutEditor.AddComponent(editorWindow);
        }

        protected override void OnStart() {
            _started = true;
            _layoutEditor.LoadLayoutFromSettings();
        }

        #endregion

        #region Callbacks

        private void HandleLayoutEditorModeChanged(LayoutEditorMode mode) {
            var edit = mode is LayoutEditorMode.Edit;
            if (edit) _pauseController.Pause();
            _pauseController.LockUnpause = edit;
        }

        #endregion
    }
}