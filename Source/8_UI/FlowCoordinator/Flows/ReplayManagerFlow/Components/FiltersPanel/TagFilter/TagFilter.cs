using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.Hub {
    internal class TagFilter : ReactiveComponent, IPanelListFilter<IReplayHeader> {
        #region Filter

        public IEnumerable<IPanelListFilter<IReplayHeader>>? DependsOn => null;
        public string FilterName => "Tags";
        public bool IsActive => _selection.Tags.Count > 0;

        public void Reset() {
            foreach (var tag in _selection.Tags.ToArray()) {
                _selection.Tags.Remove(tag);
            }

            _tagSelector.SelectedTags = _selection.Tags;
        }

        public event Action? FilterUpdatedEvent;

        private readonly ReplayMetadata _selection = new();

        public bool Matches(IReplayHeader value) {
            return _selection.Tags.All(value.ReplayMetadata.Tags.Contains);
        }

        #endregion

        #region Construct

        private TagSelector _tagSelector = null!;

        protected override GameObject Construct() {
            var selectorOpened = Remember(false);
            var selectorHeight = RememberAnimated(selectorOpened.Map(static x => x ? 37.5f : 0f), 300.ms, AnimationCurve.EaseOutQuint);

            _selection.TagAddedEvent += _ => FilterUpdatedEvent?.Invoke();
            _selection.TagRemovedEvent += _ => FilterUpdatedEvent?.Invoke();

            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column
                },

                Children = {
                    new TagsStrip {
                        FlexItem = {
                            Size = new() { y = 5f }
                        },

                        Metadata = _selection,
                        sEditorOpened = selectorOpened,
                        OnEditClicked = () => selectorOpened.Value = !selectorOpened.Value
                    },

                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column
                        },

                        FlexItem = {
                            sSize = selectorHeight.Map(static x => new YogaVector { y = x.pt })
                        },

                        Children = {
                            new TagSelector {
                                FlexItem = {
                                    FlexGrow = 1f,
                                    Margin = new() { top = 0.5f.pt }
                                },

                                OnSelectedTagAdded = x => _selection.Tags.Add(x),
                                OnSelectedTagRemoved = x => _selection.Tags.Remove(x)
                            }.Bind(ref _tagSelector)
                        }
                    }.AsRectMask()
                }
            }.Use();
        }

        protected override void OnInitialize() {
            ReplayMetadataManager.TagDeletedEvent += HandleTagDeleted;
        }

        protected override void OnDestroy() {
            ReplayMetadataManager.TagDeletedEvent -= HandleTagDeleted;
        }

        #endregion

        #region Callbacks

        private void HandleTagDeleted(ReplayTag tag) {
            _selection.Tags.Remove(tag);
        }

        #endregion
    }
}