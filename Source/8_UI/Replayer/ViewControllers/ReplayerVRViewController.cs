using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using Zenject;
using BeatSaberMarkupLanguage.FloatingScreen;
using UnityEngine;
using VRUIControls;
using BeatSaberMarkupLanguage;
using IPA.Utilities;
using UnityEngine.UI;
using BeatLeader.Utils;
using BeatLeader.Replayer;
using BeatLeader.Models;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.ReplayerVRView.bsml")]
    internal class ReplayerVRViewController : StandaloneViewController<FloatingScreen> {
        [Inject] private readonly IReplayPauseController _pauseController = null!;
        [Inject] private readonly IReplayFinishController _finishController = null!;
        [Inject] private readonly IReplayTimeController _timeController = null!;
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly ReplayerCameraController _cameraController = null!;
        [Inject] private readonly ReplayWatermark _watermark = null!;

        [UIValue("toolbar")]
        private ToolbarWithSettings _toolbar = null!;

        [UIValue("floating-controls")] 
        private FloatingControls _floatingControls = null!;

        protected override void OnPreParse() {
            _floatingControls = ReeUIComponentV2.Instantiate<FloatingControls>(transform);
            _toolbar = ToolbarWithSettings.Instantiate(transform);
            _floatingControls.Setup(Screen, _cameraController.ViewableCamera?.Camera?.transform);
            _toolbar.Setup(_timeController, _pauseController, _finishController,
                _playersManager, _cameraController.ViewableCamera, _launchData, _watermark);
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            var container = new GameObject("Container");
            var containerTransform = Container.transform;

            container.transform.SetParent(containerTransform.parent, false);
            containerTransform.SetParent(container.transform, false);

            containerTransform.localScale = new(0.02f, 0.02f, 0.02f);
            Container = container;

            var screen = Screen;
            screen.gameObject.GetOrAddComponent<VRGraphicRaycaster>()
                .SetField("_physicsRaycaster", BeatSaberUI.PhysicsRaycasterWithCache);

            screen.ScreenSize = new(100, 55);
            screen.ShowHandle = true;
            screen.HandleSide = FloatingScreen.Side.Bottom;
            screen.HighlightHandle = true;

            var screenHandle = screen.handle;
            screenHandle.transform.localPosition = new(11, -29, 0);
            screenHandle.transform.localScale = new(20, 3.67f, 3.67f);

            var scaler = screen.gameObject.GetOrAddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 3.44f;
            scaler.referencePixelsPerUnit = 10;
        }
    }
}
