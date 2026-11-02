using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.Utils;
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

        private ICameraViewParams? FindCameraParams(ICameraView view) {
            return _cameraViewParams.FirstOrDefault(x => x.SupportsCameraView(view));
        }

        private void ShowCameraParams(ICameraView view) {
            _selectedViewParams?.Setup(null);
            _selectedViewParams = FindCameraParams(view);
            _viewContainer.Key = _selectedViewParams == null ? null : view;
            _selectedViewParams?.Setup(view);
        }

        #endregion

        #region Camera Views

        private BsListControl<ICameraView> _viewSelector = null!;

        private void LoadCameraViews(ICameraController cameraController) {
            var views = cameraController.Views;

            var viewParams = new Dictionary<ICameraView, IReactiveComponent>();
            foreach (var view in views) {
                if (FindCameraParams(view) is { } param) {
                    viewParams[view] = param;
                }
            }
            _viewContainer.Views = viewParams;

            _viewSelectorRail.Component = new BsListControl<ICameraView> {
                FlexItem = {
                    Size = new() { x = 40f, y = 6f }
                },

                Items = views.Select(static x => new BsListControlItem<ICameraView>(x, x.Name, null)).ToArray(),
                Key = cameraController.SelectedView,
                Skew = 0f,
                OnKeyChanged = HandleViewSelectorKeyChanged
            }.Bind(ref _viewSelector);

            ShowCameraParams(cameraController.SelectedView);
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

            if (cameraController == null || cameraSettings == null) {
                SetUnavailableTextActive(true);
                return;
            }

            _syncingControls = true;
            _fovSlider.MinValue = cameraSettings.MinCameraFOV;
            _fovSlider.MaxValue = cameraSettings.MaxCameraFOV;
            _fovSlider.Value = cameraSettings.CameraFOV;
            _syncingControls = false;
            _fovSlider.Interactable = InputUtils.UsesFPFC;

            LoadCameraViews(cameraController);
            cameraController.CameraFovChangedEvent += HandleCameraFovChanged;
            cameraController.CameraViewChangedEvent += HandleCameraViewChanged;
            SetUnavailableTextActive(false);
        }

        #endregion

        #region Construct

        private KeyedContainer<ICameraView> _viewContainer = null!;
        private NamedRail _viewSelectorRail = null!;
        private Label _unavailableLabel = null!;
        private BsSlider _fovSlider = null!;
        private Layout _container = null!;

        private void SetUnavailableTextActive(bool active) {
            foreach (var layoutItem in _container.Children) {
                var comp = (IReactiveComponent)layoutItem;
                comp.Enabled = comp == _unavailableLabel ? active : !active;
            }
        }

        protected override GameObject Construct() {
            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    Gap = 2f
                },

                Children = {
                    new Label {
                        FlexItem = {
                            AlignSelf = Align.Center
                        },

                        Text = "Camera controls are not available while using an external camera",
                        Enabled = false
                    }.Bind(ref _unavailableLabel),
                    //
                    new BsSlider {
                            FlexItem = {
                                Size = new() { x = 40f, y = 6f }
                            },

                            ValueStep = 5f,
                            OnValueChanged = HandleFOVSliderValueChanged
                        }
                        .Bind(ref _fovSlider)
                        .InNamedRail("Camera FOV"),
                    //
                    new NamedRail {
                        Label = { Text = "Camera View" }
                    }.Bind(ref _viewSelectorRail),
                    //
                    new KeyedContainer<ICameraView> {
                            FlexItem = {
                                FlexGrow = 1f
                            },

                            DummyView = new Label {
                                Text = "Monke has nothing to show here",
                                RaycastTarget = false
                            },
                            Views = new Dictionary<ICameraView, IReactiveComponent>(),
                            Key = null
                        }
                        .Bind(ref _viewContainer)
                        .InBackground(color: (Color.white * 0.1f).ColorWithAlpha(1f))
                        .AsFlexGroup(padding: 2f)
                        .AsFlexItem(flexGrow: 1f)
                }
            }.Bind(ref _container).Use();
        }

        #endregion

        #region Callbacks

        private bool _syncingControls;

        private void HandleCameraFovChanged(int fov) {
            _syncingControls = true;
            _fovSlider.Value = fov;
            _syncingControls = false;
        }

        private void HandleCameraViewChanged(ICameraView view) {
            _syncingControls = true;
            _viewSelector.Key = view;
            _syncingControls = false;
            ShowCameraParams(view);
        }

        private void HandleFOVSliderValueChanged(float value) {
            if (_syncingControls) return;
            _cameraController?.SetFov((int)value);
        }

        private void HandleViewSelectorKeyChanged(ICameraView view) {
            if (_syncingControls) return;
            _cameraController?.SetView(view);
        }

        #endregion
    }
}