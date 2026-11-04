using System;
using BeatLeader.Models;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
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
            SetPinned(settings.Pinned);
            SetSnapEnabled(settings.SnapEnabled);
            _screen.transform.SetLocalPose(settings.Pose);
        }

        #endregion

        #region Construct

        private ReplayerFloatingPanelResetController _resetController = null!;
        private ReplayerFloatingPanelCurvatureSettings _curvatureSettings = null!;

        private FloatingHandle _floatingHandle = null!;
        private RectTransform _handleContainer = null!;
        private bool _lastHandleState;

        private State<GraphicState> _pinButtonState = null!;
        private State<GraphicState> _snapButtonState = null!;

        [StateGen]
        private static Image CreateButton(Sprite sprite, State<GraphicState> state, Action onClick) {
            return new Image {
                Sprite = sprite,
                Material = BundleLoader.Materials.uiAdditiveGlowMaterial,
                sColor = state.MapColorSet(UIStyle.GlowingButtonColorSet)
            }.WithPointerEvents(
                onEnter: _ => state.IsHovered = true,
                onLeave: _ => state.IsHovered = false,
                onDown: _ => {
                    onClick();
                    global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                }
            );
        }

        private void SetPinned(bool pinned) {
            _pinButtonState.IsActive = pinned;
            HandlePinStateChanged(pinned);
        }

        private void SetSnapEnabled(bool enabled) {
            _snapButtonState.IsActive = enabled;
            HandleSnapStateChanged(enabled);
        }

        protected override GameObject Construct() {
            _pinButtonState = Remember(GraphicState.None);
            _snapButtonState = Remember(GraphicState.None);

            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column
                },

                Children = {
                    //controls
                    new Background {
                        FlexController = {
                            Gap = 2f,
                            Padding = new() { top = 1f, bottom = 1f, right = 2f, left = 2f }
                        },

                        FlexItem = {
                            Size = new() { y = 6f }
                        },

                        Children = {
                            //pin button
                            CreateButton(
                                BundleLoader.Sprites.pinIcon,
                                _pinButtonState,
                                () => SetPinned(!_pinButtonState.IsActive)
                            ).AsFlexItem(size: 4f),
                            //snap button
                            CreateButton(
                                BundleLoader.Sprites.snapIcon,
                                _snapButtonState,
                                () => SetSnapEnabled(!_snapButtonState.IsActive)
                            ).AsFlexItem(size: 4f),
                            //curvature button
                            CreateButton(
                                BundleLoader.Sprites.curvatureIcon,
                                Remember(GraphicState.None),
                                HandleCurvatureButtonClicked
                            ).AsFlexItem(size: 4f),
                        }
                    }.AsBackground(
                        color: new(0.1f, 0.1f, 0.1f, 1f),
                        pixelsPerUnit: 7f
                    ),
                    //handle container
                    new Layout {
                        FlexItem = {
                            Size = new() { y = 6f }
                        },

                        ContentTransform = {
                            localScale = new(7f, 3f, 3f)
                        }
                    }.Bind(ref _handleContainer)
                }
            }.Use();
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
            _curvatureSettings.IsPresent = true;
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
            _curvatureSettings.IsPresent = false;
            if (_lastHandleState) {
                _floatingHandle.Present();
            }
        }

        #endregion
    }
}