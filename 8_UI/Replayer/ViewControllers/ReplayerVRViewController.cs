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
        [Inject] private readonly IReplayPauseController _pauseController;
        [Inject] private readonly IReplayExitController _exitController;
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController;
        [Inject] private readonly ReplayLaunchData _launchData;
        [Inject] private readonly ReplayerUIBinder _uiBinder;

        [UIValue("toolbar")] private Toolbar _toolbar;
        [UIValue("floating-controls")] private FloatingControls _floatingControls;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            _floatingControls = ReeUIComponentV2.Instantiate<FloatingControls>(transform);
            _toolbar = ReeUIComponentV2.Instantiate<Toolbar>(transform);

            _floatingControls.Setup((FloatingScreen)_uiBinder.Screen, _pauseController,
                _camera.Camera.transform, !_launchData.ActualSettings.ShowUI);

            _toolbar.Setup(_launchData.Replay, _pauseController, _exitController, _beatmapTimeController);

            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }
    }
}
