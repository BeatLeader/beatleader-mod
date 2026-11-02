using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.Hub {
    internal class TagPanel : ReactiveComponent {
        #region SetEditModeEnabled

        private bool _editModeEnabled;

        public void SetEditModeEnabled(bool enabled, bool immediate) {
            _editModeEnabled = enabled;
            var targetValue = enabled ? Vector3.one : Vector3.zero;

            if (!enabled) {
                SetTagSelected(_tagSelected, true);
            }

            if (immediate) {
                _deleteButton.Enabled = enabled;
                _deleteButtonScale.SetValueImmediate(targetValue);
            } else {
                if (enabled) {
                    _deleteButton.Enabled = true;
                }
                _deleteButtonScale.TargetValue = targetValue;
            }
        }

        #endregion

        #region SetTagPresented

        private bool _tagPresented;

        public void SetTagPresented(bool enabled, bool immediate) {
            _tagPresented = enabled;
            _buttonState.IsInteractable = enabled && Interactable;
            var targetValue = enabled ? Vector3.one : Vector3.zero;

            if (immediate) {
                _panelScale.SetValueImmediate(targetValue);

                if (_replayTag != null) {
                    DisappearAnimationFinishedEvent?.Invoke(_replayTag);
                }
            } else {
                _panelScale.TargetValue = targetValue;
            }
        }

        #endregion

        #region Setup

        public bool Interactable {
            get => _interactable;
            set {
                _interactable = value;
                _buttonState.IsInteractable = value && _tagPresented;
            }
        }

        public bool AnimateMovement = true;

        public event Action<ReplayTag>? DisappearAnimationFinishedEvent;
        public event Action<ReplayTag>? DeleteButtonClickedEvent;
        public event Action<ReplayTag, bool>? TagStateChangedEvent;

        private ReplayTag? _replayTag;
        private bool _tagSelected;
        private bool _interactable = true;

        public void SetTagSelected(bool enabled, bool silent) {
            _buttonState.IsActive = enabled;
            if (!silent) {
                HandleTagButtonStateChanged(enabled);
            }
            _tagSelected = enabled;
        }

        public void SetTag(ReplayTag tag, bool animated = false) {
            _replayTag = tag;

            SetEditModeEnabled(false, true);
            SetTagSelected(false, true);
            SetTagPresented(true, !animated);
            RefreshVisuals();
        }

        #endregion

        #region Animation

        private float _wiggleAmplitude = 1f;
        private float _wiggleSpeed = 40f;
        private float _wiggleReturnSpeed = 10f;
        private float _slideSpeed = 10f;

        private bool _needToResetPos;
        private Vector2 _prevContainerSize;

        private Vector3 _previousWorldContainerPos;
        private Vector2 _previousContainerPos;

        private void StartPanelSlideAnimation() {
            // If tag container got moved not globally
            var containerPosition = ContentTransform.anchoredPosition;
            if (containerPosition != _previousContainerPos) {
                _buttonTransform.position += _previousWorldContainerPos - ContentTransform.position;
            }
            ResetInitialPosition();
        }

        private void RefreshPanelSlideAnimation() {
            // Animating
            _buttonTransform.localPosition = Vector3.Lerp(
                _buttonTransform.localPosition,
                Vector3.zero,
                Time.deltaTime * _slideSpeed
            );
            _previousWorldContainerPos = ContentTransform.position;
        }

        private void RefreshPanelWiggleAnimation() {
            if (_editModeEnabled) {
                var angle = Mathf.Sin(_wiggleSpeed * Time.time) * _wiggleAmplitude;
                _buttonTransform.localEulerAngles = new(0f, 0f, angle);
            } else {
                _buttonTransform.localRotation = Quaternion.Lerp(
                    ContentTransform.localRotation,
                    Quaternion.identity,
                    Time.deltaTime * _wiggleReturnSpeed
                );
            }
        }

        private void ResetInitialPosition() {
            _previousContainerPos = ContentTransform.anchoredPosition;
            _previousWorldContainerPos = ContentTransform.position;
        }

        protected override void OnUpdate() {
            if (_tagPresented) {
                RefreshPanelWiggleAnimation();
            }
        }

        protected override void OnLateUpdate() {
            if (AnimateMovement) {
                RefreshPanelSlideAnimation();
            }
        }

        protected override void OnEnable() {
            _needToResetPos = true;
        }

        protected override void OnLayoutApply() {
            if (ContentTransform.parent is { } parent) {
                var size = ((RectTransform)parent).rect.size;

                if (_prevContainerSize != size) {
                    _needToResetPos = true;
                    _prevContainerSize = size;
                }
            }

            if (_needToResetPos) {
                _needToResetPos = false;
                ResetInitialPosition();
            } else if (AnimateMovement) {
                StartPanelSlideAnimation();
            }
        }

        #endregion

        #region Visuals

        public float FixedHeight = 5f;

        private void RefreshVisuals() {
            _label.Text = _replayTag?.Name ?? "Tag";
            var color = _replayTag?.Color ?? Color.red;

            _buttonColors.Value = new ColorSet {
                Color = color.ColorWithAlpha(0.3f),
                NotInteractableColor = color.ColorWithAlpha(0.3f),
                HoveredColor = color.ColorWithAlpha(0.7f),
                ActiveColor = color.ColorWithAlpha(1f)
            };
        }

        public void RefreshContainerSize(bool recalculate) {
            if (recalculate) {
                _button.RecalculateLayout();
            }

            // Content is disconnected from the layout hierarchy, so we manually maintain the size
            var size = _button.ContentTransform.rect.size;
            _containerModifier.Size = new() { x = size.x, y = FixedHeight };
        }

        protected override void OnStart() {
            RefreshContainerSize(true);
        }

        #endregion

        #region Construct

        private static readonly ColorSet deleteButtonColors = new() {
            Color = Color.red.ColorWithAlpha(0.7f),
            HoveredColor = Color.red
        };

        private AnimatedState<Vector3> _deleteButtonScale = null!;
        private AnimatedState<Vector3> _panelScale = null!;
        private State<GraphicState> _buttonState = null!;
        private State<ColorSet> _buttonColors = null!;

        private YogaModifier _containerModifier = null!;
        private Background _deleteButton = null!;
        private RectTransform _buttonTransform = null!;
        private Background _button = null!;
        private Label _label = null!;

        protected override GameObject Construct() {
            var container = new ReactiveComponent().AsFlexItem(out _containerModifier);

            _deleteButtonScale = RememberAnimated(Vector3.zero, 200.ms, AnimationCurve.EaseInOut);
            _deleteButtonScale.OnFinish = x => {
                if (x == Vector3.zero) {
                    _deleteButton.Enabled = false;
                }
            };

            _panelScale = RememberAnimated(Vector3.one, 300.ms, AnimationCurve.EaseInOut);
            _panelScale.OnFinish = x => {
                if (x == Vector3.zero) {
                    DisappearAnimationFinishedEvent?.Invoke(_replayTag!);
                }
            };

            _buttonState = Remember(GraphicState.None);
            _buttonColors = Remember(new ColorSet());
            var deleteButtonState = Remember(GraphicState.None);

            new Background {
                    Sprite = BundleLoader.Sprites.background,
                    PixelsPerUnit = 8f,
                    Material = GameResources.UINoGlowMaterial,
                    sColor = _buttonState.MapColorSet(_buttonColors, static x => x).In(),

                    Children = {
                        // Delete button
                        new Background {
                                Enabled = false,
                                ContentTransform = {
                                    localScale = Vector3.zero
                                },
                                Sprite = BundleLoader.Sprites.background,
                                PixelsPerUnit = 1f,
                                sColor = deleteButtonState.MapColorSet(deleteButtonColors),
                                Children = {
                                    // Minus icon
                                    new Image {
                                        Sprite = BundleLoader.Sprites.minusIcon
                                    }.AsFlexItem(flexGrow: 1f)
                                }
                            }
                            .WithPointerEvents(
                                onEnter: _ => deleteButtonState.IsHovered = true,
                                onLeave: _ => deleteButtonState.IsHovered = false,
                                onDown: _ => {
                                    global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                                    HandleDeleteButtonClicked();
                                }
                            )
                            .AsFlexGroup(padding: 1f)
                            .AsFlexItem(
                                size: 3.5f,
                                position: new() { top = -1.5f, right = -1.5f }
                            )
                            .On(_deleteButtonScale, static (x, y) => x.ContentTransform.localScale = y, lazy: true)
                            .Bind(ref _deleteButton),

                        // Text
                        new Label {
                            FontSize = 3f,
                            Alignment = TextAlignmentOptions.Capline
                        }.AsFlexItem(size: "auto").Bind(ref _label)
                    }
                }
                .WithPointerEvents(
                    onEnter: _ => HandleTagButtonHoverChanged(true),
                    onLeave: _ => HandleTagButtonHoverChanged(false),
                    onDown: _ => HandleTagButtonClicked()
                )
                .AsFlexGroup(
                    padding: new() { left = 1f, right = 1f },
                    alignItems: Align.Center,
                    constrainHorizontal: false,
                    constrainVertical: false
                )
                .On(_panelScale, static (x, y) => x.ContentTransform.localScale = y, lazy: true)
                .Bind(ref _button)
                .Bind(ref _buttonTransform)
                .Use(container.ContentTransform);

            _button.LayoutAppliedEvent += _ => RefreshContainerSize(false);

            return container.Content;
        }

        protected override void OnInitialize() {
            this.AsFlexItem(size: new() { y = 5f });
            ReplayMetadataManager.TagUpdatedEvent += HandleTagUpdated;
        }

        protected override void OnDestroy() {
            ReplayMetadataManager.TagUpdatedEvent -= HandleTagUpdated;
        }

        #endregion

        #region Callbacks

        private void HandleTagButtonStateChanged(bool state) {
            if (_editModeEnabled) {
                return;
            }

            _tagSelected = state;
            TagStateChangedEvent?.Invoke(_replayTag!, state);
        }

        private void HandleTagButtonHoverChanged(bool hovered) {
            if (hovered && !_buttonState.IsInteractable) {
                return;
            }

            _buttonState.IsHovered = hovered;
            if (_tagPresented) {
                _panelScale.TargetValue = (hovered ? 1.1f : 1f) * Vector3.one;
            }
        }

        private void HandleTagButtonClicked() {
            if (!_buttonState.IsInteractable) {
                return;
            }

            global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();

            if (_editModeEnabled) {
                HandleDeleteButtonClicked();
                return;
            }

            _buttonState.IsActive = !_buttonState.IsActive;
            HandleTagButtonStateChanged(_buttonState.IsActive);
        }

        private void HandleDeleteButtonClicked() {
            DeleteButtonClickedEvent?.Invoke(_replayTag!);
        }

        private void HandleTagUpdated(ReplayTag tag) {
            if (tag == _replayTag) {
                RefreshVisuals();
            }
        }

        #endregion
    }
}