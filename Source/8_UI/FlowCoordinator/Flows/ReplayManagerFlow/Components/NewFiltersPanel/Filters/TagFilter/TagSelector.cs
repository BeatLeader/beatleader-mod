using System;
using System.Collections.Generic;
using System.Threading;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using IPA.Utilities;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class TagSelector : ReactiveComponent {
        #region Setup

        public IReadOnlyCollection<IReplayTag> SelectedTags => _selectedTags;

        public event Action<IReplayTag>? SelectedTagAddedEvent;
        public event Action<IReplayTag>? SelectedTagRemovedEvent;

        private readonly HashSet<IReplayTag> _selectedTags = new();
        private IReplayTagManager? _tagManager;

        public void Setup(IReplayTagManager tagManager) {
            if (_tagManager == tagManager) return;
            if (_tagManager != null) {
                _tagManager.TagCreatedEvent -= HandleTagCreated;
                _tagManager.TagDeletedEvent -= HandleTagDeleted;
            }
            _tagManager = tagManager;
            _tagCreationDialog.Setup(tagManager);
            _tagDeletionDialog.Setup(tagManager);
            DespawnAllTags();
            if (_tagManager != null) {
                SetTags(_tagManager.Tags);
                _tagManager.TagCreatedEvent += HandleTagCreated;
                _tagManager.TagDeletedEvent += HandleTagDeleted;
            }
        }

        public void SelectTags(ICollection<IReplayTag> tags) {
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

        #endregion

        #region Tags

        private readonly ReactivePool<IReplayTag, TagPanel> _tagsPool = new() { DetachOnDespawn = false };

        private void SetTags(IEnumerable<IReplayTag> tags) {
            _tagsContainer.Children.Clear();
            DespawnAllTags();
            foreach (var tag in tags) {
                SpawnTag(tag);
            }
        }

        private void SpawnTag(IReplayTag tag) {
            var panel = _tagsPool.Spawn(tag);
            panel.AsFlexItem();
            panel.SetTag(tag);
            panel.TagStateChangedEvent += HandleTagStateChanged;
            panel.DeleteButtonClickedEvent += HandleTagDeleteButtonClicked;
            _tagsContainer.Children.Add(panel);
        }

        private void DespawnTag(IReplayTag tag, bool animated) {
            if (!_tagsPool.SpawnedComponents.TryGetValue(tag, out var panel)) return;
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
        private Dummy _tagsContainer = null!;
        private ImageButton _createTagButton = null!;

        protected override GameObject Construct() {
            return new Image {
                Children = {
                    new TagCreationDialog()
                        .WithAnchor(this, RelativePlacement.Center)
                        .WithShadow()
                        .Bind(ref _tagCreationDialog),
                    //
                    new TagDeletionDialog()
                        .WithAnchor(this, RelativePlacement.Center)
                        .WithShadow()
                        .Bind(ref _tagDeletionDialog),
                    //
                    new ScrollArea {
                        ScrollOrientation = ScrollOrientation.Vertical,
                        ScrollContent = new Dummy()
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
                    new Image {
                        Sprite = BundleLoader.Sprites.backgroundBottom,
                        Children = {
                            //delete button
                            new ImageButton {
                                Image = {
                                    Sprite = BundleLoader.Sprites.trashIcon
                                },
                                Sticky = true
                            }.WithStateListener(SetEditModeEnabled).AsFlexItem(size: 4f),
                            //create button
                            new ImageButton {
                                Image = {
                                    Sprite = BundleLoader.Sprites.plusIcon
                                }
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

        private void HandleTagDeleteButtonClicked(IReplayTag tag) {
            _tagDeletionDialog.SetTag((IEditableReplayTag)tag);
            _tagDeletionDialog.Present(ContentTransform);
        }

        private void HandleTagStateAnimationFinished(IReplayTag tag) {
            if (_selectedTags.Contains(tag)) {
                HandleTagStateChanged(tag, false);
            }
            var panel = _tagsPool.SpawnedComponents[tag];
            panel.StateAnimationFinishedEvent -= HandleTagStateAnimationFinished;
            _tagsPool.Despawn(panel);
        }

        private void HandleTagStateChanged(IReplayTag tag, bool state) {
            if (state) {
                _selectedTags.Add(tag);
                SelectedTagAddedEvent?.Invoke(tag);
            } else {
                _selectedTags.Remove(tag);
                SelectedTagRemovedEvent?.Invoke(tag);
            }
            NotifySelectedTagsChanged();
        }

        private void HandleTagCreated(IReplayTag tag) {
            SynchronizationContext.Current.Send(
                _ => SpawnTag(tag),
                null
            );
        }

        private void HandleTagDeleted(IReplayTag tag) {
            SynchronizationContext.Current.Send(
                _ => DespawnTag(tag, true),
                null
            );
        }

        #endregion
    }
}