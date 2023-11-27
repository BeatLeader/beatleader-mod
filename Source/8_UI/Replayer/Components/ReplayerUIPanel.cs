using System.Collections;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ReplayerUIPanel : ReeUIComponentV3<ReplayerUIPanel> {
        #region UI Components

        [UIComponent("layout-editor"), UsedImplicitly]
        private LayoutEditor _layoutEditor = null!;

        [UIComponent("container-group"), UsedImplicitly]
        private RectTransform _containerRect = null!;

        private HorizontalBeatmapLevelPreview _songInfo = null!;
        private PlayerListEditorComponent _playerList = null!;
        private ToolbarWithSettings _toolbar = null!;
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
            ReplayLaunchData launchData,
            IReplayWatermark? watermark = null
        ) {
            _pauseController = pauseController;
            _songInfo.SetBeatmapLevel(launchData.DifficultyBeatmap.level);
            _toolbar.Setup(
                timeController, pauseController,
                finishController, playersManager, cameraController,
                launchData, watermark, _layoutEditor
            );
            _playerList.Setup(playersManager, timeController);
            //if (!launchData.IsBattleRoyale) {
            //    var player = launchData.MainReplay.ReplayData.Player;
            //    if (player is not null) 
            //}
            var settings = launchData.Settings.LayoutEditorSettings ??= new();
            _layoutEditor.Setup(settings);
            _layoutEditor.SetEditorActive(false, false);
        }

        protected override void OnInitialize() {
            _songInfo = HorizontalBeatmapLevelPreview.Instantiate(ContentTransform!);
            _toolbar = ToolbarWithSettings.Instantiate(ContentTransform!);
            _playerList = PlayerListEditorComponent.Instantiate(ContentTransform!);
            var editorWindow = LayoutEditorWindow.Instantiate(ContentTransform!);

            _layoutGrid = _containerRect.gameObject.AddComponent<LayoutGrid>();
            _layoutEditor.AdditionalComponentHandler = _layoutGrid;
            _layoutEditor.StateChangedEvent += HandleLayoutEditorStateChanged;
            _layoutEditor.AddComponent(editorWindow);
            _layoutEditor.AddComponent(_toolbar);
            _layoutEditor.AddComponent(_songInfo);
            _layoutEditor.AddComponent(_playerList);

            QueueGridRefresh();
        }

        #endregion

        #region LayoutGrid

        private void QueueGridRefresh() {
            RoutineFactory.StartUnmanagedCoroutine(RefreshGridCoroutine());
        }

        private IEnumerator RefreshGridCoroutine() {
            yield return new WaitForEndOfFrame();
            _layoutGrid.Refresh();
        }

        #endregion

        #region Callbacks

        private void HandleLayoutEditorStateChanged(bool active) {
            _layoutGrid.enabled = active;
            _pauseController.LockUnpause = active;
        }

        #endregion
    }
}