using BeatLeader.Models;
using BeatLeader.Replayer.Camera;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatLeader.Components {
    internal class CameraContentView : ContentView {
        #region UI Components

        [UIValue("params-menu-button")] private NavigationButton _paramsMenuButton;
        [UIObject("camera-fov-container")] private GameObject _cameraFovContainer;

        #endregion

        #region UI Values

        [UIValue("camera-view-values")]
        private List<object> _cameraViewValues = new() { string.Empty };

        [UIValue("camera-view")]
        private string _CameraView {
            get => _cameraController?.CurrentPoseName ?? string.Empty;
            set {
                if (!_isInitialized) return;
                _cameraController.SetCameraPose(value);
            }
        }

        [UIValue("camera-fov")]
        private int _CameraFov {
            get => _cameraController?.FieldOfView ?? 0;
            set {
                if (!_isInitialized) return;
                _replayData.ActualToWriteSettings.CameraFOV = value;
                _cameraController.FieldOfView = value;
            }
        }

        #endregion

        #region Setup

        private ReplayerCameraController _cameraController;
        private ReplayLaunchData _replayData;
        private bool _isInitialized;

        public void Setup(
            ReplayerCameraController cameraController,
            ReplayLaunchData launchData) {
            if (_cameraController != null)
                _cameraController.CameraPoseChangedEvent -= HandlePoseChanged;

            _cameraController = cameraController;
            _replayData = launchData;
            _isInitialized = true;

            _cameraController.CameraPoseChangedEvent += HandlePoseChanged;

            _cameraViewValues.Clear();
            _cameraViewValues.AddRange(_cameraController.PoseProviders.Select(x => x.Name));

            NotifyPropertyChanged(nameof(_CameraView));
            NotifyPropertyChanged(nameof(_CameraFov));
            InitPoses();
            HandlePoseChanged(_cameraController.CurrentPose);
        }

        protected override void OnInitialize() {
            _cameraFovContainer.AddComponent<InputDependentObject>().Init(InputUtils.InputType.FPFC);
        }
        protected override void OnInstantiate() {
            _paramsMenuButton = Instantiate<NavigationButton>(transform);
            _paramsMenuButton.OnClick += HandleParamsButtonClicked;
            _paramsMenuButton.Text = "View params";
        }

        #endregion

        #region UI Callbacks

        private void HandleParamsButtonClicked() {
            if (_selectedParamsMenu != null)
                PresentView(_selectedParamsMenu);
        }

        #endregion

        #region Params

        private readonly List<ParamsContentViewBase> _contentViews = new() {
            InstantiateOnSceneRoot<PlayerViewParamsContentView>(),
            InstantiateOnSceneRoot<FlyingViewParamsContentView>()
        };

        private ParamsContentViewBase _selectedParamsMenu;

        private void HandlePoseChanged(ICameraPoseProvider provider) {
            if (provider == null) return;
            NotifyPropertyChanged(nameof(_CameraView));
            _paramsMenuButton.Interactable = TryFindView(provider, out _selectedParamsMenu);
        }

        private bool TryFindView(ICameraPoseProvider provider, out ParamsContentViewBase view) {
            return (view = _contentViews.FirstOrDefault(
                x => x.PoseType == provider.GetType() && x.Id == provider.Id)) != null;
        }

        private bool TryFindPose(ParamsContentViewBase view, out ICameraPoseProvider provider) {
            return (provider = _cameraController.PoseProviders
                .FirstOrDefault(x => x.GetType() == view.PoseType && x.Id == view.Id)) != null;
        }

        private void InitPoses() {
            foreach (var item in _contentViews) {
                if (!TryFindPose(item, out var provider)) continue;
                item.Setup(provider);
                item.ManualInit(null);
            }
        }

        #endregion
    }
}
