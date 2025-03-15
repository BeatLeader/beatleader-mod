using BeatLeader.UI.BSML_Addons.Components;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Floating.ResetProgressUI.bsml")]
    internal class ResetController : MonoBehaviour {
        #region Configuration

        private const int PreHoldTime = 300;
        private const int HoldTime = 700;

        #endregion

        #region UI Components

        [UIComponent("animation-image")]
        private readonly BetterImage _image = null!;

        #endregion

        #region Setup

        public Transform? ResetTransform { get; private set; }
        public Pose pose;

        public InputDevice actionInputDevice;
        public InputFeatureUsage<bool> actionFeature;

        private ResetProgressAnimator _animator = null!;
        private bool _initialized;

        private void Awake() {
            this.ParseInObjectHierarchy();
            actionInputDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            actionFeature = CommonUsages.secondaryButton;
            _animator = gameObject.AddComponent<ResetProgressAnimator>();
            _animator.SetImage(_image.Image);
            _animator.RevealWasFinishedEvent += HandleRevealWasFinished;
        }

        public void SetResetObject(Transform? obj) {
            ResetTransform = obj;
            _initialized = obj != null;
            enabled = _initialized;
        }

        #endregion

        #region Events

        public event Action? PoseWasResetEvent;

        #endregion

        #region Logic

        private readonly Stopwatch _stopwatch = new();
        private bool _wasExecuted;

        private void Update() {
            if (!_initialized || actionInputDevice == null) return;

            actionInputDevice.TryGetFeatureValue(actionFeature, out var state);

            if (!state) {
                _animator.CancelAnimation();
                _stopwatch.Stop();
                _stopwatch.Reset();
                _wasExecuted = false;
                return;
            }

            if (_wasExecuted) return;
            _stopwatch.Start();

            if (_stopwatch.ElapsedMilliseconds < PreHoldTime) return;
            _animator.StartAnimation((float)HoldTime / 1000);
            _wasExecuted = true;
        }

        #endregion

        #region Callbacks

        private void HandleRevealWasFinished(bool result) {
            if (!result) return;

            _stopwatch.Stop();
            _stopwatch.Reset();

            if (ResetTransform != null) {
                ResetTransform.SetLocalPose(pose);
            }
            PoseWasResetEvent?.Invoke();
        }

        #endregion
    }
}
