using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal interface IPanelListFilter<in T> : IReactiveComponent, ILayoutItem, ITableFilter<T> {
        IEnumerable<IPanelListFilter<T>>? DependsOn { get; }
        string FilterName { get; }
        string FilterStatus { get; }
    }

    internal class ListFiltersPanel<T> : ReactiveComponent, ITextTableFilter<T> {
        #region TableFilter

        public Func<T, IEnumerable<string>>? SearchContract { get; set; }

        public event Action? FilterUpdatedEvent;

        private readonly Dictionary<T, string> _matchedItems = new();

        bool ITableFilter<T>.Matches(T value) {
            var filterMatches = _modal.ActiveFilters.All(x => x.Matches(value));
            var text = SearchText.ToLower();
            if (!filterMatches) return false;
            if (!string.IsNullOrEmpty(text) && SearchContract != null) {
                var strings = SearchContract(value);
                var matches = strings.Any(str => str.ToLower().Contains(text));
                if (matches) _matchedItems[value] = text;
                return matches;
            }
            return true;
        }

        public string? GetMatchedPhrase(T value) {
            if (string.IsNullOrEmpty(SearchText)) return null;
            _matchedItems.TryGetValue(value, out var res);
            return res;
        }

        #endregion

        #region Filters

        public ICollection<IPanelListFilter<T>> Filters => _filters;

        private ObservableCollectionAdapter<IPanelListFilter<T>> _filters = null!;

        #endregion

        #region FilterContainer

        private class FilterContainer : ReactiveComponent {
            #region Setup

            public bool FilterEnabled {
                get => _enableToggle.Active;
                set {
                    _enableToggle.Active = value;
                    HandleButtonClicked(true);
                }
            }

            public event Action<IPanelListFilter<T>, bool>? FilterUpdatedEvent;

            private IPanelListFilter<T>? _filter;

            public void Setup(IPanelListFilter<T>? filter) {
                if (_filter != null) {
                    _filterContainer.OpenedView = null;
                    _filter.FilterUpdatedEvent -= HandleFilterUpdated;
                }
                _filter = filter;
                if (_filter != null) {
                    _filterContainer.OpenedView = _filter;
                    _filter.FilterUpdatedEvent += HandleFilterUpdated;
                    _placeholder.Text = _filter.FilterName;
                }
            }

            private void HandleButtonClicked(bool silent) {
                _filterContainer.Opened = FilterEnabled;
                if (silent) return;
                FilterUpdatedEvent?.Invoke(_filter!, FilterEnabled);
            }

            private void HandleFilterUpdated() {
                FilterUpdatedEvent?.Invoke(_filter!, FilterEnabled);
            }

            #endregion

            #region Construct

            private Label _placeholder = null!;
            private PushContainer _filterContainer = null!;
            private CheckCircle _enableToggle = null!;

            protected override GameObject Construct() {
                return new Layout {
                    FlexController = {
                        Gap = 1f
                    },

                    Children = {
                        new PushContainer {
                            FlexItem = {
                                FlexGrow = 1f
                            },

                            ClosedView = new Label()
                                .WithRectExpand()
                                .Bind(ref _placeholder),
                            
                            BackgroundImage = {
                                Color = (Color.white * 0.9f).ColorWithAlpha(1f)
                            }
                        }.Bind(ref _filterContainer),
                        //
                        new Background {
                            FlexController = {
                                AlignItems = Align.Center,
                                Padding = 1f
                            },

                            FlexItem = {
                                Size = YogaVector.Auto
                            },

                            Color = (Color.white * 0.9f).ColorWithAlpha(1f),
                            Children = {
                                new CheckCircle {
                                    FlexItem = {
                                        Size = 3f
                                    },

                                    OnStateChanged = _ => HandleButtonClicked(false)
                                }.Bind(ref _enableToggle)
                            }
                        }.AsBlurBackground()
                    }
                }.Use();
            }

            #endregion
        }

        #endregion

        #region FiltersModal

        private class FiltersModal : ModalBase {
            #region Filter Dependencies

            private readonly Dictionary<IPanelListFilter<T>, List<IPanelListFilter<T>>> _dependencies = new();

            private void AddDependentFilter(IEnumerable<IPanelListFilter<T>>? hosts, IPanelListFilter<T> dependent) {
                if (hosts == null) return;
                foreach (var host in hosts) {
                    _dependencies.EnsureExistsAndDo(
                        host,
                        new(),
                        x => x.Add(dependent)
                    );
                }
            }

            private void RemoveDependentFilter(IEnumerable<IPanelListFilter<T>>? hosts, IPanelListFilter<T> dependent) {
                if (hosts == null) return;
                foreach (var host in hosts) {
                    _dependencies.DoIfExists(
                        host,
                        x => x.Remove(dependent)
                    );
                }
            }

            private void HandleFilterUpdated(IPanelListFilter<T> filter, bool state) {
                FilterUpdatedEvent?.Invoke(filter);
                if (state) return;
                _dependencies.DoIfExists(
                    filter,
                    deps => {
                        foreach (var dep in deps) {
                            var comp = _filtersPool.SpawnedComponents[dep];
                            comp.FilterEnabled = false;
                        }
                    }
                );
            }

            #endregion

            #region Filters

            public event Action<IPanelListFilter<T>>? FilterUpdatedEvent;

            public void AddFilter(IPanelListFilter<T> filter) {
                var comp = _filtersPool.Spawn(filter);
                comp.Setup(filter);
                AddDependentFilter(filter.DependsOn, filter);
                comp.FilterUpdatedEvent += HandleFilterUpdated;
                _container.Children.Add(comp);
            }

            public void RemoveFilter(IPanelListFilter<T> filter) {
                var comp = _filtersPool.SpawnedComponents[filter];
                comp.Setup(null);
                comp.Use();
                RemoveDependentFilter(filter.DependsOn, filter);
                comp.FilterUpdatedEvent -= HandleFilterUpdated;
                _filtersPool.Despawn(comp);
            }

            public void DisableAllFilters() {
                foreach (var (_, filter) in _filtersPool.SpawnedComponents) {
                    filter.FilterEnabled = false;
                }
            }

            #endregion

            #region Construct

            public IEnumerable<IPanelListFilter<T>> ActiveFilters => _filtersPool.SpawnedComponents
                .Where(static x => x.Value.FilterEnabled)
                .Select(static x => x.Key);

            private readonly ReactivePool<IPanelListFilter<T>, FilterContainer> _filtersPool = new();
            private Background _container = null!;

            protected override IReactiveComponent ConstructContent() {
                return new Background {
                    FlexController = {
                        FlexDirection = FlexDirection.Column,
                        JustifyContent = Justify.FlexStart,
                        AlignItems = Align.Stretch,
                        Padding = 2f,
                        Gap = 1f
                    },

                    FlexItem = {
                        Size = new() { x = 64.pt, y = 60.pt }
                    }
                }.AsBlurBackground().Bind(ref _container);
            }

            #endregion
        }

        #endregion

        #region Setup

        private void RefreshFiltersCaption() {
            _filtersContext.WithItemsText(
                _modal.ActiveFilters.Select(static x => x.FilterStatus)
            );
            _filtersText = _filtersContext.Text;
        }

        protected override void OnInitialize() {
            _modal.FilterUpdatedEvent += HandleFilterUpdated;
            _modal.PlacementAnchor = ContentTransform;
            _searchKeyboard.PlacementAnchor = _searchInputField.ContentTransform;

            _filters = new(
                new List<IPanelListFilter<T>>(),
                HandleFilterAdded,
                HandleFilterRemoved
            );

            _searchContext.AddCallback(this, static (x, y) => x.HandleSearchContextUpdated(y), lazy: true);
            _filtersContext.AddCallback(this, static (x, y) => x.HandleFiltersContextUpdated(y), lazy: true);
        }

        #endregion

        #region Construct

        private string SearchText => _searchContext.Text ?? string.Empty;

        private readonly BsInputFieldContext _searchContext = new();
        private readonly BsInputFieldContext _filtersContext = new();
        private string? _searchText;
        private string? _filtersText;

        private FiltersModal _modal = null!;
        private BsInputField _searchInputField = null!;
        private BsKeyboardModal _searchKeyboard = null!;

        protected override GameObject Construct() {
            return new Layout {
                FlexController = {
                    Gap = 1f
                },

                Children = {
                    new FiltersModal {
                        OnClickOutside = () => _modal.IsPushed = false,
                        PlacementData = new() { Placement = RelativePlacement.BottomRight, Offset = new(0f, 0.5f) }
                    }.Bind(ref _modal),
                    //
                    new BsInputField {
                        FlexItem = {
                            FlexGrow = 1f
                        },

                        Context = _searchContext,
                        Placeholder = "Search"
                    }.Bind(ref _searchInputField),

                    new BsKeyboardModal {
                        Context = _searchContext,
                        PlacementData = new() {
                            Placement = RelativePlacement.BottomCenter,
                            Offset = new(0f, 32f),
                            Clip = true
                        }
                    }.Bind(ref _searchKeyboard),
                    //filter panel
                    new BsInputField {
                        FlexItem = {
                            FlexGrow = 1f
                        },

                        Context = _filtersContext,
                        Placeholder = "Filters",
                        Icon = GameResources.Sprites.FilterIcon
                    }
                }
            }.Use();
        }

        #endregion

        #region Callbacks

        private void HandleSearchContextUpdated(BsInputFieldContext context) {
            if (context.Text == _searchText) return;
            _searchText = context.Text;
            FilterUpdatedEvent?.Invoke();
        }

        private void HandleFiltersContextUpdated(BsInputFieldContext context) {
            if (context.Focused) {
                _modal.IsPushed = true;
                context.Focused = false;
            }

            if (context.Text != _filtersText) {
                _filtersText = context.Text;
                if (string.IsNullOrEmpty(_filtersText)) {
                    _modal.DisableAllFilters();
                    FilterUpdatedEvent?.Invoke();
                }
            }
        }

        private void HandleFilterAdded(IPanelListFilter<T> filter) {
            _modal.AddFilter(filter);
        }

        private void HandleFilterRemoved(IPanelListFilter<T> filter) {
            _modal.RemoveFilter(filter);
        }

        private void HandleFilterUpdated(IPanelListFilter<T> filter) {
            FilterUpdatedEvent?.Invoke();
            RefreshFiltersCaption();
        }

        #endregion
    }
}