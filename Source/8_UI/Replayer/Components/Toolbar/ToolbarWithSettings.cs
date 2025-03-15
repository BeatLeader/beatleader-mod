using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;

namespace BeatLeader.Components {
    internal class ToolbarWithSettings : EditableElement {
        [UIValue("settings-modal")] 
        private SettingsModal _settingsModal = null!;

        [UIValue("toolbar")] 
        private Toolbar _toolbar = null!;

        public override string Name { get; } = "Toolbar";

        public override LayoutMap LayoutMap { get; } = new() {
            layer = 1,
            position = new(0.375f, 0f)
        };

        private RootContentView _rootContentView = null!;
        private LayoutEditor? _layoutEditor;

        protected override void OnInstantiate() {   
            base.OnInstantiate();
            _settingsModal = Instantiate<SettingsModal>(transform);
            _toolbar = Instantiate<Toolbar>(transform);
            _rootContentView = InstantiateOnSceneRoot<RootContentView>();

            _rootContentView.ManualInit(null!);
            _settingsModal.Setup(_rootContentView);

            _toolbar.SettingsButtonClickedEvent += _settingsModal.ShowModal;
        }

        public void Setup(
            IReplayTimeController timeController,
            IReplayPauseController pauseController,
            IReplayFinishController finishController,
            IVirtualPlayersManager playersManager,
            IViewableCameraController? cameraController,
            ReplayLaunchData launchData,
            IReplayWatermark? watermark = null,
            LayoutEditor? layoutEditor = null) {
            if (_layoutEditor != null)
                _layoutEditor.EditModeStateWasChangedEvent -= HandleEditModeChanged;

            if ((_layoutEditor = layoutEditor) != null)
                _layoutEditor.EditModeStateWasChangedEvent += HandleEditModeChanged;

            _rootContentView.Setup(timeController,
                pauseController, playersManager, cameraController,
                launchData, watermark, _toolbar.Timeline, layoutEditor);
            _toolbar.Setup(pauseController, finishController,
                timeController, playersManager, launchData);
        }

        public void HandleClose() {
            _settingsModal.HideModalImmediate();
        }

        private void HandleEditModeChanged(bool state) {
            _settingsModal.HideModalImmediate();
        }
    }
}
