using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Replayer.Camera;
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

        [UIValue("camera-view-values")] private List<object> _cameraViewValues;
        [UIValue("camera-view")] private string cameraView
        {
            get => _cameraController.CurrentPoseName;
            set
            {
                _cameraView = value;
                _cameraController.SetCameraPose(value);
                NotifyPropertyChanged(nameof(cameraView));
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }
        [UIValue("camera-fov")] private int cameraFov
        {
            get => _cameraController.FieldOfView;
            set
            {
                fpfcCameraFov = value;
                _cameraController.FieldOfView = value;
                NotifyPropertyChanged(nameof(cameraFov));
                AutomaticConfigTool.NotifyTypeChanged(GetType());
            }
        }
        private string _cameraView
        {
            get => InputManager.IsInFPFC ? fpfcCameraView : vrCameraView;
            set
            {
                ref var view = ref InputManager.IsInFPFC ? ref fpfcCameraView : ref vrCameraView;
                view = value;
            }
        }

        [SerializeAutomatically] private static string fpfcCameraView = "PlayerView";
        [SerializeAutomatically] private static string vrCameraView = "BehindView";
        [SerializeAutomatically] private static int fpfcCameraFov = 90;

        [UIObject("camera-fov-container")] private GameObject _cameraFovContainer;
        [UIValue("pose-menu-button")] private SubMenuButton _poseMenuButton;

        private List<CameraParamsMenu> _posesParams;
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
            obj.Init(InputManager.InputType.FPFC);
            if (obj.ShouldBeVisible) cameraFov = fpfcCameraFov;
            cameraView = InputManager.IsInFPFC ? fpfcCameraView : vrCameraView;
            _poseMenuButtonCanvasGroup = _poseMenuButton.ButtonGameObject.AddComponent<CanvasGroup>();
        }

        private void NotifyCameraPoseChanged(ICameraPoseProvider provider)
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
