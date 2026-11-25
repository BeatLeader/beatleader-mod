using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using ScrollArea = Reactive.BeatSaber.Components.ScrollArea;

namespace BeatLeader.UI.Hub {
    internal class TagSelector : ReactiveComponent {
        #region Setup

        public IEnumerable<ReplayTag> SelectedTags {
            get => _selectedTags;
            set {
                _selectedTags.Clear();

                foreach (var (tag, panel) in _tagsPool.SpawnedComponents) {
                    panel.IsSelected = value.Contains(tag);

                    if (panel.IsSelected) {
                        _selectedTags.Add(tag);
                    }
                }
            }
        }

        public Action<ReplayTag>? OnSelectedTagAdded { get; set; }
        public Action<ReplayTag>? OnSelectedTagRemoved { get; set; }

        private readonly HashSet<ReplayTag> _selectedTags = new();

        protected override void OnInitialize() {
            ReplayMetadataManager.TagCreatedEvent += SpawnTag;
            ReplayMetadataManager.TagDeletedEvent += DespawnTag;

            foreach (var tag in ReplayMetadataManager.Tags.Values) {
                SpawnTag(tag);
            }
        }

        protected override void OnDestroy() {
            ReplayMetadataManager.TagCreatedEvent -= SpawnTag;
            ReplayMetadataManager.TagDeletedEvent -= DespawnTag;
        }

        #endregion

        #region Tags

        private readonly ReactivePool<ReplayTag, TagPanel> _tagsPool = new() { DetachOnDespawn = false };

        private void SpawnTag(ReplayTag tag) {
            var panel = _tagsPool.Spawn(tag);

            panel.Tag = tag;
            panel.IsPresent = true;

            panel.OnTagStateChanged = HandleTagStateChanged;
            panel.OnDeleteButtonClicked = x => _deletingTag.Value = x;
            panel.OnDisappearAnimationFinished = null;

            _tagsContainer.Children.Add(panel);
        }

        private void DespawnTag(ReplayTag tag) {
            if (!_tagsPool.SpawnedComponents.TryGetValue(tag, out var panel)) {
                return;
            }

            panel.OnTagStateChanged = null;
            panel.OnDeleteButtonClicked = null;
            panel.OnDisappearAnimationFinished = HandleTagDisappearAnimationFinished;
            panel.IsPresent = false;
        }

        private void SetEditModeEnabled(bool enabled) {
            _createButtonState.IsInteractable = !enabled;
            foreach (var (_, tag) in _tagsPool.SpawnedComponents) {
                tag.IsBeingEdited = enabled;
            }
        }

        #endregion

        #region Construct

        private Layout _tagsContainer = null!;
        private State<GraphicState> _createButtonState = null!;
        private State<ReplayTag?> _deletingTag = null!;

        protected override GameObject Construct() {
            var editButtonState = Remember(GraphicState.None);
            var creationOpened = Remember(false);
            var dialogAnchor = Ref<RectTransform>();

            _createButtonState = Remember(GraphicState.None);
            _deletingTag = Remember<ReplayTag?>(null);

            return new Background {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    Gap = 1f,
                    ConstrainHorizontal = false,
                    ConstrainVertical = false
                },

                Children = {
                    new TagCreationDialog {
                        PlacementData = new() { Placement = RelativePlacement.Center, Offset = new(0f, 13f) },
                        PlacementAnchor = dialogAnchor,

                        sIsPushed = creationOpened,
                        OnPopped = () => creationOpened.Value = false,
                        OnClickOutside = () => creationOpened.Value = false,
                    },

                    new TagDeletionDialog {
                        PlacementData = new() { Placement = RelativePlacement.Center, Offset = new(0f, 0.5f) },
                        PlacementAnchor = dialogAnchor,

                        sTag = _deletingTag,

                        sIsPushed = _deletingTag.Map(static x => x != null),
                        OnPopped = () => _deletingTag.Value = null
                    },

                    new ScrollArea {
                        FlexItem = {
                            FlexGrow = 1f,
                            Margin = new() { top = 1f }
                        },

                        ScrollContext = new ScrollContext(),
                        LineSize = 7f,

                        ScrollContent = new Layout {
                            FlexController = {
                                ConstrainVertical = false,
                                Padding = 2f,
                                FlexWrap = Wrap.Wrap,
                                AlignItems = Align.FlexStart,
                                JustifyContent = Justify.SpaceAround,
                                Gap = new() { x = 1.5f, y = 3f }
                            },

                            FlexItem = {
                                Size = new() { y = YogaValue.Auto }
                            }
                        }.Bind(ref _tagsContainer)
                    },

                    new Background {
                        Name = "ActionButtons",

                        FlexController = {
                            Padding = 1f,
                            JustifyContent = Justify.SpaceAround
                        },

                        FlexItem = {
                            Size = new() { y = 6f }
                        },

                        Sprite = BundleLoader.Sprites.backgroundBottom,
                        Children = {
                            // Delete button
                            CreateGlowingButton(
                                BundleLoader.Sprites.trashIcon,
                                editButtonState,
                                () => {
                                    editButtonState.IsActive = !editButtonState.IsActive;
                                    SetEditModeEnabled(editButtonState.IsActive);
                                }
                            ),

                            // Create button
                            CreateGlowingButton(
                                BundleLoader.Sprites.plusIcon,
                                _createButtonState,
                                () => creationOpened.Value = true
                            )
                        }
                    }.AsBlurBackground(color: Color.white.ColorWithAlpha(0.9f))
                }
            }.AsBlurBackground().Bind(dialogAnchor).Use();
        }

        [StateGen]
        private static Image CreateGlowingButton(Sprite sprite, State<GraphicState> graphicState, Action onClick) {
            return new Image {
                FlexItem = {
                    Size = 4.pt
                },
                
                Sprite = sprite,
                Material = BundleLoader.Materials.uiAdditiveGlowMaterial,
                sColor = graphicState.MapColorSet(UIStyle.GlowingButtonColorSet),
            }.WithPointerEvents(
                onEnter: _ => graphicState.IsHovered = true,
                onLeave: _ => graphicState.IsHovered = false,
                onDown: _ => {
                    if (!graphicState.IsInteractable) return;
                    global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                    onClick();
                }
            );
        }

        #endregion

        #region Callbacks

        private void HandleTagDisappearAnimationFinished(ReplayTag tag) {
            if (_selectedTags.Contains(tag)) {
                HandleTagStateChanged(tag, false);
            }

            var panel = _tagsPool.SpawnedComponents[tag];
            panel.OnDisappearAnimationFinished = null;
            _tagsPool.Despawn(panel);
        }

        private void HandleTagStateChanged(ReplayTag tag, bool state) {
            if (state) {
                _selectedTags.Add(tag);
                OnSelectedTagAdded?.Invoke(tag);
            } else {
                _selectedTags.Remove(tag);
                OnSelectedTagRemoved?.Invoke(tag);
            }
        }

        #endregion
    }
}