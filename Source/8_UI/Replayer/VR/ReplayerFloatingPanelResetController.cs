using System;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using UnityEngine;
using UnityEngine.XR;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerFloatingPanelResetController : ReactiveComponent {
        #region Setup

        public void Setup(Camera renderCamera) {
            _canvas.worldCamera = renderCamera;
        }

        #endregion

        #region Animation

        private readonly ValueAnimator _alphaAnimator = new();
        private readonly ValueAnimator _fillAnimator = new() { LerpCoefficient = 5f };

        private void RefreshAnimation() {
            _image.FillAmount = _fillAnimator.Progress;
            _image.Color = Color.white.ColorWithAlpha(_alphaAnimator.Progress);
        }

        #endregion

        #region Input

        public event Action? ResetRequestedEvent;

        private InputDevice _inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        private readonly InputFeatureUsage<bool> _buttonFeature = CommonUsages.secondaryButton;

        private float _holdTime = 2f;
        private float _lastPressTime;
        private bool _lastPressState;
        private bool _resetFinished;

        protected override void OnUpdate() {
            if (!_inputDevice.TryGetFeatureValue(_buttonFeature, out var pressed)) return;
            //updating animators
            _alphaAnimator.Update();
            if (!pressed) _fillAnimator.Update();
            //checking did state change or not
            if (pressed != _lastPressState) {
                _lastPressTime = Time.time;
                _lastPressState = pressed;
                _resetFinished = false;
                if (pressed) {
                    _alphaAnimator.Push();
                    _fillAnimator.SetProgress(0f);
                } else {
                    _alphaAnimator.Pull();
                }
            }
            //checking is finished
            if (!_resetFinished && pressed) {
                _fillAnimator.SetProgress((Time.time - _lastPressTime) / _holdTime);
                if (Time.time - _lastPressTime >= _holdTime) {
                    _alphaAnimator.Pull();
                    _resetFinished = true;
                    ResetRequestedEvent?.Invoke();
                }
            }
            //refreshing animations
            RefreshAnimation();
        }

        #endregion

        #region Construct

        private Canvas _canvas = null!;
        private Image _image = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new Image {
                        Sprite = BundleLoader.ProgressRingIcon,
                        ImageType = UnityEngine.UI.Image.Type.Filled,
                        FillMethod = UnityEngine.UI.Image.FillMethod.Radial360,
                        RaycastTarget = false
                    }.Bind(ref _image).WithSizeDelta(300f, 300f)
                }
            }.Use();
        }

        protected override void OnInitialize() {
            Content.AddComponent<FloatingScreen>();
            _canvas = Content.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.planeDistance = 0.5f;
            _canvas.sortingOrder = 10000;
        }

        #endregion
    }
}