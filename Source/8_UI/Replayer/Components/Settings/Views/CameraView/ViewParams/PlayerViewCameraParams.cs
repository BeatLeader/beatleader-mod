using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
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
        private Vector3 _positionOffset;

        private void LoadFromView() {
            if (_cameraView == null) return;
            _smoothnessSlider.SetValueSilent(_cameraView.Smoothness);
            _keepUprightToggle.SetActive(_cameraView.KeepUpright, false, true);
            _smoothness = _cameraView.Smoothness;
            _keepUpright = _cameraView.KeepUpright;
            _positionOffset = _cameraView.PositionOffset;
        }

        private void RefreshView() {
            if (_cameraView == null) return;
            _cameraView.Smoothness = _smoothness;
            _cameraView.PositionOffset = _positionOffset;
            _cameraView.KeepUpright = _keepUpright;
        }

        #endregion

        #region Construct

        private Slider _smoothnessSlider = null!;
        private Toggle _keepUprightToggle = null!;
        
        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new Slider {
                        ValueRange = {
                            Start = 0.1f,
                            End = 10f
                        },
                        Value = 1f,
                        ValueStep = 0.1f
                    }.WithListener(
                        x => x.Value,
                        HandleSmoothnessChanged
                    ).Bind(ref _smoothnessSlider).InNamedRail("Smoothness"),
                    //
                    new Toggle().WithListener(
                        x => x.Active,
                        HandleKeepUprightChanged
                    ).Bind(ref _keepUprightToggle).InNamedRail("Keep Upright")
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

        private void HandleKeepUprightChanged(bool value) {
            _keepUpright = value;
            RefreshView();
        }
        
        private void HandlePositionOffsetChanged(Vector3 vector) {
            _positionOffset = vector;
            RefreshView();
        }

        #endregion
    }
}