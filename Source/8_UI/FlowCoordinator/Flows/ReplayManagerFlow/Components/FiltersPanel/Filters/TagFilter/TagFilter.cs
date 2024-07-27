using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class TagFilter : ReactiveComponent, IPanelListFilter<IReplayHeaderBase> {
        #region Filter

        public IEnumerable<IPanelListFilter<IReplayHeaderBase>>? DependsOn => null;
        public string FilterName => "Tag Filter";

        public event Action? FilterUpdatedEvent;

        private readonly HashSet<IReplayTag> _selectedTags = new();

        public bool Matches(IReplayHeaderBase value) {
            var i = value.ReplayMetadata.Tags.Count(tag => _selectedTags.Contains(tag));
            return i == _selectedTags.Count;
        }

        #endregion

        #region Setup

        private IReplayTagManager? _tagManager;

        public void Setup(IReplayTagManager tagManager) {
            _tagManager = tagManager;
        }

        private void RefreshTextArea() {
            var names = _tagSelectorModal.Component.SelectedTags.Select(static x => x.Name);
            _textArea.WithItemsText(names, true);
            FilterUpdatedEvent?.Invoke();
        }

        #endregion

        #region Construct

        private TextArea _textArea = null!;
        private TagSelectorModal _tagSelectorModal = null!;
        private bool _tagSelectorModalOpened;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new TagSelectorModal()
                        .WithShadow()
                        .WithOpenListener(HandleTagSelectorOpened)
                        .WithCloseListener(HandleTagSelectorClosed)
                        .WithAnchor(() => _textArea, RelativePlacement.BottomCenter)
                        .Bind(ref _tagSelectorModal),
                    //text area
                    new TextArea {
                            Placeholder = "Choose Tags",
                            Icon = GameResources.Sprites.EditIcon
                        }
                        .WithListener(x => x.Text, HandleTextAreaTextChanged)
                        .WithListener(x => x.Focused, HandleTextAreaFocusChanged)
                        .AsFlexItem(grow: 1f)
                        .Bind(ref _textArea)
                }
            }.AsFlexGroup(
                padding: 1f,
                justifyContent: Justify.Center
            ).Use();
        }

        protected override void OnInitialize() {
            this.AsFlexItem(size: new() { x = 52f, y = 10f });
        }

        #endregion

        #region Callbacks

        private void HandleTagSelectorOpened(IModal modal, bool finished) {
            if (finished) return;
            _tagSelectorModalOpened = true;
            var tagSelector = _tagSelectorModal.Component;
            if (_tagManager == null) {
                throw new UninitializedComponentException();
            }
            tagSelector.WithSizeDelta(50f, 40f);
            tagSelector.Setup(_tagManager);
            tagSelector.SelectTags(_selectedTags);
            tagSelector.SelectedTagAddedEvent += HandleSelectedTagAdded;
            tagSelector.SelectedTagRemovedEvent += HandleSelectedTagRemoved;
        }

        private void HandleTagSelectorClosed(IModal modal, bool finished) {
            _textArea.Focused = false;
            if (finished) return;
            _tagSelectorModalOpened = false;
            var tagSelector = _tagSelectorModal.Component;
            tagSelector.SelectedTagAddedEvent -= HandleSelectedTagAdded;
            tagSelector.SelectedTagRemovedEvent -= HandleSelectedTagRemoved;
        }

        private void HandleTextAreaFocusChanged(bool focused) {
            if (!focused) return;
            _tagSelectorModal.Present(ContentTransform);
        }

        private void HandleTextAreaTextChanged(string text) {
            if (text.Length != 0) return;
            //
            if (_tagSelectorModalOpened) {
                _tagSelectorModal.Component.ClearSelectedTags();
            }
            _selectedTags.Clear();
            FilterUpdatedEvent?.Invoke();
        }

        private void HandleSelectedTagAdded(IReplayTag tag) {
            _selectedTags.Add(tag);
            RefreshTextArea();
        }

        private void HandleSelectedTagRemoved(IReplayTag tag) {
            _selectedTags.Remove(tag);
            RefreshTextArea();
        }

        #endregion
    }
}