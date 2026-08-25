using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using HMUI;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerFloatingPanelControls : ReactiveComponent {
        #region Setup

        private FloatingScreen? _screen;
        private ToolbarWithSettings? _mainUi;
        private ReplayerFloatingUISettings? _settings;
        private bool _isInitialized;
        private bool _curvatureSettingsPresented;
        private bool _lastAttachToHandState;
        private bool _mainUiHidden;

        public void Setup(
            FloatingScreen screen,
            ToolbarWithSettings mainUi,
            MenuControllersManager menuControllersManager,
            Camera camera,
            ReplayerFloatingUISettings settings
        ) {
            _screen = screen;
            _mainUi = mainUi;
            _settings = settings;
            screen.gameObject.GetOrAddComponent<FloatingScreenHandFollower>().Setup(settings, menuControllersManager);
            _floatingHandle.Setup(screen.transform);
            _resetController.Setup(camera);
            _curvatureSettings.Setup(screen, settings);
            _isInitialized = true;
            _lastAttachToHandState = settings.AttachToHand;
            //applying controls
            _attachToHandButton.Click(settings.AttachToHand, true, true);
            _pinButton.Click(settings.Pinned, true, true);
            _snapButton.Click(settings.SnapEnabled, true, true);
            if (!settings.AttachToHand) {
                _screen.transform.SetLocalPose(settings.Pose);
            }
            RefreshControlsVisibility();
        }

        #endregion

        #region Construct

        private ReplayerFloatingPanelResetController _resetController = null!;
        private ReplayerFloatingPanelCurvatureSettings _curvatureSettings = null!;

        private FloatingHandle _floatingHandle = null!;
        private RectTransform _handleContainer = null!;

        private ImageButton _attachToHandButton = null!;
        private ImageButton _pinButton = null!;
        private ImageButton _snapButton = null!;
        private ImageButton _curvatureButton = null!;
        private ImageButton _hideButton = null!;
        private HoverHint _attachToHandHint = null!;
        private HoverHint _pinHint = null!;
        private HoverHint _snapHint = null!;
        private HoverHint _curvatureHint = null!;
        private HoverHint _hideHint = null!;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    //controls
                    new Background {
                        Children = {
                            //hand UI button
                            new ImageButton {
                                WithinLayoutIfDisabled = false,
                                Image = {
                                    Sprite = BundleLoader.Sprites.handUIIcon,
                                    Material = BundleLoader.Materials.uiAdditiveGlowMaterial
                                },
                                Colors = UIStyle.GlowingButtonColorSet,
                                Latching = true,
                                OnStateChanged = HandleAttachToHandStateChanged
                            }.AsFlexItem(size: 4f).Bind(ref _attachToHandButton),
                            //pin button
                            new ImageButton {
                                WithinLayoutIfDisabled = false,
                                Image = {
                                    Sprite = BundleLoader.Sprites.pinIcon,
                                    Material = BundleLoader.Materials.uiAdditiveGlowMaterial
                                },
                                Colors = UIStyle.GlowingButtonColorSet,
                                Latching = true,
                                OnStateChanged = HandlePinStateChanged
                            }.AsFlexItem(size: 4f).Bind(ref _pinButton),
                            //snap button
                            new ImageButton {
                                WithinLayoutIfDisabled = false,
                                Image = {
                                    Sprite = BundleLoader.Sprites.snapIcon,
                                    Material = BundleLoader.Materials.uiAdditiveGlowMaterial
                                },
                                Colors = UIStyle.GlowingButtonColorSet,
                                Latching = true,
                                OnStateChanged = HandleSnapStateChanged
                            }.AsFlexItem(size: 4f).Bind(ref _snapButton),
                            //curvature button
                            new ImageButton {
                                WithinLayoutIfDisabled = false,
                                Image = {
                                    Sprite = BundleLoader.Sprites.curvatureIcon,
                                    Material = BundleLoader.Materials.uiAdditiveGlowMaterial
                                },
                                Colors = UIStyle.GlowingButtonColorSet,
                                OnClick = HandleCurvatureButtonClicked
                            }.AsFlexItem(size: 4f).Bind(ref _curvatureButton),
                            //hide button
                            new ImageButton {
                                WithinLayoutIfDisabled = false,
                                Image = {
                                    Sprite = BundleLoader.Sprites.hideIcon,
                                    Material = BundleLoader.Materials.uiAdditiveGlowMaterial
                                },
                                Colors = UIStyle.GlowingButtonColorSet,
                                OnClick = HandleHideButtonClicked
                            }.AsFlexItem(size: 4f).Bind(ref _hideButton),
                        }
                    }.AsFlexGroup(
                        gap: 2f,
                        padding: new() { top = 1f, bottom = 1f, right = 2f, left = 2f }
                    ).AsBackground(
                        color: new(0.1f, 0.1f, 0.1f, 1f),
                        pixelsPerUnit: 7f
                    ).AsFlexItem(size: new() { y = 6f }),
                    //handle container
                    new Layout {
                        ContentTransform = {
                            localScale = new(7f, 3f, 3f)
                        }
                    }.AsFlexItem(size: new() { y = 6f }).Bind(ref _handleContainer)
                }
            }.AsFlexGroup(direction: FlexDirection.Column).Use();
        }

        protected override void OnInitialize() {
            _attachToHandHint = CreateHoverHint(_attachToHandButton);
            _pinHint = CreateHoverHint(_pinButton);
            _snapHint = CreateHoverHint(_snapButton);
            _curvatureHint = CreateHoverHint(_curvatureButton);
            _hideHint = CreateHoverHint(_hideButton);
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

        private static HoverHint CreateHoverHint(ImageButton button) {
            var hint = button.Content.AddComponent<HoverHint>();
            BeatSaberUtils.MenuContainer.Inject(hint);
            return hint;
        }

        protected override void OnDestroy() {
            //out of the controls hierarchy so may be destroyed after this object
            _resetController.ResetRequestedEvent -= HandleResetRequested;
        }

        #endregion

        #region Update

        protected override void OnUpdate() {
            if (!_isInitialized) return;
            var attachToHand = _settings!.AttachToHand;
            if (attachToHand == _lastAttachToHandState) return;
            _lastAttachToHandState = attachToHand;
            _attachToHandButton.Click(attachToHand, true, true);
            RefreshControlsVisibility();
        }

        #endregion

        #region Callbacks

        private void HandleAttachToHandStateChanged(bool state) {
            if (!_isInitialized) return;
            _settings!.AttachToHand = state;
            _lastAttachToHandState = state;
            RefreshControlsVisibility();
        }

        private void HandlePinStateChanged(bool state) {
            if (!_isInitialized) return;
            _settings!.Pinned = state;
            RefreshControlsVisibility();
        }

        private void HandleSnapStateChanged(bool state) {
            if (!_isInitialized) return;
            _floatingHandle.lookAtCenterPoint = state;
            _settings!.SnapEnabled = state;
            RefreshControlsVisibility();
        }

        private void HandleCurvatureButtonClicked() {
            if (!_isInitialized) return;
            _curvatureSettingsPresented = true;
            _curvatureSettings.Present();
            RefreshControlsVisibility();
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
            _curvatureSettingsPresented = false;
            _curvatureSettings.Hide();
            RefreshControlsVisibility();
        }

        private void HandleHideButtonClicked() {
            if (!_isInitialized) return;
            _mainUiHidden = !_mainUiHidden;
            
            RefreshControlsVisibility();
        }

        private void RefreshControlsVisibility() {
            var attachToHand = _settings!.AttachToHand;
            _mainUi!.Enabled = !_mainUiHidden;
            _attachToHandButton.Enabled = !_mainUiHidden;
            _pinButton.Enabled = !_mainUiHidden && !attachToHand;
            _snapButton.Enabled = !_mainUiHidden && !attachToHand && !_settings.Pinned;
            _curvatureButton.Enabled = !_mainUiHidden;
            _hideButton.Image.Sprite = _mainUiHidden ? BundleLoader.Sprites.showIcon : BundleLoader.Sprites.hideIcon;
            _attachToHandHint.text = attachToHand ? "Move UI to stage" : "Attach UI to hand";
            _pinHint.text = _settings.Pinned ? "Unpin UI" : "Pin UI";
            _snapHint.text = _settings.SnapEnabled ? "Disable auto-facing" : "Face center while moving";
            _curvatureHint.text = "Adjust UI curvature";
            _hideHint.text = _mainUiHidden ? "Show main UI" : "Hide main UI";
            RefreshHandleVisibility();
        }

        private void RefreshHandleVisibility() {
            var shouldShowHandle = !_mainUiHidden && !_settings!.Pinned && !_settings.AttachToHand && !_curvatureSettingsPresented;
            if (shouldShowHandle) {
                _floatingHandle.Present();
            } else {
                _floatingHandle.Hide();
            }
            _handleContainer.gameObject.SetActive(shouldShowHandle);
        }

        #endregion
    }
}