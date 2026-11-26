using System;
using System.Collections.Generic;
using System.Linq;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;
using SelectionMode = Reactive.Components.Basic.SelectionMode;

namespace BeatLeader.UI.Hub {
    internal interface IPanelListFilter<in T> : IReactiveComponent, ITableFilter<T> {
        IEnumerable<IPanelListFilter<T>>? DependsOn { get; }
        string FilterName { get; }
        bool IsActive { get; }

        void Reset();
    }

    internal class ListFiltersPanel<T> : ReactiveComponent, ITextTableFilter<T> {
        #region TableFilter

        public Func<T, IEnumerable<string>>? SearchContract { get; set; }

        public event Action? FilterUpdatedEvent;

        private readonly Dictionary<T, string> _matchedItems = new();

        bool ITableFilter<T>.Matches(T value) {
            var filterMatches = _entries.Value.All(x => !x.Applied || x.Filter.Matches(value));
            var text = _searchContext.Text;
            
            if (!filterMatches) {
                return false;
            }
            
            if (!string.IsNullOrEmpty(text) && SearchContract != null) {
                var strings = SearchContract(value);
                var matches = strings.Any(str => str.Contains(text!, StringComparison.OrdinalIgnoreCase));
                
                if (matches) {
                    _matchedItems[value] = text!;
                }
                
                return matches;
            }
            
            return true;
        }

        public string? GetMatchedPhrase(T value) {
            if (string.IsNullOrEmpty(_searchContext.Text)) return null;
            _matchedItems.TryGetValue(value, out var res);
            return res;
        }

        #endregion

        #region Filters

        public IReadOnlyList<IPanelListFilter<T>> Filters {
            get;
            set {
                foreach (var filter in field) {
                    filter.FilterUpdatedEvent -= RefreshFilters;
                }

                field = value;

                foreach (var filter in field) {
                    filter.FilterUpdatedEvent += RefreshFilters;
                }

                RefreshFilters();
            }
        } = [];

        public Action<bool>? OnFiltersOpenedChanged { get; set; }

        private readonly record struct FilterEntry(IPanelListFilter<T> Filter, bool Toggled, bool Locked, bool Applied);

        private readonly HashSet<IPanelListFilter<T>> _untoggledFilters = new();

        private void RefreshFilters() {
            _entries.Value = Filters
                .Select(x => new FilterEntry(x, !_untoggledFilters.Contains(x), !AreDependenciesApplied(x), IsApplied(x)))
                .ToArray();

            FilterUpdatedEvent?.Invoke();

            bool IsApplied(IPanelListFilter<T> filter) {
                return filter.IsActive && !_untoggledFilters.Contains(filter) && AreDependenciesApplied(filter);
            }

            bool AreDependenciesApplied(IPanelListFilter<T> filter) {
                return filter.DependsOn?.All(IsApplied) ?? true;
            }
        }

        #endregion

        #region Construct

        private static readonly IAnimationTrack<IReactiveComponent> unfoldTrack = AnimationTracks.Create<IReactiveComponent>(
            static (x, progress) => x.ContentTransform.localScale = new Vector3(1f, progress, 1f)
        );

        private static readonly Animation<IReactiveComponent> unfold = new(unfoldTrack, 150.ms, AnimationCurve.EaseOutQuint);
        private static readonly Animation<IReactiveComponent> fold = new(unfoldTrack, 100.ms, AnimationCurve.EaseOutQuint, Direction: AnimationDirection.Reverse);

        private readonly BsInputFieldContext _searchContext = new();
        private State<IReadOnlyList<FilterEntry>> _entries = null!;

        protected override GameObject Construct() {
            _entries = Remember<IReadOnlyList<FilterEntry>>([]);

            var filtersOpened = Remember(false);
            var appliedCount = _entries.Map(static x => x.Count(static y => y.Applied));
            var filtersAnchor = Ref<RectTransform>();
            var searchAnchor = Ref<RectTransform>();

            filtersOpened.AddCallback(this, static (x, y) => x.OnFiltersOpenedChanged?.Invoke(y), lazy: true);
            _searchContext.Map(static x => x.Text).AddCallback(this, static (x, _) => x.FilterUpdatedEvent?.Invoke(), lazy: true);

            return new Layout {
                FlexController = {
                    Gap = 1f
                },

                Children = {
                    new Modal {
                        PlacementAnchor = filtersAnchor,
                        PlacementData = new() {
                            Placement = RelativePlacement.BottomCenter,
                            Offset = new(31.5f, 2f),
                            Clip = false
                        },

                        OpenAnimation = unfold,
                        CloseAnimation = fold,
                        CompositionAnimation = new() { DimCanvas = false },

                        sIsPushed = filtersOpened,
                        OnPopped = () => filtersOpened.Value = false,
                        OnClickOutside = () => filtersOpened.Value = false,

                        Children = {
                            new Background {
                                FlexController = {
                                    FlexDirection = FlexDirection.Column,
                                    Padding = 1.5f.pt,
                                    Gap = 1.5f.pt
                                },

                                FlexItem = {
                                    Size = new() { x = 58f, y = 68f }
                                },

                                Children = {
                                    new Layout {
                                        FlexController = {
                                            AlignItems = Align.Center,
                                            Padding = new() { left = 1.pt, right = 1.pt }
                                        },

                                        FlexItem = {
                                            Size = new() { y = 5.pt }
                                        },

                                        Children = {
                                            new Label {
                                                FlexItem = {
                                                    FlexGrow = 1f
                                                },

                                                Text = "Filters",
                                                FontSize = 4f,
                                                Alignment = TextAlignmentOptions.MidlineLeft
                                            },

                                            new BsTextButton {
                                                FlexItem = {
                                                    Size = new() { x = 16.pt }
                                                },

                                                Text = "Clear all",
                                                Alignment = TextAlignmentOptions.Capline,
                                                ShowUnderline = false,

                                                OnClick = () => {
                                                    foreach (var filter in Filters) {
                                                        filter.Reset();
                                                    }
                                                }
                                            }
                                        }
                                    },

                                    new Repeater<FilterEntry, FilterSection> {
                                        FlexController = {
                                            FlexDirection = FlexDirection.Column,
                                            Gap = 1.5f.pt
                                        },

                                        SelectionMode = SelectionMode.None,
                                        sItems = _entries,

                                        ConstructCell = ctx => new FilterSection {
                                            sCaption = ctx.Map(static x => x.Item.Filter.FilterName),
                                            sFilterView = ctx.Map(static x => (ILayoutItem?)x.Item.Filter),
                                            sToggled = ctx.Map(static x => x.Item.Toggled),
                                            sLocked = ctx.Map(static x => x.Item.Locked),
                                            
                                            OnToggled = x => HandleFilterToggled(ctx.Item.Filter, x)
                                        }
                                    }
                                }
                            }.AsBlurBackground().AsRectMask()
                        }
                    },

                    new BsInputField {
                        FlexItem = {
                            FlexGrow = 1f
                        },

                        Context = _searchContext,
                        Placeholder = "Search"
                    }.Bind(searchAnchor),

                    new BsKeyboardModal {
                        Context = _searchContext,
                        PlacementData = new() {
                            Placement = RelativePlacement.BottomCenter,
                            Offset = new(0f, 32f),
                            Clip = true
                        },
                        PlacementAnchor = searchAnchor
                    },

                    new BsAeroButton {
                        FlexItem = {
                            Size = new() { x = 35.pct }
                        },

                        Skew = 0f,
                        OnClick = () => filtersOpened.Value = true,

                        ConstructContent = (color, _) => new Layout {
                            FlexController = {
                                AlignItems = Align.Stretch,
                                Padding = new() { left = 1.5f.pt, right = 1.5f.pt },
                                Gap = 1.pt
                            },

                            FlexItem = {
                                FlexGrow = 1f
                            },

                            Children = {
                                new Image {
                                    FlexItem = {
                                        Size = new() { y = 62.5f.pct },
                                        AspectRatio = 1f,
                                        AlignSelf = Align.Center
                                    },

                                    Sprite = GameResources.Sprites.FilterIcon,
                                    PreserveAspect = true,
                                    sColor = color.In()
                                },

                                new Label {
                                    FlexItem = {
                                        FlexGrow = 1f,
                                        Margin = new() { left = 0.5f.pt }
                                    },

                                    Text = "Filters",
                                    Alignment = TextAlignmentOptions.CaplineLeft,
                                    FontStyle = FontStyles.Italic,
                                    EnableWrapping = false,
                                    EnableAutoSizing = true,
                                    FontSizeMin = 2f,
                                    FontSizeMax = 4f,
                                    sColor = color.In()
                                },

                                new Background {
                                    FlexController = {
                                        JustifyContent = Justify.Center,
                                        AlignItems = Align.Center
                                    },

                                    FlexItem = {
                                        Size = 4.5f.pt,
                                        AlignSelf = Align.Center
                                    },

                                    Sprite = global::Reactive.BeatSaber.GameResources.CircleIcon,
                                    Color = UIStyle.GlowingButtonColorSet.GetColor(GraphicState.Active),
                                    sEnabled = appliedCount.Map(static x => x > 0),

                                    Children = {
                                        new Label {
                                            FontSize = 2.8f,
                                            Alignment = TextAlignmentOptions.Capline,
                                            Color = Color.black,
                                            sText = appliedCount.Map(static x => x.ToString())
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }.Bind(filtersAnchor).Use();
        }

        #endregion

        #region Callbacks

        private void HandleFilterToggled(IPanelListFilter<T> filter, bool toggled) {
            if (toggled) {
                _untoggledFilters.Remove(filter);
            } else {
                _untoggledFilters.Add(filter);
            }

            RefreshFilters();
        }

        #endregion
    }
}