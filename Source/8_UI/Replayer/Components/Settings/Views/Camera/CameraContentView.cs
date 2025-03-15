using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatLeader.Components {
    internal class CameraContentView : ContentView {
        #region UI Components

        [UIValue("params-menu-button")]
        private NavigationButton _paramsMenuButton = null!;

        [UIObject("camera-fov-container")]
        private readonly GameObject _cameraFovContainer = null!;

        #endregion

        #region UI Values

        [UIValue("camera-view-values")]
        private readonly List<object> _cameraViewValues = new() { "Empty" };

        [UIValue("camera-view")]
        private string CameraView {
            get => _cameraController?.SelectedView?.Name ?? string.Empty;
            set {
                if (!_isInitialized) return;
                _cameraController!.SetView(value);
                _launchData.Settings.CameraSettings!.CameraView = value;
            }
        }

        [UIValue("camera-fov")]
        private int CameraFov {
            get => (int)(_cameraController?.Camera?.fieldOfView ?? 0);
            set {
                if (!_isInitialized) return;
                _cameraController!.Camera!.fieldOfView = value;
                _launchData.Settings.CameraSettings!.CameraFOV = value;
            }
        }

        #endregion

        #region Setup

        private IViewableCameraController? _cameraController;
        private ReplayLaunchData _launchData = null!;
        private bool _isInitialized;

        public void Setup(IViewableCameraController cameraController, ReplayLaunchData launchData) {
            if (_cameraController != null)
                _cameraController.CameraViewChangedEvent -= HandleViewChanged;

            _cameraController = cameraController;
            _launchData = launchData;

            _cameraController.CameraViewChangedEvent += HandleViewChanged;
            if (_cameraController.Views.Count > 0) {
                _cameraViewValues.Clear();
                _cameraViewValues.AddRange(_cameraController.Views.Select(x => x.Name));
            }
            _isInitialized = true;

            NotifyPropertyChanged(nameof(CameraView));
            NotifyPropertyChanged(nameof(CameraFov));
            InitPoses();
            HandleViewChanged(_cameraController.SelectedView!);
        }

        private List<ParamsContentViewBase> _contentViews;

        private void Awake() {
            _contentViews = new() {
                InstantiateOnSceneRoot<PlayerViewParamsContentView>(),
                InstantiateOnSceneRoot<FlyingViewParamsContentView>()
            };
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

        

        private ParamsContentViewBase? _selectedParamsMenu;

        private void HandleViewChanged(ICameraView? view) {
            NotifyPropertyChanged(nameof(CameraView));
            _paramsMenuButton.Interactable = view != null
                && TryFindView(view, out _selectedParamsMenu);
        }

        private bool TryFindView(ICameraView provider, out ParamsContentViewBase? view) {
            return (view = _contentViews.FirstOrDefault(
                x => x.ViewType == provider.GetType())) != null;
        }

        private bool TryFindPose(ParamsContentViewBase view, out ICameraView provider) {
            return (provider = _cameraController.Views
                .FirstOrDefault(x => x.GetType() == view.ViewType)) != null;
        }

        private void InitPoses() {
            foreach (var item in _contentViews) {
                if (!TryFindPose(item, out var provider)) continue;
                item.Setup(provider);
                item.ManualInit(null!);
            }
        }

        #endregion
    }
}