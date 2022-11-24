using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    internal class FloatingControls : ReeUIComponentV2 {
        #region UI Components

        [UIValue("pin-button")] private ToggleButton _pinButton;
        [UIValue("align-button")] private SimpleButton _alignButton;
        [UIObject("spacer-one")] private GameObject _spacerOne;
        [UIObject("spacer-two")] private GameObject _spacerTwo;

        #endregion

        #region Setup

        private Models.IReplayPauseController _pauseController;
        private ResetController _resetController;
        private FloatingScreen _resetFloating;

        private FloatingScreen _viewFloating;
        private Transform _head;
        private bool _hideUI;

        public void Setup(
            FloatingScreen floating,
            Models.IReplayPauseController pauseController,
            Transform headTransform,
            bool hideUI = false) {
            if (_pauseController != null)
                _pauseController.PauseStateChangedEvent -= HandlePauseStateChanged;

            if (_viewFloating != null)
                _viewFloating.HandleReleased -= HandleFloatingHandleWasReleased;

            _viewFloating = floating;
            _head = headTransform;
            _pauseController = pauseController;

            if (_resetController != null)
                _resetController.ResetTransform = _viewFloating.transform;

            if (_resetFloating != null) {
                _resetFloating.transform.SetParent(_head, false);
                _resetFloating.transform.localPosition = new Vector3(0, 0, 0.7f);
            }

            _pauseController.PauseStateChangedEvent += HandlePauseStateChanged;
            _viewFloating.HandleReleased += HandleFloatingHandleWasReleased;

            _hideUI = hideUI;
            _viewFloating.transform.SetLocalPose(LoadPoseFromConfig());
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
            _resetController.pose = defaultPose;
            _resetController.ResetTransform = _viewFloating?.transform ?? null;

            LoadConfig();
        }

        #endregion

        #region Config

        private static readonly Pose defaultPose = new(new(0, 1, 2), Quaternion.Euler(new(40, 0, 0)));

        private FloatingConfig Config => FloatingConfig.Instance;

        private void LoadConfig() {
            _viewFloating?.transform.SetLocalPose(LoadPoseFromConfig());
            _pinButton.Toggle(Config.IsPinned);
            HandlePauseStateChanged(false);
        }
        private Pose LoadPoseFromConfig() {
            return new Pose(Config.Position, Config.Rotation);
        }
        private void SavePoseToConfig() {
            SavePoseToConfig(_viewFloating?.transform.GetLocalPose() ?? default);
        }
        private void SavePoseToConfig(Pose pose) {
            Config.Position = pose.position;
            Config.Rotation = pose.rotation;
        }

        #endregion

        #region Event Handlers

        private void HandlePinToggled(bool pin) {
            if (_viewFloating == null) return;

            Config.IsPinned = pin;
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
                pos[i] = MathUtils.GetClosestCoordinate(transform.localPosition[i], Config.GridPosIncrement);
                rot[i] = MathUtils.GetClosestCoordinate(transform.localEulerAngles[i], Config.GridRotIncrement);
            }

            var pose = new Pose(pos, Quaternion.Euler(rot));

            transform.SetLocalPose(pose);
            SavePoseToConfig(pose);
        }
        private void HandleFloatingHandleWasReleased(object sender, FloatingScreenHandleEventArgs args) {
            SavePoseToConfig();
        }
        private void HandlePoseWasResetted() {
            SavePoseToConfig();
        }
        private void HandlePauseStateChanged(bool state) {
            if (!_hideUI || _viewFloating == null) return;

            _viewFloating.gameObject.SetActive(state);
            _resetController.enabled = state;
        }

        #endregion
    }
}
