using BeatLeader.Models;
using BeatLeader.UI.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ToolbarWithSettings : LayoutEditorComponent {
        #region UI Components

        [UIValue("settings-modal")]
        private SettingsModal _settingsModal = null!;

        [UIValue("toolbar")]
        private Toolbar _toolbar = null!;

        private RootContentView _rootContentView = null!;
        private ILayoutEditor? _layoutEditor;

        #endregion

        #region LayoutComponent

        public override string ComponentName => "Toolbar";
        protected override Vector2 MinSize { get; } = new(96, 60);
        protected override Vector2 MaxSize { get; } = new(96, 60);

        #endregion

        #region Setup

        protected override void ConstructInternal(Transform parent) {
            throw new System.NotImplementedException();
        }
        
        protected override void OnInstantiate() {
            base.OnInstantiate();
            _settingsModal = ReeUIComponentV2.Instantiate<SettingsModal>(ContentTransform);
            //_toolbar = ReeUIComponentV2.Instantiate<Toolbar>(transform);
            _rootContentView = ReeUIComponentV2.InstantiateOnSceneRoot<RootContentView>();

            _rootContentView.ManualInit(null!);
            _settingsModal.Setup(_rootContentView);

            //_toolbar.SettingsButtonClickedEvent += _settingsModal.ShowModal;
        }

        public void Setup(
            IReplayTimeController timeController,
            IReplayPauseController pauseController,
            IReplayFinishController finishController,
            IVirtualPlayersManager playersManager,
            ICameraController? cameraController,
            ReplayLaunchData launchData,
            IReplayWatermark? watermark = null,
            ILayoutEditor? layoutEditor = null
        ) {
            if (_layoutEditor is not null) {
                _layoutEditor.StateChangedEvent -= HandleLayoutEditorStateChanged;
            }
            _layoutEditor = layoutEditor;
            if (_layoutEditor is not null) {
                _layoutEditor.StateChangedEvent += HandleLayoutEditorStateChanged;
            }

            _rootContentView.Setup(
                timeController,
                pauseController, playersManager, cameraController,
                launchData, watermark, _toolbar.Timeline, layoutEditor
            );
            //_toolbar.Setup(
            //    pauseController, finishController,
            //    timeController, playersManager, launchData
            //);
        }

        private void HandleLayoutEditorStateChanged(bool state) {
            _settingsModal.HideModalImmediate();
        }

        #endregion
    }
}