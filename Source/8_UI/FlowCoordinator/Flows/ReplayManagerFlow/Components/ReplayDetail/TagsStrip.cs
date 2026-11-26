using System;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.Hub {
    internal class TagsStrip : ReactiveComponent {
        #region Public API

        public ReplayMetadata? Metadata {
            get;
            set {
                var shouldUpdate = field == null || value == null || !field.Tags.SequenceEqual(value.Tags);

                if (field != null) {
                    field.TagAddedEvent -= HandleTagAddedExternal;
                    field.TagRemovedEvent -= HandleTagRemovedExternal;
                }

                field = value;

                if (field != null) {
                    field.TagAddedEvent += HandleTagAddedExternal;
                    field.TagRemovedEvent += HandleTagRemovedExternal;
                }

                // The tags are swapped once the strip is folded
                if (shouldUpdate) {
                    FoldReloadTags();
                }
            }
        }

        public bool ShowEditButton {
            get => _showEditButton.Value;
            set => _showEditButton.Value = value;
        }

        public bool EditorOpened {
            get => _editorOpened.Value;
            set => _editorOpened.Value = value;
        }

        public Action? OnEditClicked { get; set; }

        #endregion

        #region Tags

        private const int MaxShownTags = 3;

        private readonly ReactivePool<ReplayTag, TagPanel> _tagsPool = new() { DetachOnDespawn = false };

        private void FoldReloadTags() {
            _fold.TargetValue = 0f;
        }

        private void ReloadTags() {
            _tagsPool.DespawnAll();

            foreach (var tag in Metadata!.Tags.Take(MaxShownTags)) {
                SpawnTag(tag, false);
            }

            _tagsCount.Value = Metadata.Tags.Count;
        }

        private void SpawnTag(ReplayTag tag, bool animated) {
            if (_tagsPool.SpawnedComponents.TryGetValue(tag, out var panel)) {
                panel.OnDisappearAnimationFinished = null;
            } else {
                panel = _tagsPool.Spawn(tag);
            }

            panel.Interactable = false;
            panel.Animated = animated;
            panel.Tag = tag;
            panel.IsPresent = true;

            _tagsContainer.Children.Add(panel);
        }

        private void DespawnTag(ReplayTag tag, bool animated) {
            if (!_tagsPool.SpawnedComponents.TryGetValue(tag, out var panel)) {
                return;
            }

            panel.OnDisappearAnimationFinished = HandleTagDisappearAnimationFinished;
            panel.Animated = animated;
            panel.IsPresent = false;
        }

        #endregion

        #region Construct

        private State<bool> _showEditButton = null!;
        private State<int> _tagsCount = null!;
        private AnimatedState<float> _fold = null!;
        private State<bool> _editorOpened = null!;
        private Layout _tagsContainer = null!;

        protected override GameObject Construct() {
            var editButtonState = Remember(GraphicState.None);

            _showEditButton = Remember(true);
            _tagsCount = Remember(0);
            _editorOpened = Remember(false);

            var editArrowAngle = RememberAnimated(_editorOpened.Map(static x => x ? 180f : 0f), 150.ms, AnimationCurve.EaseInOut);

            _fold = RememberAnimated(1f, 120.ms, AnimationCurve.EaseInOut);
            _fold.OnFinish = x => {
                if (x == 0f) {
                    ReloadTags();
                    _fold.TargetValue = 1f;
                }
            };

            var labelShown = _tagsCount.Map(static x => x is 0 or > MaxShownTags);

            return new Layout {
                FlexController = {
                    Gap = 0.5f
                },

                ContentTransform = {
                    slocalScale = _fold.Map(static x => new Vector3(1f, x, 1f))
                },

                Children = {
                    new Background {
                        FlexController = {
                            JustifyContent = Justify.SpaceAround
                        },

                        FlexItem = {
                            FlexGrow = 1f
                        },

                        sSprite = _showEditButton.Map(static x => x ? BundleLoader.Sprites.backgroundLeft : BundleLoader.Sprites.background),
                        sColor = _fold.Map(static x => Color.white.ColorWithAlpha(x)),

                        Children = {
                            new Layout {
                                Name = "TagsContainer",

                                FlexController = {
                                    Gap = 1f,
                                    AlignItems = Align.Center,
                                    JustifyContent = Justify.SpaceAround,
                                    Padding = new() { left = 1f }
                                },

                                FlexItem = {
                                    FlexGrow = 1f
                                },

                                sEnabled = _tagsCount.Map(static x => x > 0)
                            }.Bind(ref _tagsContainer),

                            new Label {
                                FlexItem = {
                                    Position = new() { top = 0.pt, right = -100.pct },
                                    Margin = new() { left = 1f, right = 1f },
                                    
                                    sPositionType = labelShown.Map(x => x ? PositionType.Static : PositionType.Absolute)
                                },

                                LayoutApplicator = LayoutApplicator.Animated(300.ms, AnimationCurve.EaseOutQuint),

                                Color = BeatSaberStyle.SecondaryTextColor,
                                FontSize = 3.5f,
                                Alignment = TextAlignmentOptions.Capline,

                                sText = _tagsCount.Map(static x => x == 0 ? "No tags" : $"And {x - MaxShownTags} more"),
                            }
                        }
                    }.AsBlurBackground().AsRectMask(),

                    new Background {
                        Name = "EditButton",

                        FlexController = {
                            Padding = 1f,
                            AlignItems = Align.Center,
                            JustifyContent = Justify.Center
                        },

                        Sprite = BundleLoader.Sprites.backgroundRight,
                        sEnabled = _showEditButton,

                        Children = {
                            new Image {
                                FlexItem = {
                                    Size = 2.5f
                                },

                                ContentTransform = {
                                    slocalEulerAngles = editArrowAngle.Map(static x => new Vector3(0f, 0f, x))
                                },

                                Sprite = global::Reactive.BeatSaber.GameResources.ArrowIcon,
                                PreserveAspect = true,
                                Material = BundleLoader.Materials.uiAdditiveGlowMaterial,
                                sColor = editButtonState.MapColorSet(UIStyle.GlowingButtonColorSet),

                                Do = x => x.WithPointerEvents(
                                    onEnter: _ => editButtonState.IsHovered = true,
                                    onLeave: _ => editButtonState.IsHovered = false,
                                    onDown: _ => {
                                        global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                                        OnEditClicked?.Invoke();
                                    }
                                )
                            }
                        }
                    }.AsBlurBackground()
                }
            }.WithAlpha(_fold).Use();
        }

        protected override void OnDestroy() {
            if (Metadata != null) {
                Metadata.TagAddedEvent -= HandleTagAddedExternal;
                Metadata.TagRemovedEvent -= HandleTagRemovedExternal;
            }
        }

        #endregion

        #region Callbacks

        private void HandleTagDisappearAnimationFinished(ReplayTag tag) {
            var panel = _tagsPool.SpawnedComponents[tag];
            panel.OnDisappearAnimationFinished = null;
            _tagsPool.Despawn(panel);

            // Spawning another tag if needed
            var spawned = _tagsPool.SpawnedComponents.Count;
            if (Metadata != null && spawned < MaxShownTags && Metadata.Tags.Count > spawned) {
                tag = Metadata.Tags.First(x => !_tagsPool.SpawnedComponents.ContainsKey(x));
                SpawnTag(tag, true);
            }
        }

        private void HandleTagAddedExternal(ReplayTag tag) {
            if (_tagsPool.SpawnedComponents.Count < MaxShownTags || _tagsPool.SpawnedComponents.ContainsKey(tag)) {
                SpawnTag(tag, true);
            }
            _tagsCount.Value = Metadata!.Tags.Count;
        }

        private void HandleTagRemovedExternal(ReplayTag tag) {
            DespawnTag(tag, true);
            _tagsCount.Value = Metadata!.Tags.Count;
        }

        #endregion
    }
}