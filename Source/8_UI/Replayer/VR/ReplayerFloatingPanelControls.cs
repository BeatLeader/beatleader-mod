using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerFloatingPanelControls : ReactiveComponent {
        #region Setup

        private FloatingScreen? _screen;
        private ReplayerFloatingUISettings? _settings;
        private bool _isInitialized;

        public void Setup(FloatingScreen screen, Camera camera, ReplayerFloatingUISettings settings) {
            _screen = screen;
            _settings = settings;
            _floatingHandle.Setup(screen.transform);
            _resetController.Setup(camera);
            _curvatureSettings.Setup(screen, settings);
            _isInitialized = true;
            //applying controls
            _pinButton.Click(settings.Pinned, true, true);
            _snapButton.Click(settings.SnapEnabled, true, true);
            _screen.transform.SetLocalPose(settings.Pose);
        }

        #endregion

        #region Construct

        private ReplayerFloatingPanelResetController _resetController = null!;
        private ReplayerFloatingPanelCurvatureSettings _curvatureSettings = null!;

        private FloatingHandle _floatingHandle = null!;
        private RectTransform _handleContainer = null!;
        private bool _lastHandleState;

        private ImageButton _pinButton = null!;
        private ImageButton _snapButton = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    //controls
                    new Image {
                        Children = {
                            //pin button
                            new ImageButton {
                                Image = {
                                    Sprite = BundleLoader.Sprites.pinIcon
                                },
                                Sticky = true,
                            }.WithStateListener(HandlePinStateChanged).AsFlexItem(size: 4f).Bind(ref _pinButton),
                            //snap button
                            new ImageButton {
                                Image = {
                                    Sprite = BundleLoader.Sprites.snapIcon
                                },
                                Sticky = true
                            }.WithStateListener(HandleSnapStateChanged).AsFlexItem(size: 4f).Bind(ref _snapButton),
                            //curvature button
                            new ImageButton {
                                Image = {
                                    Sprite = BundleLoader.Sprites.curvatureIcon
                                }
                            }.WithClickListener(HandleCurvatureButtonClicked).AsFlexItem(size: 4f),
                        }
                    }.AsFlexGroup(
                        gap: 2f,
                        padding: new() { top = 1f, bottom = 1f, right = 2f, left = 2f }
                    ).AsBackground(
                        color: new(0.1f, 0.1f, 0.1f, 1f),
                        pixelsPerUnit: 7f
                    ).AsFlexItem(size: new() { y = 6f }),
                    //handle container
                    new Dummy {
                        ContentTransform = {
                            localScale = new(7f, 3f, 3f)
                        }
                    }.AsFlexItem(size: new() { y = 6f }).Bind(ref _handleContainer)
                }
            }.AsFlexGroup(direction: FlexDirection.Column).Use();
        }

        protected override void OnInitialize() {
            //creating handle
            var handleGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            handleGo.layer = 5;
            var handleTransform = handleGo.transform;
            handleTransform.SetParent(_handleContainer, false);
            handleTransform.localEulerAngles = new(0f, 0f, 90f);
            _floatingHandle = handleGo.AddComponent<FloatingHandle>();
            _floatingHandle.centerPoint = new(0f, 1.7f, 0f);
            _floatingHandle.PoseChangedEvent += HandleScreenPoseChanged;
            //reset controller
            _resetController = new();
            _resetController.ResetRequestedEvent += HandleResetRequested;
            _resetController.Use();
            //curvature settings
            _curvatureSettings = new();
            _curvatureSettings.CloseButtonClicked += HandleCurvatureSettingsCloseButtonClicked;
            _curvatureSettings.Use(ContentTransform);
        }

        protected override void OnDestroy() {
            //out of the controls hierarchy so may be destroyed after this object
            _resetController.ResetRequestedEvent -= HandleResetRequested;
        }

        #endregion

        #region Callbacks

        private void HandlePinStateChanged(bool state) {
            if (!_isInitialized) return;
            if (state) {
                _floatingHandle.Hide();
            } else {
                _floatingHandle.Present();
            }
            _lastHandleState = !state;
            _settings!.Pinned = state;
        }

        private void HandleSnapStateChanged(bool state) {
            if (!_isInitialized) return;
            _floatingHandle.lookAtCenterPoint = state;
            _settings!.SnapEnabled = state;
        }

        private void HandleCurvatureButtonClicked() {
            if (!_isInitialized) return;
            _curvatureSettings.Present();
            if (_lastHandleState) {
                _floatingHandle.Hide();
            }
        }

        private void HandleResetRequested() {
            if (!_isInitialized) return;
            _screen!.transform.SetLocalPose(_settings!.InitialPose);
        }

        private void HandleScreenPoseChanged(Pose pose) {
            if (!_isInitialized) return;
            _settings!.Pose = pose;
        }

        private void HandleCurvatureSettingsCloseButtonClicked() {
            _curvatureSettings.Hide();
            if (_lastHandleState) {
                _floatingHandle.Present();
            }
        }

        #endregion
    }
}