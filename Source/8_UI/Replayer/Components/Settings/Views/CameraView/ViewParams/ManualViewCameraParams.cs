using BeatLeader.Models;
using BeatLeader.Replayer;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class ManualViewCameraParams : ReactiveComponent, SettingsCameraView.ICameraViewParams {
        #region ViewParams

        public bool SupportsCameraView(ICameraView cameraView) {
            return cameraView is ManualCameraView;
        }

        public void Setup(ICameraView? cameraView) {
            _cameraView = cameraView as ManualCameraView;
            LoadFromView();
        }

        #endregion

        #region View

        private ManualCameraView? _cameraView;
        private bool _loadingFromView;

        private ManualCameraView? EditableView => _loadingFromView ? null : _cameraView;

        private void LoadFromView() {
            if (_cameraView == null) return;
            Vector3 position = _cameraView.Position;
            _loadingFromView = true;
            _xSlider.Value = position.x;
            _ySlider.Value = position.y;
            _zSlider.Value = position.z;
            _rotationSlider.Value = _cameraView.Rotation;
            _loadingFromView = false;
        }

        #endregion

        #region Construct

        private BsSlider _xSlider = null!;
        private BsSlider _ySlider = null!;
        private BsSlider _zSlider = null!;
        private BsSlider _rotationSlider = null!;

        protected override GameObject Construct() {
            NamedRail CreateAxisSlider(string name, int axis, ref BsSlider slider) {
                return new BsSlider {
                        MinValue = -10f,
                        MaxValue = 10f,
                        ValueStep = 0.1f,
                        Value = 0f,
                        ValueFormatter = x => $"{Mathf.Round(x * 10f) / 10f}m",
                        OnValueChanged = x => HandleAxisChanged(axis, x)
                    }
                    .Bind(ref slider)
                    .InNamedRail(name);
            }

            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    JustifyContent = Justify.FlexStart,
                    Gap = 2f
                },

                Children = {
                    //
                    CreateAxisSlider("X", 0, ref _xSlider),
                    //
                    CreateAxisSlider("Y", 1, ref _ySlider),
                    //
                    CreateAxisSlider("Z", 2, ref _zSlider),
                    //
                    new BsSlider {
                        MinValue = -180f,
                        MaxValue = 180f,
                        ValueStep = 20f,
                        Value = 0f,
                        ValueFormatter = x => $"{x}\u00b0",
                        OnValueChanged = HandleRotationChanged
                    }.Bind(ref _rotationSlider).InNamedRail("Rotation"),
                    //
                    new BsTextButton {
                        Text = "Reset",
                        Skew = 0f,
                        OnClick = HandlePositionReset
                    }.InNamedRail("Reset Pose"),
                }
            }.Use();
        }

        #endregion

        #region Callbacks

        private void HandleAxisChanged(int axis, float value) {
            if (EditableView is not { } view) return;
            Vector3 position = view.Position;
            position[axis] = value;
            view.Position = position;
        }

        private void HandleRotationChanged(float rotation) {
            EditableView?.Rotation = rotation;
        }

        private void HandlePositionReset() {
            _cameraView?.Reset();
            LoadFromView();
        }

        #endregion
    }
}