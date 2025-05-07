using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
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
            _button.Latching = !enabled;
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
                _deleteButtonScale.Value = targetValue;
            }
        }

        #endregion

        #region SetTagPresented

        private bool _tagPresented;

        public void SetTagPresented(bool enabled, bool immediate) {
            _tagPresented = enabled;
            _button.Interactable = enabled && Interactable;
            var targetValue = enabled ? Vector3.one : Vector3.zero;

            if (immediate) {
                _panelScale.SetValueImmediate(targetValue);

                if (_replayTag != null) {
                    DisappearAnimationFinishedEvent?.Invoke(_replayTag);
                }
            } else {
                _panelScale.Value = targetValue;
            }
        }

        #endregion

        #region Setup

        public bool Interactable {
            get => _interactable;
            set {
                _interactable = value;
                _button.Interactable = value && _tagPresented;
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
            _button.Click(enabled, !silent);
            _tagSelected = enabled;
        }

        public void SetTag(ReplayTag tag, bool animated = false) {
            if (_replayTag == null) {
                _setInitialValues = true;
            }

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

        private bool _setInitialValues;

        private Vector3 _previousWorldContainerPos;
        private Vector2 _previousContainerPos;

        private void RefreshPanelSlideAnimation() {
            if (_setInitialValues) {
                _setInitialValues = false;
                _buttonTransform.localPosition = Vector3.zero;
                _previousContainerPos = ContentTransform.anchoredPosition;
                _previousWorldContainerPos = ContentTransform.position;
                return;
            }

            // If tag container got moved not globally
            var containerPosition = ContentTransform.anchoredPosition;
            if (containerPosition != _previousContainerPos) {
                _buttonTransform.position = _previousWorldContainerPos;
            }

            // Animating
            _buttonTransform.localPosition = Vector3.Lerp(
                _buttonTransform.localPosition,
                Vector3.zero,
                Time.deltaTime * _slideSpeed
            );
            _previousContainerPos = ContentTransform.anchoredPosition;
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

        #endregion

        #region Visuals

        public float FixedHeight {
            get => _fixedHeight;
            set {
                _fixedHeight = value;
                RefreshContainerSize();
            }
        }

        private float _fixedHeight = 5f;

        private void RefreshVisuals() {
            _label.Text = _replayTag?.Name ?? "Tag";
            var color = _replayTag?.Color ?? Color.red;

            _button.Colors = new SimpleColorSet {
                Color = color.ColorWithAlpha(0.3f),
                NotInteractableColor = color.ColorWithAlpha(0.3f),
                HoveredColor = color.ColorWithAlpha(0.7f),
                ActiveColor = color.ColorWithAlpha(1f)
            };
        }

        private void RefreshContainerSize() {
            // Content is disconnected from the layout hierarchy, so we manually maintain the size
            var size = _button.ContentTransform.rect.size;
            _containerModifier.Size = new() { x = size.x, y = FixedHeight };
        }

        protected override void OnRectDimensionsChanged() {
            RefreshContainerSize();
        }

        #endregion

        #region Construct

        private AnimatedValue<Vector3> _deleteButtonScale = null!;
        private AnimatedValue<Vector3> _panelScale = null!;

        private YogaModifier _containerModifier = null!;
        private ImageButton _deleteButton = null!;
        private RectTransform _buttonTransform = null!;
        private ImageButton _button = null!;
        private Label _label = null!;

        protected override GameObject Construct() {
            var container = new ReactiveComponent().AsFlexItem(out _containerModifier);

            _deleteButtonScale = RememberAnimated(Vector3.zero, 200.ms(), AnimationCurve.EaseInOut);
            _deleteButtonScale.OnFinish = x => {
                if (x.Value == Vector3.zero) {
                    _deleteButton.Enabled = false;
                }
            };

            _panelScale = RememberAnimated(Vector3.one, 300.ms(), AnimationCurve.EaseInOut);
            _panelScale.OnFinish = x => {
                if (x.Value == Vector3.zero) {
                    DisappearAnimationFinishedEvent?.Invoke(_replayTag!);
                }
            };

            new BackgroundButton {
                    Image = {
                        Sprite = BundleLoader.Sprites.background,
                        PixelsPerUnit = 8f,
                        Material = GameResources.UINoGlowMaterial
                    },

                    Latching = true,
                    OnClick = HandleTagButtonClicked,
                    OnStateChanged = HandleTagButtonStateChanged,

                    Children = {
                        // Delete button
                        new BackgroundButton {
                                Enabled = false,
                                OnClick = HandleDeleteButtonClicked,
                                ContentTransform = {
                                    localScale = Vector3.zero
                                },
                                Image = {
                                    Sprite = BundleLoader.Sprites.background,
                                    PixelsPerUnit = 1f
                                },
                                Colors = new SimpleColorSet {
                                    Color = Color.red.ColorWithAlpha(0.7f),
                                    HoveredColor = Color.red
                                },
                                Children = {
                                    // Minus icon
                                    new Image {
                                        Sprite = BundleLoader.Sprites.minusIcon
                                    }.AsFlexItem(flexGrow: 1f)
                                }
                            }
                            .AsFlexGroup(padding: 1f)
                            .AsFlexItem(
                                size: 3.5f,
                                position: new() { top = -1.5f, right = -1.5f }
                            )
                            .Animate(_deleteButtonScale, x => x.ContentTransform.localScale)
                            .Bind(ref _deleteButton),

                        // Text
                        new Label {
                            FontSize = 3f,
                            Alignment = TextAlignmentOptions.Capline
                        }.AsFlexItem(size: "auto").Bind(ref _label)
                    }
                }
                .AsFlexGroup(
                    padding: new() { left = 1f, right = 1f },
                    alignItems: Align.Center,
                    constrainHorizontal: false
                )
                .Animate(_panelScale, x => x.ContentTransform.localScale)
                .WithScaleAnimation(1f, 1.1f)
                .Bind(ref _button)
                .Bind(ref _buttonTransform)
                .Use(container.ContentTransform);

            // Helper to listen for content updates.
            new LayoutWrapper(() => _button.Content) {
                OnLayoutRecalculated = RefreshContainerSize
            };

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

        private void HandleTagButtonClicked() {
            if (_editModeEnabled) {
                HandleDeleteButtonClicked();
            }
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