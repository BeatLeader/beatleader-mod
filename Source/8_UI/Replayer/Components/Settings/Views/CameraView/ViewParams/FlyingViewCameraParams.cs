using BeatLeader.Models;
using BeatLeader.Replayer;
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
            if (_cameraView != null) {
                _speedSlider.Value = _cameraView.FlySpeed;
            }
        }

        private FlyingCameraView? _cameraView;
        private BsSlider _speedSlider = null!;

        #endregion

        #region Construct

        protected override GameObject Construct() {
            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    JustifyContent = Justify.FlexStart,
                    Gap = 2f
                },

                Children = {
                    new BsSlider {
                        MinValue = 1f,
                        MaxValue = 8f,
                        ValueStep = 1f,
                        Value = 4f,
                        OnValueChanged = HandleSpeedChanged
                    }.Bind(ref _speedSlider).InNamedRail("Speed"),
                    //
                    new BsTextButton {
                        FlexItem = {
                            Size = new() { x = 12f }
                        },

                        Text = "Reset",
                        Skew = 0f,
                        OnClick = HandlePositionReset
                    }.InNamedRail("Reset Position"),
                    //hint
                    new Label {
                        FlexItem = {
                            Margin = new() { top = 2f }
                        },

                        Text = "\u24d8 Notice: Free view starts working only once you press C!",
                        Alignment = TextAlignmentOptions.Center
                    }
                }
            }.Use();
        }

        #endregion

        #region Callbacks

        private void HandleSpeedChanged(float speed) {
            _cameraView?.FlySpeed = (int)speed;
        }

        private void HandlePositionReset() {
            _cameraView?.Reset();
        }

        #endregion
    }
}