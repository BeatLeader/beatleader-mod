using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class FlyingViewCameraParams : ReactiveComponent, SettingsCameraView.ICameraViewParams {
        #region ViewParams

        public bool SupportsCameraView(ICameraView cameraView) {
            return cameraView is FlyingCameraView;
        }

        public void Setup(ICameraView? cameraView) {
            _cameraView = cameraView as FlyingCameraView;
            LoadFromView();
        }

        #endregion

        #region View

        private FlyingCameraView? _cameraView;
        private int _speed;
        private Vector2 _sensitivity;

        private void LoadFromView() {
            if (_cameraView == null) return;
            _speed = _cameraView.FlySpeed;
            _sensitivity = _cameraView.MouseSensitivity;
        }
        
        private void RefreshView() {
            if (_cameraView == null) return;
            _cameraView.FlySpeed = _speed;
            _cameraView.MouseSensitivity = _sensitivity;
        }

        #endregion

        #region Construct

        private Slider _smoothnessSlider = null!;
    
        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new Slider {
                        ValueRange = {
                            Start = 1f,
                            End = 8f
                        },
                        Value = 5f,
                        ValueStep = 1f
                    }.WithListener(
                        x => x.Value,
                        HandleSpeedChanged
                    ).Bind(ref _smoothnessSlider).InNamedRail("Speed"),
                    //
                    new BsButton {
                            Skew = 0f,
                            OnClick = HandlePositionReset
                        }
                        .WithLabel("Reset")
                        .AsFlexItem()
                        .InNamedRail("Reset Position"),
                    //hint
                    new Label {
                        Text = "\u24d8 Notice: Free view starts working only once you press C!",
                        Alignment = TextAlignmentOptions.Center
                    }.AsFlexItem(margin: new() { top = 2f })
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart,
                gap: 2f
            ).Use();
        }

        #endregion

        #region Callbacks

        private void HandleSpeedChanged(float speed) {
            _speed = (int)speed;
            RefreshView();
        }

        private void HandlePositionReset() {
            _cameraView?.Reset();
        }

        #endregion
    }
}