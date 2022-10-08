using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using Zenject;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.UI;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatLeader.Replayer.Camera;

namespace BeatLeader.ViewControllers
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.ReplayerVRView.bsml")]
    internal class ReplayerVRViewController : BSMLAutomaticViewController
    {
        [Inject] private readonly ReplayerCameraController _camera;
        [Inject] private readonly PlaybackController _playbackController;
        [Inject] private readonly BeatmapTimeController _beatmapTimeController;
        [Inject] private readonly ReplayLaunchData _launchData;
        [Inject] private readonly DiContainer _container;
        [Inject] private readonly ReplayerUIBinder _uiBinder;

        [UIValue("toolbar")] private Toolbar _toolbar;
        [UIValue("floating-controls")] private FloatingControls _floatingControls;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            _floatingControls = ReeUIComponentV2.Instantiate<FloatingControls>(transform);
            _toolbar = ReeUIComponentV2WithContainer.InstantiateInContainer<Toolbar>(_container, transform);

            _floatingControls.Setup((FloatingScreen)_uiBinder.Screen, _playbackController,
                _camera.Camera.transform, !_launchData.ActualSettings.ShowUI);

            _toolbar.Setup(_launchData.Replay, _playbackController,
                _playbackController, _beatmapTimeController);

            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }
    }
}
