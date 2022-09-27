using BeatLeader.UI.BSML_Addons.Components;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR;

namespace BeatLeader.Components
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Floating.FloatingResetter.bsml")]
    internal class FloatingResetter : MonoBehaviour
    {
        public Transform Transform { get; set; }

        public InputDevice inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        public InputFeatureUsage<bool> button = CommonUsages.secondaryButton;
        public Pose resetPose;
        public int preAnimTime = 300;
        public int resetTime = 700;

        [UIComponent("animation-image")] private BetterImage _image;

        private Stopwatch _stopwatch = new();
        private ResetUIAnimator _animator;
        private bool _wasExecuted;

        private void Awake()
        {
            this.ParseInObjectHierarchy();
            _animator = _image.gameObject.AddComponent<ResetUIAnimator>();
            _animator.image = _image.Image;
            _animator.RevealWasFinishedEvent += HandleRevealFinished;
        }
        private void Update()
        {
            inputDevice.TryGetFeatureValue(button, out bool state);

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

                if (_stopwatch.ElapsedMilliseconds >= preAnimTime)
                {
                    _animator.StartAnimation((float)resetTime / 1000);
                    _wasExecuted = true;
                }
            }
        }

        private void HandleRevealFinished(bool result)
        {
            if (!result) return;

            Transform?.SetLocalPose(resetPose);
            _stopwatch.Stop();
            _stopwatch.Reset();
        }
    }
}
