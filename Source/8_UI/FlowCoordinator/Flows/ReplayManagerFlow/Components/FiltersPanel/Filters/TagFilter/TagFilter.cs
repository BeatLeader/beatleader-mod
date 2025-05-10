using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class TagFilter : ReactiveComponent, IPanelListFilter<IReplayHeader> {
        #region Filter

        public IEnumerable<IPanelListFilter<IReplayHeader>>? DependsOn => null;
        public string FilterName => "Tag Filter";
        public string FilterStatus { get; private set; } = null!;

        public event Action? FilterUpdatedEvent;

        private readonly HashSet<ReplayTag> _selectedTags = new();

        public bool Matches(IReplayHeader value) {
            var i = value.ReplayMetadata.Tags.Count(tag => _selectedTags.Contains(tag));
            return i == _selectedTags.Count;
        }

        private void RefreshFilterStatus() {
            if (_selectedTags.Count == 0) {
                FilterStatus = "No Tags";
                return;
            }
            var items = _selectedTags.Take(3).Select((x, idx) => $"{(idx > 0 ? ", " : "")}{x.Name}");
            FilterStatus = "Tags: " + string.Join(string.Empty, items);
        }

        #endregion

        #region Setup

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
            return new Layout {
                Children = {
                    new TagSelectorModal()
                        .WithShadow()
                        .WithOpenListener(HandleTagSelectorOpened)
                        .WithCloseListener(HandleTagSelectorClosed)
                        .WithAnchor(
                            Lazy<IReactiveComponent>(() => _textArea),
                            RelativePlacement.BottomCenter
                        )
                        .Bind(ref _tagSelectorModal),
                    //text area
                    new TextArea {
                            Placeholder = "Choose Tags",
                            Icon = GameResources.Sprites.EditIcon
                        }
                        .WithListener(x => x.Text, HandleTextAreaTextChanged)
                        .WithListener(x => x.Focused, HandleTextAreaFocusChanged)
                        .AsFlexItem(flexGrow: 1f)
                        .Bind(ref _textArea)
                }
            }.AsFlexGroup(
                padding: new() { top = 1f, bottom = 1f },
                justifyContent: Justify.Center
            ).Use();
        }

        protected override void OnInitialize() {
            RefreshFilterStatus();
            this.AsFlexItem(size: new() { x = 52f, y = 10f });
        }

        #endregion

        #region Callbacks

        private void HandleTagSelectorOpened(IModal modal, bool finished) {
            if (finished) return;
            _tagSelectorModalOpened = true;
            var tagSelector = _tagSelectorModal.Component;

            tagSelector.AsFlexItem(size: new() { x = 50.pt(), y = 40.pt() });
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

        private void HandleSelectedTagAdded(ReplayTag tag) {
            _selectedTags.Add(tag);
            RefreshFilterStatus();
            RefreshTextArea();
        }

        private void HandleSelectedTagRemoved(ReplayTag tag) {
            _selectedTags.Remove(tag);
            RefreshFilterStatus();
            RefreshTextArea();
        }

        #endregion
    }
}