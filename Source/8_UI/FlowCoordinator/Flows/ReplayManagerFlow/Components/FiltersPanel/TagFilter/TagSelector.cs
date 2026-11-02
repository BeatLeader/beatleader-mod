using System;
using System.Collections.Generic;
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

        public IReadOnlyCollection<ReplayTag> SelectedTags => _selectedTags;

        public event Action<ReplayTag>? SelectedTagAddedEvent;
        public event Action<ReplayTag>? SelectedTagRemovedEvent;

        private readonly HashSet<ReplayTag> _selectedTags = new();

        public void SelectTags(ICollection<ReplayTag> tags) {
            _selectedTags.Clear();

            foreach (var (tag, panel) in _tagsPool.SpawnedComponents) {
                var selected = tags.Contains(tag);
                panel.SetTagSelected(selected, true);

                if (selected) {
                    _selectedTags.Add(tag);
                }
            }
        }

        public void ClearSelectedTags() {
            foreach (var tag in _selectedTags) {
                var panel = _tagsPool.SpawnedComponents[tag];
                panel.SetTagSelected(false, true);
                SelectedTagRemovedEvent?.Invoke(tag);
            }
            _selectedTags.Clear();
        }

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

            panel.SetTag(tag);

            panel.SetTagPresented(false, true);
            panel.SetTagPresented(true, false);

            panel.TagStateChangedEvent += HandleTagStateChanged;
            panel.DeleteButtonClickedEvent += HandleTagDeleteButtonClicked;

            _tagsContainer.Children.Add(panel);
        }

        private void DespawnTag(ReplayTag tag) {
            if (!_tagsPool.SpawnedComponents.TryGetValue(tag, out var panel)) {
                return;
            }

            panel.TagStateChangedEvent -= HandleTagStateChanged;
            panel.DeleteButtonClickedEvent -= HandleTagDeleteButtonClicked;
            //starting disappear animation
            panel.DisappearAnimationFinishedEvent += HandleTagDisappearAnimationFinished;
            panel.SetTagPresented(false, false);
        }

        private void SetEditModeEnabled(bool enabled) {
            _createButtonState.IsInteractable = !enabled;
            foreach (var (_, tag) in _tagsPool.SpawnedComponents) {
                tag.SetEditModeEnabled(enabled, false);
            }
        }

        #endregion

        #region Construct

        private TagCreationDialog _tagCreationDialog = null!;
        private TagDeletionDialog _tagDeletionDialog = null!;
        private Layout _tagsContainer = null!;
        private State<GraphicState> _createButtonState = null!;

        protected override GameObject Construct() {
            var editButtonState = Remember(GraphicState.None);
            _createButtonState = Remember(GraphicState.None);

            return new Background {
                Children = {
                    new TagCreationDialog()
                        .WithPlacement(RelativePlacement.Center)
                        .Bind(ref _tagCreationDialog),
                    //
                    new TagDeletionDialog()
                        .WithPlacement(RelativePlacement.Center)
                        .Bind(ref _tagDeletionDialog),
                    //
                    new ScrollArea {
                        ScrollContext = new ScrollContext(),
                        LineSize = 7f,
                        ScrollContent = new Layout()
                            .AsFlexGroup(
                                padding: 2f,
                                wrap: Wrap.Wrap,
                                alignItems: Align.FlexStart,
                                justifyContent: Justify.SpaceAround,
                                gap: new() { x = 1.5f, y = 2f }
                            )
                            .AsFlexItem(size: new() { y = "auto" })
                            .Bind(ref _tagsContainer)
                    }.AsFlexItem(flexGrow: 1f, margin: new() { top = 1f }),
                    //action buttons
                    new Background {
                        Sprite = BundleLoader.Sprites.backgroundBottom,
                        Children = {
                            //delete button
                            CreateGlowingButton(
                                BundleLoader.Sprites.trashIcon,
                                editButtonState,
                                () => {
                                    editButtonState.IsActive = !editButtonState.IsActive;
                                    SetEditModeEnabled(editButtonState.IsActive);
                                }
                            ).AsFlexItem(size: 4f),
                            //create button
                            CreateGlowingButton(
                                BundleLoader.Sprites.plusIcon,
                                _createButtonState,
                                HandleCreateButtonClicked
                            ).AsFlexItem(size: 4f)
                        }
                    }.AsBlurBackground(
                        color: Color.white.ColorWithAlpha(0.9f)
                    ).AsFlexGroup(
                        padding: 1f,
                        justifyContent: Justify.SpaceAround
                    ).AsFlexItem(size: new() { y = 6f })
                }
            }.AsBlurBackground().AsFlexGroup(
                direction: FlexDirection.Column,
                gap: 1f,
                constrainHorizontal: false,
                constrainVertical: false
            ).Use();
        }

        [StateGen]
        private static Image CreateGlowingButton(Sprite sprite, State<GraphicState> graphicState, Action onClick) {
            return new Image {
                Sprite = sprite,
                Material = BundleLoader.Materials.uiAdditiveGlowMaterial,
                sColor = graphicState.MapColorSet(UIStyle.GlowingButtonColorSet)
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

        private void HandleCreateButtonClicked() {
            _tagCreationDialog.PlacementAnchor = CanvasTransform;
            _tagCreationDialog.IsPushed = true;
        }

        private void HandleTagDeleteButtonClicked(ReplayTag tag) {
            _tagDeletionDialog.SetTag(tag);
            _tagDeletionDialog.PlacementAnchor = CanvasTransform;
            _tagDeletionDialog.IsPushed = true;
        }

        private void HandleTagDisappearAnimationFinished(ReplayTag tag) {
            if (_selectedTags.Contains(tag)) {
                HandleTagStateChanged(tag, false);
            }

            var panel = _tagsPool.SpawnedComponents[tag];
            panel.DisappearAnimationFinishedEvent -= HandleTagDisappearAnimationFinished;
            _tagsPool.Despawn(panel);
        }

        private void HandleTagStateChanged(ReplayTag tag, bool state) {
            if (state) {
                _selectedTags.Add(tag);
                SelectedTagAddedEvent?.Invoke(tag);
            } else {
                _selectedTags.Remove(tag);
                SelectedTagRemovedEvent?.Invoke(tag);
            }
        }

        #endregion
    }
}