using BeatLeader.Models;
using BeatLeader.Replayer.Camera;
using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;

namespace BeatLeader.Components {
    internal class ToolbarWithSettings : EditableElement {
        [UIValue("settings-modal")] private SettingsModal _settingsModal = null!;
        [UIValue("toolbar")] private Toolbar _toolbar = null!;

        private RootContentView _rootContentView = null!;
        private LayoutEditor? _layoutEditor;

        public override string Name { get; } = "Toolbar";
        public override LayoutMapData DefaultLayoutMap { get; protected set; } = new() {
            enabled = true,
            position = new(0.5f, 0)
        };

        protected override void OnInstantiate() {
            _settingsModal = Instantiate<SettingsModal>(transform);
            _toolbar = Instantiate<Toolbar>(transform);
            _rootContentView = InstantiateOnSceneRoot<RootContentView>();

            _rootContentView.ManualInit(null!);
            _settingsModal.Setup(_rootContentView);

            _toolbar.SettingsButtonClickedEvent += _settingsModal.ShowModal;
        }

        public void Setup(
            IBeatmapTimeController timeController,
            IReplayPauseController pauseController,
            IReplayFinishController finishController,
            IVirtualPlayersManager playersManager,
            ReplayLaunchData launchData,
            ReplayerCameraController cameraController,
            IReplayWatermark? watermark = null,
            LayoutEditor? layoutEditor = null) {
            if (_layoutEditor != null)
                _layoutEditor.EditModeChangedEvent -= HandleEditModeChanged;

            if ((_layoutEditor = layoutEditor) != null)
                _layoutEditor.EditModeChangedEvent += HandleEditModeChanged;

            _rootContentView.Setup(timeController,
                pauseController, playersManager, launchData,
                cameraController, watermark, _toolbar.Timeline, layoutEditor);
            _toolbar.Setup(pauseController, finishController,
                timeController, playersManager, launchData);
        }

        private void HandleEditModeChanged(bool state) {
            _settingsModal.HideModalImmediate();
        }
    }
}
