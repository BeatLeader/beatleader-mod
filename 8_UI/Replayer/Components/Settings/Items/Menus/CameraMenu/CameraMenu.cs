using BeatLeader.Models;
using BeatLeader.Replayer.Camera;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.CameraMenu.CameraMenu.bsml")]
    internal class CameraMenu : MenuWithContainer
    {
        [Inject] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly ReplayLaunchData _replayData;

        [UIValue("camera-view-values")] private List<object> _cameraViewValues;
        [UIValue("camera-view")] private string cameraView
        {
            get => _cameraController.CurrentPoseName;
            set
            {
                _cameraController.SetCameraPose(value);
            }
        }
        [UIValue("camera-fov")] private int cameraFov
        {
            get => _cameraController.FieldOfView;
            set
            {
                _replayData.ActualToWriteSettings.CameraFOV = value;
                _cameraController.FieldOfView = value;
            }
        }

        [UIObject("camera-fov-container")] private GameObject _cameraFovContainer;
        [UIValue("pose-menu-button")] private SubMenuButton _poseMenuButton;

        private static readonly IReadOnlyList<Type> _cameraMenus = new[]
        {
            typeof(FlyingViewParamsMenu),
            typeof(PlayerViewParamsMenu)
        };
        private List<CameraParamsMenu> _posesParams;
        private CanvasGroup _poseMenuButtonCanvasGroup;

        protected override void OnBeforeParse()
        {
            _posesParams = _cameraMenus.Select(x => (CameraParamsMenu)Instantiate(x)).ToList();
            _poseMenuButton = CreateButtonForMenu(this, null, "View params");
            _cameraViewValues = new List<object>(_cameraController.PoseProviders.Select(x => x.Name));
            _cameraController.CameraPoseChangedEvent += HandleCameraPoseChanged;
            _cameraController.CameraFOVChangedEvent += HandleCameraFOVChanged;
        }
        protected override void OnAfterParse()
        {
            _cameraFovContainer.AddComponent<InputDependentObject>().Init(InputUtils.InputType.FPFC);
            _poseMenuButtonCanvasGroup = _poseMenuButton.ButtonGameObject.AddComponent<CanvasGroup>();
            HandleCameraPoseChanged(_cameraController.CurrentPose);
        }

        private void HandleCameraFOVChanged(int fov)
        {
            NotifyPropertyChanged(nameof(cameraFov));
        }
        private void HandleCameraPoseChanged(ICameraPoseProvider provider)
        {
            NotifyPropertyChanged(nameof(cameraView));
            var menu = _posesParams.FirstOrDefault(x => x.Id == provider.Id && x.Type == provider.GetType());
            if (menu != null)
            {
                _poseMenuButton.Init(menu, null);
                menu.Init(provider);
            }

            if (_poseMenuButtonCanvasGroup != null)
                _poseMenuButtonCanvasGroup.alpha = menu != null ? 1 : 0.7f;
            _poseMenuButton.Interactable = menu != null;
        }
    }
}
