using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    internal class FloatingControls : ReeUIComponentV2 {
        #region UI Components

        [UIValue("pin-button")]
        private ToggleButton _pinButton = null!;

        [UIValue("align-button")]
        private SimpleButton _alignButton = null!;

        [UIObject("spacer-one")]
        private readonly GameObject _spacerOne = null!;

        [UIObject("spacer-two")]
        private readonly GameObject _spacerTwo = null!;

        #endregion

        #region Setup

        private ResetController _resetController = null!;
        private FloatingScreen _resetFloating = null!;

        private FloatingScreen _viewFloating = null!;
        private Transform _head = null!;

        public void Setup(
            FloatingScreen floating,
            Transform headTransform) {
            if (_viewFloating != null)
                _viewFloating.HandleReleased -= HandleFloatingHandleWasReleased;

            _viewFloating = floating;
            _head = headTransform;

            if (_resetController != null)
                _resetController.SetResetObject(_viewFloating.transform);

            if (_resetFloating != null) {
                _resetFloating.transform.SetParent(_head, false);
                _resetFloating.transform.localPosition = new Vector3(0, 0, 0.7f);
            }

            _viewFloating.HandleReleased += HandleFloatingHandleWasReleased;
            _viewFloating.transform.SetLocalPose(FloatingConfig.Instance.Pose);
        }

        protected override void OnInstantiate() {
            _pinButton = Instantiate<ToggleButton>(transform);
            _alignButton = Instantiate<SimpleButton>(transform);
        }

        protected override void OnInitialize() {
            _pinButton.OnToggle += HandlePinToggled;
            _pinButton.DisabledSprite = BundleLoader.PinIcon;
            _pinButton.enabledColor = Color.cyan;

            _alignButton.OnClick += HandleAlignPressed;
            _alignButton.Sprite = BundleLoader.AlignIcon;
            _alignButton.highlightedColor = Color.cyan;

            _resetFloating = FloatingScreen.CreateFloatingScreen(new Vector2(6, 6), false, Vector3.zero, Quaternion.identity);
            _resetFloating.GetComponent<BaseRaycaster>().TryDestroy();
            _resetFloating.transform.SetParent(_head, false);
            _resetFloating.transform.localPosition = new Vector3(0, 0, 0.7f);

            _resetController = _resetFloating.gameObject.AddComponent<ResetController>();
            _resetController.PoseWasResettedEvent += HandlePoseWasResetted;
            _resetController.pose = FloatingConfig.DefaultPose;
            _resetController.SetResetObject(_viewFloating?.transform ?? null!);

            _viewFloating?.transform.SetLocalPose(FloatingConfig.Instance.Pose);
            _pinButton.Toggle(FloatingConfig.Instance.IsPinned);
        }

        #endregion

        #region Event Handlers

        private void HandlePinToggled(bool pin) {
            if (_viewFloating == null) return;

            FloatingConfig.Instance.IsPinned = pin;
            _viewFloating.handle.gameObject.SetActive(!pin);
            _spacerOne.SetActive(!pin);
            _spacerTwo.SetActive(!pin);
        }

        private void HandleAlignPressed() {
            if (_viewFloating.transform == null) return;

            var pos = new Vector3();
            var rot = new Vector3();
            var transform = _viewFloating.transform;

            for (int i = 0; i <= 2; i++) {
                pos[i] = MathUtils.GetClosestCoordinate(transform.localPosition[i], FloatingConfig.Instance.GridPosIncrement);
                rot[i] = MathUtils.GetClosestCoordinate(transform.localEulerAngles[i], FloatingConfig.Instance.GridRotIncrement);
            }

            var pose = new Pose(pos, Quaternion.Euler(rot));

            transform.SetLocalPose(pose);
            FloatingConfig.Instance.Pose = pose;
        }

        private void HandleFloatingHandleWasReleased(object sender, FloatingScreenHandleEventArgs args) {
            HandlePoseWasResetted();
        }

        private void HandlePoseWasResetted() {
            FloatingConfig.Instance.Pose = _viewFloating?.transform.GetLocalPose() ?? default(Pose);
        }

        #endregion
    }
}
