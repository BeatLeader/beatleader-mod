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
        #region Public API

        public ReplayTag? Tag {
            get => _tag.Value;
            set {
                _tag.Value = value;

                SetBeingEdited(false, false);
                IsSelected = false;
            }
        }

        public bool Interactable {
            get;
            set {
                field = value;
                _tagButtonState.IsInteractable = value && IsPresent;
            }
        } = true;

        public bool IsSelected {
            get => _tagButtonState.IsActive;
            set => _tagButtonState.IsActive = value;
        }

        public bool IsPresent {
            get;
            set {
                field = value;

                var scale = value ? Vector3.one : Vector3.zero;
                if (Animated) {
                    _tagPanelScale.TargetValue = scale;
                } else {
                    _tagPanelScale.SetValueImmediate(scale);
                }
                
                _tagButtonState.IsInteractable = value && Interactable;
            }
        }

        public bool IsBeingEdited {
            get => _isBeingEdited;
            set => SetBeingEdited(value, Animated);
        }

        public bool Animated { get; set; } = true;

        public Action<ReplayTag>? OnDisappearAnimationFinished { get; set; }
        public Action<ReplayTag>? OnDeleteButtonClicked { get; set; }
        public Action<ReplayTag, bool>? OnTagStateChanged { get; set; }

        #endregion

        #region Edit Mode

        private static readonly Animation<IReactiveComponent> wiggleAnimation = new(
            AnimationTracks.Create<IReactiveComponent>(static (x, progress) => {
                var angle = Mathf.Sin(progress * 2f * Mathf.PI);
                x.ContentTransform.localEulerAngles = new(0f, 0f, angle);
            }),
            Duration: (2f * Mathf.PI / 40f).sec,
            Iterations: AnimationIteration.Infinite
        );

        private bool _isBeingEdited;

        private void SetBeingEdited(bool editing, bool animated) {
            _isBeingEdited = editing;

            var scale = editing ? Vector3.one : Vector3.zero;

            if (animated) {
                _deleteButtonScale.TargetValue = scale;
            } else {
                _deleteButtonScale.SetValueImmediate(scale);
            }

            if (editing) {
                if (!_wigglePlayer.IsPlaying) {
                    _wigglePlayer.Play(wiggleAnimation);
                }
            } else {
                _wigglePlayer.Complete();
            }
        }

        #endregion

        #region Construct

        private readonly AnimatedLayoutApplicator _slideApplicator = new() { Pos = 10.fact };

        private AnimationPlayer<IReactiveComponent> _wigglePlayer = null!;

        private AnimatedState<Vector3> _tagPanelScale = null!;
        private AnimatedState<Vector3> _deleteButtonScale = null!;
        private State<GraphicState> _tagButtonState = null!;
        private State<ReplayTag?> _tag = null!;

        protected override GameObject Construct() {
            _tagButtonState = Remember(GraphicState.NonInteractable);
            _tag = Remember<ReplayTag?>(null);

            _wigglePlayer = RememberAnimationPlayer<IReactiveComponent>(this);
            _deleteButtonScale = RememberAnimated(Vector3.zero, 200.ms, AnimationCurve.EaseInOut);
            _tagPanelScale = RememberAnimated(Vector3.zero, 300.ms, AnimationCurve.EaseInOut);

            _tagPanelScale.OnFinish = x => {
                if (x == Vector3.zero) {
                    OnDisappearAnimationFinished?.Invoke(_tag.Value!);
                }
            };
            
            _tagButtonState.AddCallback(
                this,
                static (x, state) => {
                    if (x.IsPresent) {
                        x._tagPanelScale.TargetValue = Vector3.one * (state is { IsInteractable: true, IsHovered: true } ? 1.1f : 1f);
                    }
                },
                lazy: true
            );

            return new Background {
                FlexController = {
                    Padding = new() { left = 1f, right = 1f },
                    AlignItems = Align.Center
                },

                ContentTransform = {
                    slocalScale = _tagPanelScale
                },

                Do = x => x.WithPointerEvents(
                    onEnter: _ => _tagButtonState.IsHovered = true,
                    onLeave: _ => _tagButtonState.IsHovered = false,
                    onDown: _ => HandleTagButtonClicked()
                ),

                LayoutApplicator = _slideApplicator,

                PixelsPerUnit = 8f,
                Sprite = BundleLoader.Sprites.background,
                Material = GameResources.UINoGlowMaterial,

                sColor = _tagButtonState.MapColorSet(_tag, x => {
                    var color = x?.Color ?? Color.red;

                    return new() {
                        Color = color.ColorWithAlpha(0.3f),
                        NotInteractableColor = color.ColorWithAlpha(0.3f),
                        HoveredColor = color.ColorWithAlpha(0.7f),
                        ActiveColor = color.ColorWithAlpha(1f),
                    };
                }).In(),

                Children = {
                    new Background {
                        Name = "Delete Button",
                        
                        FlexController = {
                            Padding = 1f
                        },

                        FlexItem = {
                            Size = 3.5f,
                            Position = new() { top = -1.5f, right = -1.5f },
                            PositionType = PositionType.Absolute
                        },

                        ContentTransform = {
                            slocalScale = _deleteButtonScale
                        },

                        Do = x => x.WithPointerEvents(
                            onDown: _ => {
                                global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                                HandleDeleteButtonClicked();
                            }
                        ),

                        Sprite = BundleLoader.Sprites.background,
                        PixelsPerUnit = 1f,
                        
                        sColor = _tagButtonState.MapColorSet(new() {
                            Color = Color.red.ColorWithAlpha(0.7f),
                            HoveredColor = Color.red
                        }),

                        sEnabled = _deleteButtonScale.Map(static x => x != Vector3.zero),

                        Children = {
                            // Minus icon
                            new Image {
                                FlexItem = {
                                    FlexGrow = 1f
                                },

                                Sprite = BundleLoader.Sprites.minusIcon
                            }
                        }
                    },
                    
                    new Label {
                        FlexItem = {
                            Size = YogaVector.Auto
                        },

                        FontSize = 3f,
                        Alignment = TextAlignmentOptions.Capline,
                        sText = _tag.Map(static x => x?.Name ?? "Tag")
                    }
                }
            }.Use();
        }

        protected override void OnInitialize() {
            ReplayMetadataManager.TagUpdatedEvent += HandleTagUpdated;
        }

        protected override void OnDestroy() {
            ReplayMetadataManager.TagUpdatedEvent -= HandleTagUpdated;
        }

        protected override void OnEnable() {
            _slideApplicator.Complete();
        }

        #endregion

        #region Callbacks

        private void HandleTagButtonClicked() {
            if (!_tagButtonState.IsInteractable) {
                return;
            }

            global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();

            // In edit mode the whole tag acts as the delete button
            if (_isBeingEdited) {
                HandleDeleteButtonClicked();
                return;
            }

            IsSelected = !IsSelected;
            OnTagStateChanged?.Invoke(_tag.Value!, IsSelected);
        }

        private void HandleDeleteButtonClicked() {
            OnDeleteButtonClicked?.Invoke(_tag.Value!);
        }

        private void HandleTagUpdated(ReplayTag tag) {
            // The tag is edited in place, so the state has to be told it changed
            if (tag == _tag.Value) {
                _tag.EnforceInvalidation();
            }
        }

        #endregion
    }
}