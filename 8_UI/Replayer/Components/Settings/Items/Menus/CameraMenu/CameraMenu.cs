using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [SerializeAutomatically]
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.CameraMenu.CameraMenu.bsml")]
    internal class CameraMenu : MenuWithContainer
    {
        [Inject] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly InputManager _inputManager;

        [SerializeAutomatically] private static string cameraView = "PlayerView";
        [SerializeAutomatically] private static int cameraFov = 90;

        private List<CameraParamsMenu> _posesParams;

        [UIValue("camera-view-values")] private List<object> _cameraViewValues;
        [UIValue("camera-view")] private string _cameraView
        {
            get => _cameraController.CurrentPoseName;
            set
            {
                cameraView = value;
                _cameraController.SetCameraPose(value);
                NotifyPropertyChanged(nameof(_cameraView));
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }
        [UIValue("camera-fov")] private int _cameraFov
        {
            get => _cameraController.FieldOfView;
            set
            {
                cameraFov = value;
                _cameraController.FieldOfView = value;
                NotifyPropertyChanged(nameof(_cameraFov));
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }

        [UIObject("camera-fov-container")] private GameObject _cameraFovContainer;
        [UIValue("pose-menu-button")] private SubMenuButton _poseMenuButton;
        private CanvasGroup _poseMenuButtonCanvasGroup;

        protected override void OnBeforeParse()
        {
            _posesParams = Assembly.GetExecutingAssembly().ScanAndActivateTypes(null, x => (CameraParamsMenu)Instantiate(x));
            _poseMenuButton = CreateButtonForMenu(this, null, "View params");
            _cameraViewValues = new List<object>(_cameraController.poseProviders.Select(x => x.Name));
            _cameraController.OnCameraPoseChanged += NotifyCameraPoseChanged;
        }
        protected override void OnAfterParse()
        {
            var obj = _cameraFovContainer.AddComponent<InputDependentObject>();
            obj.Init(_inputManager, InputManager.InputType.FPFC);
            if (obj.ShouldBeVisible) _cameraFov = cameraFov;
            _cameraView = cameraView;
            _poseMenuButtonCanvasGroup = _poseMenuButton.ButtonGameObject.AddComponent<CanvasGroup>();
        }

        private void NotifyCameraPoseChanged(CameraPoseProvider provider)
        {
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
