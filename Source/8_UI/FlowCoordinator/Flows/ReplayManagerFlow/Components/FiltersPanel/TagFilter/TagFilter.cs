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
            _textAreaContext.WithItemsText(names);
            _textAreaText = _textAreaContext.Text;
            FilterUpdatedEvent?.Invoke();
        }

        #endregion

        #region Construct

        private readonly BsInputFieldContext _textAreaContext = new();
        private string? _textAreaText;

        private BsInputField _textArea = null!;
        private TagSelectorModal _tagSelectorModal = null!;

        protected override GameObject Construct() {
            return new Layout {
                FlexController = {
                    Padding = new() { top = 1f, bottom = 1f },
                    JustifyContent = Justify.Center
                },

                FlexItem = {
                    Size = new() { x = 52f, y = 10f }
                },

                Children = {
                    new TagSelectorModal {
                        OnPushed = HandleTagSelectorOpened,
                        OnClickOutside = HandleTagSelectorClickedOutside,
                        PlacementData = new() { Placement = RelativePlacement.BottomCenter, Offset = new(0f, 0.5f) },
                        Component = {
                            FlexItem = {
                                Size = new() { x = 50.pt, y = 40.pt }
                            }
                        }
                    }.Bind(ref _tagSelectorModal),
                    //text area
                    new BsInputField {
                        FlexItem = {
                            FlexGrow = 1f
                        },

                        Context = _textAreaContext,
                        Placeholder = "Choose Tags",
                        Icon = GameResources.Sprites.EditIcon
                    }.Bind(ref _textArea)
                }
            }.Use();
        }

        protected override void OnInitialize() {
            RefreshFilterStatus();

            _tagSelectorModal.PlacementAnchor = _textArea.ContentTransform;
            _textAreaContext.AddCallback(this, static (x, y) => x.HandleTextAreaContextUpdated(y), lazy: true);
        }

        #endregion

        #region Callbacks

        private void HandleTagSelectorOpened() {
            var tagSelector = _tagSelectorModal.Component;

            tagSelector.SelectTags(_selectedTags);
            tagSelector.SelectedTagAddedEvent += HandleSelectedTagAdded;
            tagSelector.SelectedTagRemovedEvent += HandleSelectedTagRemoved;
        }

        private void HandleTagSelectorClickedOutside() {
            _tagSelectorModal.IsPushed = false;

            var tagSelector = _tagSelectorModal.Component;
            tagSelector.SelectedTagAddedEvent -= HandleSelectedTagAdded;
            tagSelector.SelectedTagRemovedEvent -= HandleSelectedTagRemoved;
        }

        private void HandleTextAreaContextUpdated(BsInputFieldContext context) {
            if (context.Focused) {
                _tagSelectorModal.IsPushed = true;
                context.Focused = false;
            }

            if (context.Text != _textAreaText) {
                _textAreaText = context.Text;

                if (string.IsNullOrEmpty(_textAreaText)) {
                    _tagSelectorModal.Component.ClearSelectedTags();
                    _selectedTags.Clear();
                    RefreshFilterStatus();
                    FilterUpdatedEvent?.Invoke();
                }
            }
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