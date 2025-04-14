using System;
using System.Collections.Generic;
using System.Threading;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using Image = Reactive.BeatSaber.Components.Image;
using ImageButton = Reactive.BeatSaber.Components.ImageButton;
using ScrollArea = Reactive.BeatSaber.Components.ScrollArea;
using ScrollOrientation = Reactive.Components.Basic.ScrollOrientation;

namespace BeatLeader.UI.Hub {
    internal class TagSelector : ReactiveComponent {
        #region Setup

        public IReadOnlyCollection<ReplayTag> SelectedTags => _selectedTags;

        public event Action<ReplayTag>? SelectedTagAddedEvent;
        public event Action<ReplayTag>? SelectedTagRemovedEvent;

        private readonly HashSet<ReplayTag> _selectedTags = new();
        
        public void SelectTags(ICollection<ReplayTag> tags) {
            foreach (var (tag, panel) in _tagsPool.SpawnedComponents) {
                var selected = tags.Contains(tag);
                panel.SetTagSelected(selected, true);
            }
            NotifySelectedTagsChanged();
        }

        public void ClearSelectedTags() {
            foreach (var tag in _selectedTags) {
                var panel = _tagsPool.SpawnedComponents[tag];
                panel.SetTagSelected(false, true);
                SelectedTagRemovedEvent?.Invoke(tag);
            }
            _selectedTags.Clear();
            NotifySelectedTagsChanged();
        }

        private void NotifySelectedTagsChanged() {
            NotifyPropertyChanged(nameof(SelectedTags));
        }

        protected override void OnInitialize() {
            ReplayMetadataManager.TagCreatedEvent += HandleTagCreated;
            ReplayMetadataManager.TagDeletedEvent += HandleTagDeleted;
            
            SetTags(ReplayMetadataManager.Tags.Values);
        }

        protected override void OnDestroy() {
            ReplayMetadataManager.TagCreatedEvent -= HandleTagCreated;
            ReplayMetadataManager.TagDeletedEvent -= HandleTagDeleted;
        }

        #endregion

        #region Tags

        private readonly ReactivePool<ReplayTag, TagPanel> _tagsPool = new() { DetachOnDespawn = false };

        private void SetTags(IEnumerable<ReplayTag> tags) {
            _tagsContainer.Children.Clear();
            DespawnAllTags();
            foreach (var tag in tags) {
                SpawnTag(tag);
            }
        }

        private void SpawnTag(ReplayTag tag) {
            var panel = _tagsPool.Spawn(tag);
            panel.AsFlexItem();
            panel.SetTag(tag);
            panel.TagStateChangedEvent += HandleTagStateChanged;
            panel.DeleteButtonClickedEvent += HandleTagDeleteButtonClicked;
            _tagsContainer.Children.Add(panel);
        }

        private void DespawnTag(ReplayTag tag, bool animated) {
            if (!_tagsPool.SpawnedComponents.TryGetValue(tag, out var panel)) {
                return;
            }

            panel.TagStateChangedEvent -= HandleTagStateChanged;
            panel.DeleteButtonClickedEvent -= HandleTagDeleteButtonClicked;
            //starting disappear animation
            panel.StateAnimationFinishedEvent += HandleTagStateAnimationFinished;
            panel.SetTagPresented(false, !animated);
        }

        private void DespawnAllTags() {
            foreach (var (_, panel) in _tagsPool.SpawnedComponents) {
                panel.TagStateChangedEvent -= HandleTagStateChanged;
                panel.DeleteButtonClickedEvent -= HandleTagDeleteButtonClicked;
            }
            _tagsPool.DespawnAll();
        }

        private void SetEditModeEnabled(bool enabled) {
            _createTagButton.Interactable = !enabled;
            foreach (var (_, tag) in _tagsPool.SpawnedComponents) {
                tag.SetEditModeEnabled(enabled, false);
            }
        }

        #endregion

        #region Construct

        private TagCreationDialog _tagCreationDialog = null!;
        private TagDeletionDialog _tagDeletionDialog = null!;
        private Layout _tagsContainer = null!;
        private ImageButton _createTagButton = null!;

        protected override GameObject Construct() {
            return new ImageLayout {
                Children = {
                    new TagCreationDialog()
                        .WithJumpAnimation()
                        .WithAnchor(this, RelativePlacement.Center, unbindOnceOpened: false)
                        .WithShadow()
                        .Bind(ref _tagCreationDialog),
                    //
                    new TagDeletionDialog()
                        .WithJumpAnimation()
                        .WithAnchor(this, RelativePlacement.Center, unbindOnceOpened: false)
                        .WithShadow()
                        .Bind(ref _tagDeletionDialog),
                    //
                    new ScrollArea {
                        ScrollOrientation = ScrollOrientation.Vertical,
                        ScrollContent = new Layout()
                            .AsRootFlexGroup(
                                padding: 2f,
                                wrap: Wrap.Wrap,
                                overflow: Overflow.Scroll,
                                alignItems: Align.FlexStart,
                                gap: new() { x = 1.5f, y = 2f }
                            )
                            .AsFlexItem(size: new() { y = "auto" })
                            .Bind(ref _tagsContainer)
                    }.AsFlexItem(grow: 1f, margin: new() { top = 1f }),
                    //action buttons
                    new ImageLayout {
                        Sprite = BundleLoader.Sprites.backgroundBottom,
                        Children = {
                            //delete button
                            new ImageButton {
                                Image = {
                                    Sprite = BundleLoader.Sprites.trashIcon,
                                    Material = BundleLoader.Materials.uiAdditiveGlowMaterial
                                },
                                Colors = UIStyle.ButtonColorSet,
                                Latching = true,
                                OnStateChanged = SetEditModeEnabled,
                            }.AsFlexItem(size: 4f),
                            //create button
                            new ImageButton {
                                Image = {
                                    Sprite = BundleLoader.Sprites.plusIcon,
                                    Material = BundleLoader.Materials.uiAdditiveGlowMaterial
                                },
                                Colors = UIStyle.ButtonColorSet
                            }.WithModal(_tagCreationDialog).AsFlexItem(size: 4f).Bind(ref _createTagButton),
                        }
                    }.AsBlurBackground(
                        color: Color.white.ColorWithAlpha(0.9f)
                    ).AsFlexGroup(padding: 1f).AsFlexItem(size: new() { y = 6f })
                }
            }.AsBlurBackground().AsFlexGroup(
                direction: FlexDirection.Column,
                gap: 1f
            ).Use();
        }

        #endregion

        #region Callbacks

        private void HandleTagDeleteButtonClicked(ReplayTag tag) {
            _tagDeletionDialog.SetTag(tag);
            _tagDeletionDialog.Present(ContentTransform);
        }

        private void HandleTagStateAnimationFinished(ReplayTag tag) {
            if (_selectedTags.Contains(tag)) {
                HandleTagStateChanged(tag, false);
            }
            var panel = _tagsPool.SpawnedComponents[tag];
            panel.StateAnimationFinishedEvent -= HandleTagStateAnimationFinished;
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
            NotifySelectedTagsChanged();
        }

        private void HandleTagCreated(ReplayTag tag) {
            SynchronizationContext.Current.Send(
                _ => SpawnTag(tag),
                null
            );
        }

        private void HandleTagDeleted(ReplayTag tag) {
            SynchronizationContext.Current.Send(
                _ => DespawnTag(tag, true),
                null
            );
        }

        #endregion
    }
}