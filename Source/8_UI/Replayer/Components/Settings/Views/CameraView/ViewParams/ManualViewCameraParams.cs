using System;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.UI.Reactive.Components;
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
        private Vector3 _position;
        private float _rotation;

        private void LoadFromView() {
            if (_cameraView == null) return;
            _position = _cameraView.Position;
            _rotation = _cameraView.Rotation;
            _xSlider.SetValueSilent(_position.x);
            _ySlider.SetValueSilent(_position.y);
            _zSlider.SetValueSilent(_position.z);
            _rotationSlider.SetValueSilent(_rotation);
        }

        private void RefreshView() {
            if (_cameraView == null) return;
            _cameraView.Position = _position;
            _cameraView.Rotation = _rotation;
        }

        #endregion

        #region Construct

        private Slider _xSlider = null!;
        private Slider _ySlider = null!;
        private Slider _zSlider = null!;
        private Slider _rotationSlider = null!;

        protected override GameObject Construct() {
            static NamedRail CreateAxisSlider(string name, Action<float> callback, ref Slider slider) {
                return new Slider {
                        ValueRange = new() {
                            Start = -10f,
                            End = 10f
                        },
                        Value = 0f,
                        ValueStep = 0.1f,
                        ValueFormatter = x => $"{Mathf.Round(x * 10f) / 10f}m"
                    }
                    .WithListener(x => x.Value, callback)
                    .Bind(ref slider)
                    .InNamedRail(name);
            }

            return new Layout {
                Children = {
                    //
                    CreateAxisSlider("X", HandleXChanged, ref _xSlider),
                    //
                    CreateAxisSlider("Y", HandleYChanged, ref _ySlider),
                    //
                    CreateAxisSlider("Z", HandleZChanged, ref _zSlider),
                    //
                    new Slider {
                        ValueRange = new() {
                            Start = -180f,
                            End = 180f
                        },
                        Value = 0f,
                        ValueStep = 20f,
                        ValueFormatter = x => $"{x}\u00b0"
                    }.WithListener(
                        x => x.Value,
                        HandleRotationChanged
                    ).Bind(ref _rotationSlider).InNamedRail("Rotation"),
                    //
                    new BsButton {
                            Text = "Reset",
                            Skew = 0f,
                            OnClick = HandlePositionReset
                        }
                        .AsFlexItem()
                        .InNamedRail("Reset Pose"),
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart,
                gap: 2f
            ).Use();
        }

        #endregion

        #region Callbacks

        private void HandleXChanged(float pos) {
            _position.x = pos;
            RefreshView();
        }

        private void HandleYChanged(float pos) {
            _position.y = pos;
            RefreshView();
        }

        private void HandleZChanged(float pos) {
            _position.z = pos;
            RefreshView();
        }

        private void HandleRotationChanged(float pos) {
            _rotation = pos;
            RefreshView();
        }

        private void HandlePositionReset() {
            _cameraView?.Reset();
            LoadFromView();
        }

        #endregion
    }
}