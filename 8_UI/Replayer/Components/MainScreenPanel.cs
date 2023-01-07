using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections;
using UnityEngine;

namespace BeatLeader.Components {
    internal class MainScreenPanel : ReeUIComponentV2 {
        #region Events

        public event Action? LayoutBuiltEvent;

        #endregion

        #region UI Components

        [UIValue("layout-editor")]
        public LayoutEditor LayoutEditor { get; private set; } = null!;

        [UIValue("song-info")]
        private HorizontalBeatmapLevelPreview _songInfo = null!;

        [UIValue("player-info")]
        private HorizontalMiniProfile _playerInfo = null!;

        [UIValue("toolbar")]
        private ToolbarWithSettings _toolbar = null!;

        [UIComponent("container-group")]
        private readonly RectTransform _containerRect = null!;

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
            IReplayWatermark? watermark = null) {
            OnDispose();
            _pauseController = pauseController;
            _songInfo.SetBeatmapLevel(launchData.DifficultyBeatmap!.level);
            _toolbar.Setup(timeController, pauseController,
                finishController, playersManager, cameraController,
                launchData, watermark, LayoutEditor);
            LayoutEditor.antiSnapKeyCode = launchData.Settings.Shortcuts.LayoutEditorAntiSnapHotkey;

            if (!launchData.IsBattleRoyale) {
                var player = launchData.Replays[0].Key;
                if (player != null) _playerInfo.SetPlayer(player);
            }
        }

        protected override void OnInstantiate() {
            _playerInfo = Instantiate<HorizontalMiniProfile>(transform);
            _songInfo = Instantiate<HorizontalBeatmapLevelPreview>(transform);
            _toolbar = Instantiate<ToolbarWithSettings>(transform);
            LayoutEditor = Instantiate<LayoutEditor>(transform);
            LayoutEditor.EditModeStateWasChangedEvent += HandleLayoutEditorStateChanged;
        }

        protected override void OnInitialize() {
            var configInstance = LayoutEditorConfig.Instance;
            LayoutEditor.layoutGridModel = configInstance.LayoutGridModel;
            LayoutEditor.layoutMapsSource = configInstance;

            LayoutEditor.Setup(_containerRect);
            LayoutEditor.Add(_playerInfo, _toolbar, _songInfo);
            CoroutinesHandler.instance.StartCoroutine(MapLayoutCoroutine());
        }

        #endregion

        #region Layout

        public IEnumerator MapLayoutCoroutine() {
            yield return new WaitForEndOfFrame();
            LayoutEditor.ForceMapLayout();
            LayoutBuiltEvent?.Invoke();
        }

        #endregion

        #region Callbacks

        private void HandleLayoutEditorStateChanged(bool enabled) {
            _pauseController.LockUnpause = enabled;
        }

        #endregion
    }
}
