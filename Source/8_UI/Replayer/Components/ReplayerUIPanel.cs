using BeatLeader.Components;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerUIPanel : ReeUIComponentV3<ReplayerUIPanel> {
        #region UI Components

        [UIComponent("layout-editor"), UsedImplicitly]
        private LayoutEditor _layoutEditor = null!;

        [UIComponent("container-group"), UsedImplicitly]
        private RectTransform _containerRect = null!;

        private BeatmapLevelPreviewEditorComponent _songInfo = null!;
        private PlayerListEditorComponent _playerList = null!;
        private ToolbarEditorComponent _toolbar = null!;
        private LayoutGrid _layoutGrid = null!;

        #endregion

        #region Setup

        private IReplayPauseController _pauseController = null!;

        public void Setup(
            IReplayPauseController pauseController,
            IReplayFinishController finishController,
            IReplayTimeController timeController,
            IVirtualPlayersManager playersManager,
            IViewableCameraController cameraController,
            IVirtualPlayerBodySpawner bodySpawner,
            ReplayLaunchData launchData,
            IReplayWatermark? watermark = null
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
                launchData
            );
            _playerList.Setup(playersManager, timeController);
            var settings = launchData.Settings.LayoutEditorSettings ??= new();
            _layoutEditor.Setup(settings);
            _layoutEditor.SetEditorActive(false, false);
        }

        protected override void OnInitialize() {
            _songInfo = BeatmapLevelPreviewEditorComponent.Instantiate(ContentTransform);
            _toolbar = ToolbarEditorComponent.Instantiate(ContentTransform);
            _playerList = PlayerListEditorComponent.Instantiate(ContentTransform);
            var editorWindow = LayoutEditorWindow.Instantiate(ContentTransform);

            _layoutGrid = _containerRect.gameObject.AddComponent<LayoutGrid>();
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
            _pauseController.LockUnpause = active;
            _layoutGrid.Refresh();
        }

        #endregion
    }
}