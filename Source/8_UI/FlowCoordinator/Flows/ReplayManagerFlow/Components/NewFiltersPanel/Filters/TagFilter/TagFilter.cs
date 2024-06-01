using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class TagFilter : ReactiveComponent, ListFiltersPanel<IReplayHeader>.IFilter {
        #region Filter

        public IEnumerable<ListFiltersPanel<IReplayHeader>.IFilter>? DependsOn => null;
        public string FilterName => "Tag Filter";

        public event Action? FilterUpdatedEvent;

        public bool Matches(IReplayHeader value) {
            var i = value.ReplayMetadata.Tags.Count(tag => _tagSelector.SelectedTags.Contains(tag));
            return i == _tagSelector.SelectedTags.Count;
        }

        #endregion

        #region Setup

        public void Setup(IReplayTagManager tagManager) {
            _tagSelector.Setup(tagManager);
        }

        #endregion

        #region Construct

        private TagSelector _tagSelector = null!;
        private TextArea _textArea = null!;
        private Modal<TagSelector> _modal = null!;

        private void OpenModal() {
            ModalSystemHelper.OpenModalRelatively(
                _modal,
                ContentTransform,
                _textArea.ContentTransform,
                ModalSystemHelper.RelativePlacement.BottomRight,
                shadowSettings: new()
            );
        }

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new Modal<TagSelector>()
                        .With(
                            x => x.Component
                                .WithListener(
                                    y => y.SelectedTags,
                                    y => _textArea.WithItemsText(
                                        y.Select(static x => x.Name)
                                    )
                                )
                                .Bind(ref _tagSelector)
                        )
                        .With(
                            x => {
                                x.ModalAskedToBeClosedEvent += () => _textArea.Focused = false;
                            }
                        ).Bind(ref _modal),
                    //text area
                    new TextArea {
                        Placeholder = "Choose Tags",
                        Icon = GameResources.Sprites.EditIcon
                    }.WithListener(
                        x => x.Text,
                        x => {
                            if (x.Length == 0) {
                                _tagSelector.ClearSelectedTags();
                            }
                            FilterUpdatedEvent?.Invoke();
                        }
                    ).WithListener(
                        x => x.Focused,
                        x => {
                            if (x) OpenModal();
                        }
                    ).AsFlexItem(grow: 1f).Bind(ref _textArea)
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
    }
}