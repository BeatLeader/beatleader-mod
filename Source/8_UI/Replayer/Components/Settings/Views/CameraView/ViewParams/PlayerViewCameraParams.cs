using BeatLeader.Models;
using BeatLeader.Replayer;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class PlayerViewCameraParams : ReactiveComponent, SettingsCameraView.ICameraViewParams {
        #region ViewParams

        public bool SupportsCameraView(ICameraView cameraView) {
            return cameraView is PlayerViewCameraView;
        }

        public void Setup(ICameraView? cameraView) {
            _cameraView = cameraView as PlayerViewCameraView;
            LoadFromView();
        }

        #endregion

        #region View

        private PlayerViewCameraView? _cameraView;
        private bool _loadingFromView;

        private PlayerViewCameraView? EditableView => _loadingFromView ? null : _cameraView;

        private void LoadFromView() {
            if (_cameraView == null) return;
            _loadingFromView = true;
            _smoothnessSlider.Value = _cameraView.Smoothness;
            _offsetSlider.Value = -_cameraView.PositionOffset.z;
            _keepUprightToggle.Toggled = _cameraView.KeepUpright;
            _keepStraightToggle.Toggled = _cameraView.KeepStraight;
            _loadingFromView = false;
        }

        #endregion

        #region Construct

        private BsSlider _smoothnessSlider = null!;
        private BsSlider _offsetSlider = null!;
        private BsToggle _keepUprightToggle = null!;
        private BsToggle _keepStraightToggle = null!;

        protected override GameObject Construct() {
            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    JustifyContent = Justify.FlexStart,
                    Gap = 2f
                },

                Children = {
                    new BsSlider {
                        MinValue = 0.0f,
                        MaxValue = PlayerViewCameraView.SMOOTHNESS_MAX,
                        ValueStep = 0.1f,
                        Value = 9f,
                        OnValueChanged = HandleSmoothnessChanged
                    }.Bind(ref _smoothnessSlider).InNamedRail("Smoothness"),
                    //
                    new BsSlider {
                        MinValue = 0f,
                        MaxValue = 2f,
                        ValueStep = 0.1f,
                        Value = 1f,
                        OnValueChanged = HandleOffsetChanged
                    }.Bind(ref _offsetSlider).InNamedRail("Z Offset"),
                    //
                    new BsToggle {
                        OnToggleStateChanged = HandleKeepUprightChanged
                    }.Bind(ref _keepUprightToggle).InNamedRail("Keep Upright (Lock Z)"),
                    //
                    new BsToggle {
                        OnToggleStateChanged = HandleKeepStraightChanged
                    }.Bind(ref _keepStraightToggle).InNamedRail("Keep Straight (Lock X)"),
                }
            }.Use();
        }

        #endregion

        #region Callbacks

        private void HandleSmoothnessChanged(float value) {
            EditableView?.Smoothness = value;
        }

        private void HandleOffsetChanged(float value) {
            EditableView?.PositionOffset = new Vector3(0f, 0f, -value);
        }

        private void HandleKeepUprightChanged(bool value) {
            EditableView?.KeepUpright = value;
        }

        private void HandleKeepStraightChanged(bool value) {
            EditableView?.KeepStraight = value;
        }

        #endregion
    }
}