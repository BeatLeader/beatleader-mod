using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.UI.Reactive.Components;
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
        private float _smoothness;
        private bool _keepUpright;
        private bool _keepStraight;
        private Vector3 _positionOffset;

        private void LoadFromView() {
            if (_cameraView == null) return;
            _smoothnessSlider.SetValueSilent(_cameraView.Smoothness);
            _offsetSlider.SetValueSilent(-_cameraView.PositionOffset.z);
            _keepUprightToggle.SetActive(_cameraView.KeepUpright, false, true);
            _keepStraightToggle.SetActive(_cameraView.KeepStraight, false, true);
            _smoothness = _cameraView.Smoothness;
            _keepUpright = _cameraView.KeepUpright;
            _keepStraight = _cameraView.KeepStraight;
            _positionOffset = _cameraView.PositionOffset;
        }

        private void RefreshView() {
            if (_cameraView == null) return;
            _cameraView.Smoothness = _smoothness;
            _cameraView.PositionOffset = _positionOffset;
            _cameraView.KeepUpright = _keepUpright;
            _cameraView.KeepStraight = _keepStraight;
        }

        #endregion

        #region Construct

        private Slider _smoothnessSlider = null!;
        private Slider _offsetSlider = null!;
        private Toggle _keepUprightToggle = null!;
        private Toggle _keepStraightToggle = null!;
        
        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new Slider {
                        ValueRange = new() {
                            Start = 0.0f,
                            End = PlayerViewCameraView.SMOOTHNESS_MAX
                        },
                        Value = 9f,
                        ValueStep = 0.1f
                    }.WithListener(
                        x => x.Value,
                        HandleSmoothnessChanged
                    ).Bind(ref _smoothnessSlider).InNamedRail("Smoothness"),
                    //
                    new Slider {
                        ValueRange = new() {
                            Start = 0f,
                            End = 2f
                        },
                        Value = 1f,
                        ValueStep = 0.1f
                    }.WithListener(
                        x => x.Value,
                        HandleOffsetChanged
                    ).Bind(ref _offsetSlider).InNamedRail("Z Offset"),
                    //
                    new Toggle().WithListener(
                        x => x.Active,
                        HandleKeepUprightChanged
                    ).Bind(ref _keepUprightToggle).InNamedRail("Keep Upright (Lock Z)"),
                    //
                    new Toggle().WithListener(
                        x => x.Active,
                        HandleKeepStraightChanged
                    ).Bind(ref _keepStraightToggle).InNamedRail("Keep Straight (Lock X)"),
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart,
                gap: 2f
            ).Use();
        }

        #endregion

        #region Callbacks

        private void HandleSmoothnessChanged(float value) {
            _smoothness = value;
            RefreshView();
        }
        
        private void HandleOffsetChanged(float value) {
            _positionOffset = new(0f, 0f, -value);
            RefreshView();
        }

        private void HandleKeepUprightChanged(bool value) {
            _keepUpright = value;
            RefreshView();
        }
        
        private void HandleKeepStraightChanged(bool value) {
            _keepStraight = value;
            RefreshView();
        }
        
        private void HandlePositionOffsetChanged(Vector3 vector) {
            _positionOffset = vector;
            RefreshView();
        }

        #endregion
    }
}