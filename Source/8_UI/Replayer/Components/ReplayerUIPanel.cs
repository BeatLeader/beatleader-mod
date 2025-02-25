using BeatLeader.Components;
using BeatLeader.Models;
using Reactive;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerUIPanel : ReactiveComponent {
        #region UI Components

        private LayoutEditor _layoutEditor = null!;
        private BeatmapLevelPreviewEditorComponent _songInfo = null!;
        private ToolbarEditorComponent _toolbar = null!;

        private PlayerListEditorComponent? _playerList;
        private PlayerProfileEditorComponent? _playerProfile;

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

            if (launchData.IsBattleRoyale) {
                _playerList = new();
                _playerList.Use(ContentTransform);
                _playerList.Setup(playersManager, timeController);
                _layoutEditor.AddComponent(_playerList);
            } else {
                _playerProfile = new();
                _playerProfile.Use(ContentTransform);
                _playerProfile.Setup(launchData.MainReplay.ReplayData.Player);
                _layoutEditor.AddComponent(_playerProfile);
            }

            var settings = launchData.Settings.UISettings.LayoutEditorSettings ??= new();
            _layoutEditor.Setup(settings);
            // Applying immediately only if ui is loaded
            if (_started) {
                _layoutEditor.LoadLayoutFromSettings();
                _layoutEditor.Mode = LayoutEditorMode.ViewAll;
            }
        }

        protected override void Construct(RectTransform rect) {
            _layoutEditor = new();
            _songInfo = new();
            _toolbar = new();

            _songInfo.Use(rect);
            _toolbar.Use(rect);
            _layoutEditor.WithRectExpand().Use(rect);

            var editorWindow = new LayoutEditorWindow();
            editorWindow.Use(rect);
            _layoutEditor.ModeChangedEvent += HandleLayoutEditorModeChanged;
            _layoutEditor.AddComponent(_toolbar);
            _layoutEditor.AddComponent(_songInfo);
            _layoutEditor.AddComponent(editorWindow);
        }

        protected override void OnStart() {
            _started = true;
            _layoutEditor.LoadLayoutFromSettings();
            _layoutEditor.Mode = LayoutEditorMode.ViewAll;
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