using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class SettingsCameraView : ReactiveComponent {
        #region Camera Params

        public interface ICameraViewParams : IReactiveComponent {
            bool SupportsCameraView(ICameraView cameraView);
            void Setup(ICameraView? cameraView);
        }

        public ICollection<ICameraViewParams> CameraViewParams => _cameraViewParams;

        private readonly List<ICameraViewParams> _cameraViewParams = new();
        private ICameraViewParams? _selectedViewParams;

        private void TryShowCameraParams() {
            if (_selectedCameraView == null) {
                _viewContainer.Select(null);
                return;
            }
            _selectedViewParams?.Setup(null);
            if (!_viewContainer.Select(_selectedCameraView)) {
                var viewParams = _cameraViewParams.FirstOrDefault(x => x.SupportsCameraView(_selectedCameraView));
                if (viewParams == null) return;
                _viewContainer.Items.Add(_selectedCameraView, viewParams);
                _viewContainer.Select(_selectedCameraView);
                _selectedViewParams = viewParams;
            } else {
                _selectedViewParams = (ICameraViewParams)_viewContainer.Items[_selectedCameraView];
            }
            _selectedViewParams.Setup(_selectedCameraView);
        }

        #endregion

        #region Camera Views

        private IEnumerable<ICameraView> CameraViews => _cameraController!.Views;

        private ICameraView? _selectedCameraView;
        private bool _isInitialized;

        private void RefreshCameraViews() {
            _viewSelector.Items.Clear();
            foreach (var view in CameraViews) {
                _viewSelector.Items.Add(view, view.Name);
            }
            _selectedCameraView = _cameraController!.SelectedView;
            if (_selectedCameraView == null) return;
            _viewSelector.Select(_selectedCameraView);
            TryShowCameraParams();
        }

        #endregion

        #region Setup

        private ICameraController? _cameraController;

        public void Setup(
            ICameraController? cameraController,
            ReplayerCameraSettings? cameraSettings
        ) {
            if (_cameraController != null) {
                _cameraController.CameraFovChangedEvent -= HandleCameraFovChanged;
                _cameraController.CameraViewChangedEvent -= HandleCameraViewChanged;
            }

            _cameraController = cameraController;
            _isInitialized = false;

            if (_cameraController != null && cameraSettings != null) {
                _fovSlider.ValueRange = new(cameraSettings.MinCameraFOV, cameraSettings.MaxCameraFOV);
                _fovSlider.Value = cameraSettings.CameraFOV;
                _fovSlider.Interactable = EnvironmentUtils.UsesFPFC;

                _cameraController.CameraFovChangedEvent += HandleCameraFovChanged;
                _cameraController.CameraViewChangedEvent += HandleCameraViewChanged;
                RefreshCameraViews();
                SetUnavailableTextActive(false);

                _isInitialized = true;
            } else {
                SetUnavailableTextActive(true);
            }
        }

        #endregion

        #region Construct

        private TextListControl<ICameraView> _viewSelector = null!;
        private KeyedContainer<ICameraView> _viewContainer = null!;
        private Label _unavailableLabel = null!;
        private Slider _fovSlider = null!;
        private Layout _container = null!;

        private void SetUnavailableTextActive(bool active) {
            foreach (var layoutItem in _container.Children) {
                var comp = (IReactiveComponent)layoutItem;
                comp.Enabled = comp == _unavailableLabel ? active : !active;
            }
        }

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new Label {
                        Text = "Camera controls are not available while using an external camera",
                        Enabled = false
                    }.Bind(ref _unavailableLabel).AsFlexItem(alignSelf: Align.Center),
                    //
                    new Slider {
                            ValueStep = 5f
                        }
                        .WithListener(x => x.Value, HandleFOVSliderValueChanged)
                        .AsFlexItem(size: new() { x = 40f, y = 6f })
                        .Bind(ref _fovSlider)
                        .InNamedRail("Camera FOV"),
                    //
                    new TextListControl<ICameraView>()
                        .WithListener(x => x.SelectedKey, HandleViewSelectorViewChanged)
                        .AsFlexItem(size: new() { x = 40f, y = 6f })
                        .Bind(ref _viewSelector)
                        .InNamedRail("Camera View"),
                    //
                    new KeyedContainer<ICameraView> {
                            DummyView = new Label {
                                Text = "Monke has nothing to show here"
                            }
                        }
                        .AsFlexItem(grow: 1f)
                        .Bind(ref _viewContainer)
                        .InBackground(color: (Color.white * 0.1f).ColorWithAlpha(1f))
                        .AsFlexGroup(padding: 2f)
                        .AsFlexItem(grow: 1f)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                gap: 2f
            ).Bind(ref _container).Use();
        }

        #endregion

        #region Callbacks

        private void HandleCameraFovChanged(int fov) {
            _fovSlider.SetValueSilent(fov);
        }

        private void HandleCameraViewChanged(ICameraView view) {
            _viewSelector.SelectSilent(view);
            HandleViewSelectorViewChangedInternal(view);
        }

        private void HandleFOVSliderValueChanged(float value) {
            if (!_isInitialized) return;
            _cameraController?.SetFov((int)value);
        }

        private void HandleViewSelectorViewChanged(ICameraView view) {
            if (_isInitialized) {
                _cameraController?.SetView(view);
            }
            HandleViewSelectorViewChangedInternal(view);
        }

        private void HandleViewSelectorViewChangedInternal(ICameraView view) {
            _selectedCameraView = view;
            TryShowCameraParams();
        }

        #endregion
    }
}