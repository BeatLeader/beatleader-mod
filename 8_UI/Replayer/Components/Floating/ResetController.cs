using BeatLeader.UI.BSML_Addons.Components;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR;

namespace BeatLeader.Components
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Floating.ResetProgressUI.bsml")]
    internal class ResetController : MonoBehaviour
    {
        #region Configuration

        private const int preHoldTime = 300;
        private const int holdTime = 700;

        #endregion

        #region UI Components

        [UIComponent("animation-image")] private readonly BetterImage _image;

        #endregion

        #region Setup

        public Transform ResetTransform
        {
            get => _resetTransform;
            set
            {
                _resetTransform = value;
                _initialized = value != null;
            }
        }

        public InputDevice actionInputDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        public InputFeatureUsage<bool> actionFeature = CommonUsages.secondaryButton;
        public Pose pose;

        private ResetProgressAnimator _animator;
        private Transform _resetTransform;
        private bool _initialized;

        private void Awake()
        {
            this.ParseInObjectHierarchy();
            _animator = gameObject.AddComponent<ResetProgressAnimator>();
            _animator.SetImage(_image.Image);
            _animator.RevealWasFinishedEvent += HandleRevealWasFinished;
        }

        #endregion

        #region Events

        public event Action PoseWasResettedEvent;

        #endregion

        #region Logic

        private Stopwatch _stopwatch = new();
        private bool _wasExecuted;

        private void Update()
        {
            if (!_initialized) return;

            actionInputDevice.TryGetFeatureValue(actionFeature, out bool state);

            if (!state)
            {
                _animator.CancelAnimation();
                _stopwatch.Stop();
                _stopwatch.Reset();
                _wasExecuted = false;
                return;
            }

            if (!_wasExecuted)
            {
                _stopwatch.Start();

                if (_stopwatch.ElapsedMilliseconds >= preHoldTime)
                {
                    _animator.StartAnimation((float)holdTime / 1000);
                    _wasExecuted = true;
                }
            }
        }

        #endregion

        #region Event Handlers

        private void HandleRevealWasFinished(bool result)
        {
            if (!result) return;

            _stopwatch.Stop();
            _stopwatch.Reset();

            ResetTransform?.SetLocalPose(pose);
            PoseWasResettedEvent?.Invoke();
        }

        #endregion
    }
}
