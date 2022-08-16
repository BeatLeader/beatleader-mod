using BeatLeader.Replayer;
using BeatLeader.Replayer.Managers;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [SerializeAutomatically]
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.CameraSetting.bsml")]
    internal class CameraSetting : Setting
    {
        [Inject] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly InputManager _inputManager;

        [SerializeAutomatically] private static string cameraView = "PlayerView";
        [SerializeAutomatically] private static int cameraFov = 110;

        [UIObject("camera-fov-container")] private GameObject _cameraFovContainer;
        [UIValue("camera-view-values")] private List<object> _cameraViewValues;
        [UIValue("camera-view")] private string _cameraView
        {
            get => _cameraController.CurrentPose;
            set
            {
                cameraView = value;
                _cameraController.SetCameraPose(value);
                NotifyPropertyChanged(nameof(_cameraView));
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
            }
        }

        public override bool IsSubMenu => true;
        public override int SettingIndex => 1;

        protected override void OnBeforeHandling()
        {
            _cameraViewValues = new List<object>(_cameraController.poseProviders.Select(x => x.Name));
        }
        protected override void OnAfterHandling()
        {
            var obj = _cameraFovContainer.AddComponent<InputDependentObject>();
            obj.Init(_inputManager, InputManager.InputType.FPFC);
            if (obj.ShouldBeVisible)
                _cameraFov = cameraFov;
            _cameraView = cameraView;
        }
    }
}
