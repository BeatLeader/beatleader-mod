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
    }

    internal class ListFiltersPanel<T> : ReactiveComponent, ITextTableFilter<T> {
        #region TableFilter

        public Func<T, IEnumerable<string>>? SearchContract { get; set; }

        public event Action? FilterUpdatedEvent;

        private readonly Dictionary<T, string> _matchedItems = new();

        bool ITableFilter<T>.Matches(T value) {
            var filterMatches = _modal.ActiveFilters.All(x => x.Matches(value));
            var text = _searchInputField.Text.ToLower();
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
            if (string.IsNullOrEmpty(_searchInputField.Text)) return null;
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
                get => _enableButton.Active;
                set {
                    _enableButton.Click(value);
                    HandleButtonClicked(true);
                }
            }

            public event Action<IPanelListFilter<T>, bool>? FilterUpdatedEvent;

            private IPanelListFilter<T>? _filter;

            public void Setup(IPanelListFilter<T>? filter) {
                if (_filter != null) {
                    _filterContainer.Children.Remove(_filter);
                    _filter.FilterUpdatedEvent -= HandleFilterUpdated;
                }
                _filter = filter;
                if (_filter != null) {
                    _filterContainer.Children.Add(_filter);
                    _filter.FilterUpdatedEvent += HandleFilterUpdated;
                    _filterContainer.Placeholder = _filter.FilterName;
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

            private PushContainer _filterContainer = null!;
            private ButtonBase _enableButton = null!;

            protected override GameObject Construct() {
                return new Dummy {
                    Children = {
                        new PushContainer {
                            BackgroundImage = {
                                Color = (Color.white * 0.9f).ColorWithAlpha(1f)
                            }
                        }.AsFlexGroup().AsFlexItem(grow: 1f).Bind(ref _filterContainer),
                        //
                        new Image {
                            Color = (Color.white * 0.9f).ColorWithAlpha(1f),
                            Children = {
                                new ImageButton {
                                    Image = {
                                        Sprite = BundleLoader.ProgressRingIcon
                                    },
                                    Latching = true,
                                    OnStateChanged = _ => HandleButtonClicked(false)
                                }.AsFlexItem(size: 3f).Bind(ref _enableButton)
                            }
                        }.AsFlexGroup(
                            alignItems: Align.Center,
                            padding: 1f
                        ).AsFlexItem(
                            size: "auto"
                        ).AsBlurBackground()
                    }
                }.AsFlexGroup(gap: 1f).Use();
            }

            #endregion
        }

        #endregion

        #region FiltersModal

        private class FiltersModal : ModalComponentBase {
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
                comp.AsFlexItem();
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
            private Image _container = null!;

            protected override GameObject Construct() {
                return new Image()
                    .AsFlexGroup(
                        direction: FlexDirection.Column,
                        justifyContent: Justify.FlexStart,
                        alignItems: Align.Center,
                        padding: 2f,
                        gap: 1f
                    )
                    .AsBlurBackground()
                    .WithSizeDelta(64f, 60f)
                    .Bind(ref _container)
                    .Use();
            }

            #endregion
        }

        #endregion

        #region Setup

        private void RefreshFiltersCaption() {
            _filtersTextArea.WithItemsText(
                _modal.ActiveFilters.Select(static x => x.FilterName)
            );
        }

        protected override void OnInitialize() {
            _modal.FilterUpdatedEvent += HandleFilterUpdated;
            _filters = new(
                new List<IPanelListFilter<T>>(),
                HandleFilterAdded,
                HandleFilterRemoved
            );
        }

        #endregion

        #region Construct

        private FiltersModal _modal = null!;
        private InputField _searchInputField = null!;
        private TextArea _filtersTextArea = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new FiltersModal()
                        .WithShadow()
                        .WithScaleAnimation()
                        .WithCloseListener(() => _filtersTextArea.Focused = false)
                        .WithAnchor(() => ContentTransform, RelativePlacement.BottomRight)
                        .Bind(ref _modal),
                    //
                    new InputField {
                            Placeholder = "Search",
                            Icon = GameResources.Sprites.SearchIcon,
                            Keyboard = new KeyboardModal<Keyboard, InputField> {
                                Offset = new(0f, 32f)
                            }
                        }
                        .AsFlexItem(grow: 1f)
                        .WithListener(
                            x => x.Text,
                            _ => FilterUpdatedEvent?.Invoke()
                        )
                        .Bind(ref _searchInputField),
                    //filter panel
                    new TextArea {
                            Placeholder = "Filters",
                            Icon = GameResources.Sprites.FilterIcon
                        }
                        .WithListener(
                            x => x.Focused,
                            x => {
                                if (x) _modal.Present(ContentTransform);
                            }
                        )
                        .WithListener(
                            x => x.Text,
                            x => {
                                if (!string.IsNullOrEmpty(x)) return;
                                _modal.DisableAllFilters();
                                FilterUpdatedEvent?.Invoke();
                            }
                        )
                        .AsFlexItem(grow: 1f)
                        .Bind(ref _filtersTextArea)
                }
            }.AsFlexGroup(gap: 1f).Use();
        }

        #endregion

        #region Callbacks

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