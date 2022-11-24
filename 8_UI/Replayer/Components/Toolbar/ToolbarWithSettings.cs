using BeatLeader.Models;
using BeatLeader.Replayer.Camera;
using BeatLeader.Replayer.Emulation;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ToolbarWithSettings : EditableElement {
        [UIValue("settings-modal")] private SettingsModal _settingsModal;
        [UIValue("toolbar")] private Toolbar _toolbar;

        private RootContentView _rootContentView;
        private LayoutEditor _layoutEditor;

        public override string Name => "Toolbar";
        public override LayoutMapData DefaultLayoutMap { get; protected set; } = new() {
            enabled = true,
            position = new(0.5f, 0)
        };

        protected override void OnInstantiate() {
            _settingsModal = Instantiate<SettingsModal>(transform);
            _toolbar = Instantiate<Toolbar>(transform);
            _rootContentView = InstantiateOnSceneRoot<RootContentView>();

            _rootContentView.ManualInit(null);
            _settingsModal.Setup(_rootContentView);

            _toolbar.SettingsButtonClickedEvent += _settingsModal.ShowModal;
        }

        public void Setup(
            IBeatmapTimeController timeController,
            IReplayPauseController pauseController,
            IReplayExitController exitController,
            ReplayLaunchData launchData,
            SongSpeedData speedData,
            ReplayerCameraController cameraController,
            VRControllersAccessor controllersAccessor,
            ReplayWatermark watermark,
            LayoutEditor layoutEditor) {
            if (_layoutEditor != null) {
                _layoutEditor.EditModeChangedEvent -= HandleEditModeChanged;
            }
            _layoutEditor = layoutEditor;
            _layoutEditor.EditModeChangedEvent += HandleEditModeChanged;
            _rootContentView.Setup(timeController, speedData,
                cameraController, controllersAccessor, watermark, layoutEditor, launchData);
            _toolbar.Setup(launchData.Replay, pauseController, exitController, timeController);
        }

        private void HandleEditModeChanged(bool state) {
            _settingsModal.HideModalImmediate();
        }
    }
}
