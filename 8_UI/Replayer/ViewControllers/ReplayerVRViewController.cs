using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using Zenject;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatLeader.Replayer.Camera;
using UnityEngine;
using VRUIControls;
using BeatSaberMarkupLanguage;
using IPA.Utilities;
using BeatLeader.Replayer.Emulation;
using UnityEngine.UI;
using BeatLeader.Utils;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.ReplayerVRView.bsml")]
    internal class ReplayerVRViewController : StandaloneViewController<FloatingScreen> {
        [Inject] private readonly IReplayPauseController _pauseController;
        [Inject] private readonly IReplayExitController _exitController;
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController;
        [Inject] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly VRControllersAccessor _controllersAccessor;
        [Inject] private readonly ReplayWatermark _watermark;
        [Inject] private readonly ReplayLaunchData _launchData;
        [Inject] private readonly SongSpeedData _speedData;

        [UIValue("toolbar")] private ToolbarWithSettings _toolbar;
        [UIValue("floating-controls")] private FloatingControls _floatingControls;

        protected override void OnInstantiate() {
            _floatingControls = ReeUIComponentV2.Instantiate<FloatingControls>(transform);
            _toolbar = ReeUIComponentV2.Instantiate<ToolbarWithSettings>(transform);
            _floatingControls.Setup(Screen, _pauseController,
                _cameraController.Camera.transform, !_launchData.ActualSettings.ShowUI);
            _toolbar.Setup(_beatmapTimeController, _pauseController,
                _exitController, _launchData, _speedData, _cameraController,
                _controllersAccessor, _watermark);
        }

        protected override void InitInternal() {
            base.InitInternal();

            var container = new GameObject("Container");
            var viewContainer = new GameObject("ViewContainer");

            container.transform.SetParent(Container.transform.parent, false);
            viewContainer.transform.SetParent(container.transform, false);

            Container.transform.SetParent(viewContainer.transform, false);
            Container.transform.localScale = new(0.02f, 0.02f, 0.02f);
            Container = container;

            var screen = Screen;
            screen.gameObject.GetOrAddComponent<VRGraphicRaycaster>()
                .SetField("_physicsRaycaster", BeatSaberUI.PhysicsRaycasterWithCache);

            screen.ScreenSize = new(100, 55);
            screen.ShowHandle = true;
            screen.HandleSide = FloatingScreen.Side.Bottom;
            screen.HighlightHandle = true;

            screen.handle.transform.localPosition = new(11, -23.5f, 0);
            screen.handle.transform.localScale = new(20, 3.67f, 3.67f);

            var scaler = screen.gameObject.GetOrAddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 3.44f;
            scaler.referencePixelsPerUnit = 10;
        }
    }
}
