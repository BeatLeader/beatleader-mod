using System;
using BeatLeader.Models;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class TagPanel : ReactiveComponent {
        #region SetEditModeEnabled

        private bool _editModeEnabled;

        public void SetEditModeEnabled(bool enabled, bool immediate) {
            _editModeEnabled = enabled;
            _button.Latching = !enabled;
            _needToDisableDeleteButton = !enabled;
            if (!enabled) {
                SetTagSelected(_tagSelected, true);
            }
            if (immediate) {
                _deleteButton.Enabled = enabled;
            } else if (enabled) {
                _deleteButton.Enabled = true;
            }
        }

        #endregion

        #region SetTagPresented

        private bool _tagPresented;

        public void SetTagPresented(bool enabled, bool immediate) {
            _tagPresented = enabled;
            _needToInvokeFinishEvent = !immediate;
            _button.Interactable = enabled && Interactable;
            if (!immediate) return;
            //setting values if immediate
            _buttonTransform.localScale = Vector3.one;
            if (_replayTag != null) {
                StateAnimationFinishedEvent?.Invoke(_replayTag);
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

        public event Action<IReplayTag>? StateAnimationFinishedEvent;
        public event Action<IReplayTag>? DeleteButtonClickedEvent;
        public event Action<IReplayTag, bool>? TagStateChangedEvent;

        private IReplayTag? _replayTag;
        private bool _tagSelected;
        private bool _interactable = true;

        public void SetTagSelected(bool enabled, bool silent) {
            _button.Click(enabled, !silent);
        }

        public void SetTag(IReplayTag? tag, bool animated = false) {
            if (_replayTag != null) {
                _replayTag.TagUpdatedEvent -= HandleTagUpdated;
            } else {
                _setLastFrame = true;
            }
            _replayTag = tag;
            if (_replayTag != null) {
                _replayTag.TagUpdatedEvent += HandleTagUpdated;
            }
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
        private float _deleteButtonAppearSpeed = 15f;
        private float _buttonAppearSpeed = 15f;
        private float _slideSpeed = 10f;

        private bool _needToDisableDeleteButton = true;
        private bool _needToInvokeFinishEvent;
        private bool _setLastFrame;

        private Vector3 _previousWorldContainerPos;
        private Vector2 _previousContainerPos;

        //when another panel disappears this one will go to the pos of the previous panel smoothly
        private void RefreshPanelSlideAnimation() {
            if (_setLastFrame) {
                _setLastFrame = false;
                _previousContainerPos = ContentTransform.anchoredPosition;
                _buttonTransform.localPosition = Vector3.zero;
                return;
            }
            //if tag container got moved not globally
            var containerPosition = ContentTransform.anchoredPosition;
            if (containerPosition != _previousContainerPos) {
                _buttonTransform.position = _previousWorldContainerPos;
            }
            //animating
            _buttonTransform.localPosition = Vector3.Lerp(
                _buttonTransform.localPosition,
                Vector3.zero,
                Time.deltaTime * _slideSpeed
            );
            _previousContainerPos = ContentTransform.anchoredPosition;
            _previousWorldContainerPos = ContentTransform.position;
        }

        private void RefreshPanelAppearAnimation() {
            var buttonScale = _buttonTransform.localScale;
            _buttonTransform.localScale = Vector3.Lerp(
                buttonScale,
                _tagPresented ? Vector3.one : Vector3.zero,
                _buttonAppearSpeed * Time.deltaTime
            );
            //recalculating layout to smoothly move other tags
            if (_needToInvokeFinishEvent && buttonScale.x <= 0.0001f) {
                _needToInvokeFinishEvent = false;
                if (_replayTag != null) {
                    StateAnimationFinishedEvent?.Invoke(_replayTag);
                }
            }
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

        private void RefreshDeleteButtonAnimation() {
            var deleteButtonScale = _deleteButtonTransform.localScale;
            _deleteButtonTransform.localScale = Vector3.Lerp(
                deleteButtonScale,
                _editModeEnabled ? Vector3.one : Vector3.zero,
                _deleteButtonAppearSpeed * Time.deltaTime
            );
            //no matter what axis do we check since they all have the same values
            if (_needToDisableDeleteButton && deleteButtonScale.x < 0.01f) {
                _deleteButton.Enabled = false;
                _needToDisableDeleteButton = false;
            }
        }

        protected override void OnUpdate() {
            RefreshPanelAppearAnimation();
            //handling all animations if enabled
            if (!_tagPresented) return;
            RefreshPanelWiggleAnimation();
            RefreshDeleteButtonAnimation();
        }

        protected override void OnLateUpdate() {
            RefreshPanelSlideAnimation();
        }

        protected override void OnDisable() {
            if (!_needToInvokeFinishEvent) return;
            _needToInvokeFinishEvent = false;
            if (_replayTag != null) {
                StateAnimationFinishedEvent?.Invoke(_replayTag);
            }
        }

        #endregion

        #region Construct

        private RectTransform _deleteButtonTransform = null!;
        private ImageButton _deleteButton = null!;
        private RectTransform _buttonTransform = null!;
        private ImageButton _button = null!;
        private Label _label = null!;

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

        protected override GameObject Construct() {
            //wrapping into container to allow animations
            var content = new ImageButton {
                Image = {
                    Sprite = BundleLoader.Sprites.background,
                    PixelsPerUnit = 8f,
                    Material = GameResources.UINoGlowMaterial
                },
                Latching = true,
                OnClick = HandleTagButtonClicked,
                OnStateChanged = HandleTagButtonStateChanged,
                Children = {
                    //delete button
                    new ImageButton {
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
                            //minus icon
                            new Image {
                                Sprite = BundleLoader.Sprites.minusIcon
                            }.AsFlexItem(grow: 1f)
                        }
                    }.AsFlexGroup(padding: 1f).AsFlexItem(
                        size: 3.5f,
                        position: new() { top = -1.5f, right = -1.5f }
                    ).Bind(ref _deleteButton).Bind(ref _deleteButtonTransform),
                    //text
                    new Label {
                        FontSize = 3f
                    }.AsFlexItem(size: "auto").Bind(ref _label)
                }
            }.WithScaleAnimation(1f, 1.1f).AsFlexGroup(
                padding: new() { left = 1f, right = 1f },
                alignItems: Align.Center
            ).Bind(ref _button).Bind(ref _buttonTransform);

            //creating a wrapper which we will move
            var container = new GameObject("Wrapper").AddComponent<RectTransform>();
            container.SetParent(content.ContentTransform, false);
            container.WithRectExpand();
            _buttonTransform = container;

            //moving all objects into a wrapper
            foreach (var child in content.ContentTransform.GetChildren()) {
                child.SetParent(container, true);
            }
            return content.Use();
        }

        protected override void OnInitialize() {
            this.AsFlexItem(size: new() { y = 5f });
        }

        #endregion

        #region Callbacks

        private void HandleTagButtonStateChanged(bool state) {
            if (_editModeEnabled || _replayTag == null) return;
            _tagSelected = state;
            TagStateChangedEvent?.Invoke(_replayTag, state);
        }

        private void HandleTagButtonClicked() {
            if (!_editModeEnabled) return;
            HandleDeleteButtonClicked();
        }

        private void HandleDeleteButtonClicked() {
            if (_replayTag == null) return;
            DeleteButtonClickedEvent?.Invoke(_replayTag);
        }

        private void HandleTagUpdated() {
            RefreshVisuals();
        }

        #endregion
    }
}