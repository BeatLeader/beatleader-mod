using System;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
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

        private AnimatedState<float> _alpha = null!;
        private AnimatedState<float> _fill = null!;

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
            //checking did state change or not
            if (pressed != _lastPressState) {
                _lastPressTime = Time.time;
                _lastPressState = pressed;
                _resetFinished = false;
                if (pressed) {
                    _alpha.TargetValue = 1f;
                    _fill.SetValueImmediate(0f);
                } else {
                    _alpha.TargetValue = 0f;
                    _fill.TargetValue = 0f;
                }
            }
            //checking is finished
            if (!_resetFinished && pressed) {
                _fill.SetValueImmediate(Mathf.Clamp01((Time.time - _lastPressTime) / _holdTime));
                if (Time.time - _lastPressTime >= _holdTime) {
                    _alpha.TargetValue = 0f;
                    _resetFinished = true;
                    ResetRequestedEvent?.Invoke();
                }
            }
        }

        #endregion

        #region Construct

        private Canvas _canvas = null!;

        protected override GameObject Construct() {
            _alpha = RememberAnimated(0f, 10.fact);
            _fill = RememberAnimated(0f, 5.fact);

            return new Layout {
                Children = {
                    new Image {
                        Sprite = BundleLoader.ProgressRingIcon,
                        ImageType = UnityEngine.UI.Image.Type.Filled,
                        FillMethod = UnityEngine.UI.Image.FillMethod.Radial360,
                        RaycastTarget = false,
                        sFillAmount = _fill,
                        sColor = _alpha.Map(static x => Color.white.ColorWithAlpha(x))
                    }.WithSizeDelta(300f, 300f)
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