using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using BeatLeader.Utils;
using IPA.Utilities;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ListFiltersPanel<T> : ReactiveComponent, ITextListFilter<T> {
        public interface IFilter : IReactiveComponent, ILayoutItem, IListFilter<T> {
            IEnumerable<IFilter>? DependsOn { get; }
            string FilterName { get; }
        }

        #region List

        public IFilteringListComponent<T>? List {
            get => _list;
            set {
                if (_list?.Filter == this) {
                    _list.Filter = _originalFilter;
                }
                _list = value;
                _originalFilter = _list?.Filter;
                if (_list != null) {
                    _list.Filter = this;
                }
            }
        }

        private IListFilter<T>? _originalFilter;
        private IFilteringListComponent<T>? _list;

        #endregion

        #region ListFilter

        public Func<T, IEnumerable<string>>? SearchContract { get; set; }

        public event Action? FilterUpdatedEvent;

        private readonly Dictionary<T, string> _matchedItems = new();

        bool IListFilter<T>.Matches(T value) {
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

        public ICollection<IFilter> Filters => _filters;

        private ObservableCollectionAdapter<IFilter> _filters = null!;

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

            public event Action<IFilter, bool>? FilterUpdatedEvent;

            private IFilter? _filter;

            public void Setup(IFilter? filter) {
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
                                    Sticky = true
                                }.WithStateListener(
                                    _ => HandleButtonClicked(false)
                                ).AsFlexItem(size: 3f).Bind(ref _enableButton)
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

            private readonly Dictionary<IFilter, List<IFilter>> _dependencies = new();

            private void AddDependentFilter(IEnumerable<IFilter>? hosts, IFilter dependent) {
                if (hosts == null) return;
                foreach (var host in hosts) {
                    _dependencies.EnsureExistsAndDo(
                        host,
                        new(),
                        x => x.Add(dependent)
                    );
                }
            }

            private void RemoveDependentFilter(IEnumerable<IFilter>? hosts, IFilter dependent) {
                if (hosts == null) return;
                foreach (var host in hosts) {
                    _dependencies.DoIfExists(
                        host,
                        x => x.Remove(dependent)
                    );
                }
            }

            private void HandleFilterUpdated(IFilter filter, bool state) {
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

            public event Action<IFilter>? FilterUpdatedEvent;

            public void AddFilter(IFilter filter) {
                var comp = _filtersPool.Spawn(filter);
                comp.AsFlexItem();
                comp.Setup(filter);
                AddDependentFilter(filter.DependsOn, filter);
                comp.FilterUpdatedEvent += HandleFilterUpdated;
                _container.Children.Add(comp);
            }

            public void RemoveFilter(IFilter filter) {
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

            public IEnumerable<IFilter> ActiveFilters => _filtersPool.SpawnedComponents
                .Where(static x => x.Value.FilterEnabled)
                .Select(static x => x.Key);

            private readonly ReactivePool<IFilter, FilterContainer> _filtersPool = new();
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
                    .WithRectSize(60f, 64f)
                    .Bind(ref _container)
                    .Use();
            }

            #endregion
        }

        private readonly FiltersModal _modal = new();

        private void OpenModal() {
            ModalSystemHelper.OpenModalRelatively(
                _modal,
                ContentTransform,
                ContentTransform,
                ModalSystemHelper.RelativePlacement.BottomRight,
                shadowSettings: new()
            );
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
            _modal.ModalAskedToBeClosedEvent += HandleModalClosed;
            _filters = new(
                new List<IFilter>(),
                HandleFilterAdded,
                HandleFilterRemoved
            );
        }

        #endregion

        #region Construct

        private InputField _searchInputField = null!;
        private TextArea _filtersTextArea = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new InputField {
                        Placeholder = "Search",
                        Icon = GameResources.Sprites.SearchIcon,
                        Keyboard = new KeyboardModal<Keyboard, InputField> {
                            PositionOffset = new(0f, 32f)
                        }
                    }.AsFlexItem(grow: 1f).WithListener(
                        x => x.Text,
                        _ => FilterUpdatedEvent?.Invoke()
                    ).Bind(ref _searchInputField),
                    //filter panel
                    new TextArea {
                        Placeholder = "Filters",
                        Icon = GameResources.Sprites.FilterIcon
                    }.WithListener(
                        x => x.Focused,
                        x => {
                            if (x) OpenModal();
                        }
                    ).WithListener(
                        x => x.Text,
                        x => {
                            if (!string.IsNullOrEmpty(x)) return;
                            _modal.DisableAllFilters();
                            FilterUpdatedEvent?.Invoke();
                        }
                    ).AsFlexItem(grow: 1f).Bind(ref _filtersTextArea)
                }
            }.AsFlexGroup(gap: 1f).Use();
        }

        #endregion

        #region Callbacks

        private void HandleFilterAdded(IFilter filter) {
            _modal.AddFilter(filter);
        }

        private void HandleFilterRemoved(IFilter filter) {
            _modal.RemoveFilter(filter);
        }

        private void HandleFilterUpdated(IFilter filter) {
            FilterUpdatedEvent?.Invoke();
            RefreshFiltersCaption();
        }

        private void HandleModalClosed() {
            _filtersTextArea.Focused = false;
        }

        #endregion
    }
}