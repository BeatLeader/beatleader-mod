using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal enum SelectionMode {
        None,
        Single,
        Multiple
    }

    internal class Table<TItem, TCell> : ReactiveComponent, IModifiableTableComponent<TItem> where TCell : ITableCell<TItem>, IReactiveComponent, new() {
        #region Props

        public ScrollOrientation ScrollOrientation {
            get => _scrollArea.ScrollOrientation;
            set => _scrollArea.ScrollOrientation = value;
        }

        public IScrollbar? Scrollbar {
            get => _scrollArea.Scrollbar;
            set => _scrollArea.Scrollbar = value;
        }

        public string EmptyText {
            get => EmptyLabel.Text;
            set => EmptyLabel.Text = value;
        }

        public int? ScrollbarScrollSize {
            get => (int?)_scrollArea.ScrollbarScrollSize;
            set => _scrollArea.ScrollbarScrollSize = value.HasValue ? value * CellSize : null;
        }

        public Label EmptyLabel { get; private set; } = null!;

        private void RefreshVisibility() {
            RefreshScrollbar();
            RefreshEmptyText();
        }

        private void RefreshScrollbar() {
            var averageCellsCount = ViewportSize / CellSize;
            var cellCount = Mathf.CeilToInt(averageCellsCount);
            _scrollArea.Scrollbar?.SetActive(_filteredItems.Count > cellCount);
        }

        private void RefreshEmptyText() {
            var visible = _filteredItems.Count > 0;
            _scrollArea.Enabled = visible;
            EmptyLabel.Enabled = !visible;
        }

        #endregion

        #region Filter

        public ITableFilter<TItem>? Filter {
            get => _filter;
            set {
                if (_filter != null) {
                    _filter.FilterUpdatedEvent -= HandleFilterUpdated;
                }
                _filter = value;
                if (_filter != null) {
                    _filter.FilterUpdatedEvent += HandleFilterUpdated;
                }
            }
        }

        private readonly List<TItem> _filteredItems = new();
        private ITableFilter<TItem>? _filter;

        private void HandleFilterUpdated() {
            Refresh();
        }

        private void RefreshFilter() {
            _filteredItems.Clear();
            if (_filter == null) {
                _filteredItems.AddRange(Items);
                return;
            }
            foreach (var item in Items) {
                if (!_filter.Matches(item)) continue;
                _filteredItems.Add(item);
            }
            NotifyPropertyChanged(nameof(FilteredItems));
        }

        #endregion

        #region Table

        /// <summary>
        /// A collection of added items.
        /// </summary>
        public IList<TItem> Items {
            get {
                lock (_itemsLocker) {
                    return _items;
                }
            }
        }

        /// <summary>
        /// A collection of items which will be actually displayed in the table.
        /// </summary>
        public IReadOnlyList<TItem> FilteredItems => _filteredItems;

        /// <summary>
        /// A collection of selected indexes.
        /// </summary>
        public IReadOnlyCollection<int> SelectedIndexes => _selectedIndexes;

        /// <summary>
        /// An enum that determines how many cells you can select.
        /// </summary>
        public SelectionMode SelectionMode {
            get => _selectionMode;
            set {
                _selectionMode = value;
                ClearSelection();
            }
        }

        IReadOnlyList<TItem> ITableComponent<TItem>.Items {
            get {
                lock (_itemsLocker) {
                    return _items;
                }
            }
        }

        private readonly object _itemsLocker = new();
        private readonly List<TItem> _items = new();
        private readonly HashSet<int> _selectedIndexes = new();
        private SelectionMode _selectionMode = SelectionMode.Single;

        public void Refresh(bool clearSelection = true) {
            OnEarlyRefresh();
            RefreshFilter();
            RefreshContentSize();
            RefreshVisibleCells(0f);
            ScrollContentIfNeeded();
            RefreshVisibility();
            if (clearSelection) ClearSelection();
            OnRefresh();
        }

        public void ScrollTo(int idx, bool animated = true) {
            _scrollArea.ScrollTo(CellSize * idx);
        }

        public void Select(int idx) {
            if (SelectionMode is SelectionMode.None) return;
            //
            if (SelectionMode is SelectionMode.Single && _selectedIndexes.Count > 0) {
                _selectedIndexes.Clear();
            }
            _selectedIndexes.Add(idx);
            ForceRefreshVisibleCells();
            NotifyPropertyChanged(nameof(SelectedIndexes));
        }

        public void ClearSelection(int idx = -1) {
            if (idx != -1) {
                _selectedIndexes.Remove(idx);
            } else {
                _selectedIndexes.Clear();
            }
            ForceRefreshVisibleCells();
            NotifyPropertyChanged(nameof(SelectedIndexes));
        }

        public void ScrollTo(TItem item, bool animated = true) {
            var index = FindIndex(item);
            ScrollTo(index, animated);
        }

        public void Select(TItem item) {
            var index = FindIndex(item);
            Select(index);
        }

        public void ClearSelection(TItem item) {
            var index = FindIndex(item);
            ClearSelection(index);
        }

        private int FindIndex(TItem item) {
            return _filteredItems.FindIndex(x => x!.Equals(item));
        }

        #endregion

        #region Cells

        protected float CellSize => ScrollOrientation is ScrollOrientation.Vertical ? _cellSize.y : _cellSize.x;

        private readonly ReactivePool<TCell> _cellsPool = new() { DetachOnDespawn = false };
        private readonly Dictionary<ITableCell<TItem>, int> _cachedIndexes = new();
        private bool _selectionRefreshNeeded;
        private Vector2 _cellSize;
        private float _contentPos;
        private float _destinationPos;
        private int _visibleCellsCount;
        private int _visibleCellsStartIndex;
        private int _visibleCellsEndIndex;

        private void PlaceCell(Transform transform, int index) {
            if (ScrollOrientation is ScrollOrientation.Vertical) {
                index++;
                transform.localPosition = new(0f, index * -CellSize);
            } else {
                transform.localPosition = new(index * -CellSize, 0f);
            }
        }

        private void AlignCell(RectTransform transform) {
            if (ScrollOrientation is ScrollOrientation.Vertical) {
                transform.anchorMin = new(0f, 1f);
                transform.anchorMax = new(1f, 1f);
                transform.sizeDelta = new(0f, transform.sizeDelta.y);
                transform.pivot = new(1f, 0f);
            } else {
                transform.anchorMin = new(0f, 0f);
                transform.anchorMax = new(0f, 1f);
                transform.sizeDelta = new(transform.sizeDelta.x, 0f);
                transform.pivot = new(1f, 1f);
            }
        }

        private void CalculateVisibleCellsRange(float pos) {
            //start index
            var start = Mathf.FloorToInt(pos / CellSize);
            start = start < 0 ? 0 : start;
            //end index
            var end = start + _visibleCellsCount;
            end = end > FilteredItems.Count ? FilteredItems.Count : end;
            //clear cached cells if needed
            if (start != _visibleCellsStartIndex || end != _visibleCellsEndIndex) {
                _selectionRefreshNeeded = true;
                _cachedIndexes.Clear();
            }
            //setting values
            _visibleCellsStartIndex = start;
            _visibleCellsEndIndex = end;
        }

        private void RefreshVisibleCells(float pos) {
            CalculateVisibleCellsRange(pos);
            int i;
            for (i = _visibleCellsStartIndex; i < _visibleCellsEndIndex; i++) {
                //spawning and initializing
                var item = _filteredItems[i];
                var cell = GetOrSpawnCell(i - _visibleCellsStartIndex);
                cell.Init(item);
                OnCellConstruct(cell);
                //updating state
                if (_selectionRefreshNeeded) {
                    var selected = _selectedIndexes.Contains(i);
                    cell.OnCellStateChange(selected);
                }
                //placing and saving
                PlaceCell(cell.ContentTransform, i);
                _cachedIndexes[cell] = i;
            }
            //despawning redundant cells
            for (i -= _visibleCellsStartIndex; i < _cellsPool.SpawnedComponents.Count; i++) {
                var cell = _cellsPool.SpawnedComponents.Last();
                cell.CellAskedToChangeSelectionEvent -= HandleCellWantsToChangeSelection;
                _cellsPool.Despawn(cell);
            }
        }

        private void RefreshVisibleCells() {
            RefreshVisibleCells(_contentPos);
        }

        private void ForceRefreshVisibleCells() {
            _selectionRefreshNeeded = true;
            RefreshVisibleCells(_contentPos);
        }

        private TCell GetOrSpawnCell(int index) {
            if (_cellsPool.SpawnedComponents.Count - 1 < index) {
                var cell = _cellsPool.Spawn();
                cell.Use(_scrollContent);
                cell.CellAskedToChangeSelectionEvent += HandleCellWantsToChangeSelection;
                AlignCell(cell.ContentTransform);
                return cell;
            }
            return _cellsPool.SpawnedComponents[index];
        }

        #endregion

        #region Abstraction

        protected IEnumerable<KeyValuePair<TCell, TItem>> SpawnedCells => _cachedIndexes
            .Select(pair => new KeyValuePair<TCell, TItem>((TCell)pair.Key, _filteredItems[pair.Value]));

        protected virtual void OnEarlyRefresh() { }
        protected virtual void OnRefresh() { }
        protected virtual void OnCellConstruct(TCell cell) { }

        #endregion

        #region Content

        private float ContentSize => ScrollOrientation is ScrollOrientation.Vertical ? _scrollContent.rect.height : _scrollContent.rect.width;
        private float ViewportSize => ScrollOrientation is ScrollOrientation.Vertical ? _viewport.rect.height : _viewport.rect.width;

        private void RefreshContentSize() {
            if (ScrollOrientation is ScrollOrientation.Vertical) {
                _scrollContent.sizeDelta = new(0f, FilteredItems.Count * CellSize);
            } else {
                _scrollContent.sizeDelta = new(FilteredItems.Count * CellSize, 0f);
            }
        }

        private void ScrollContentIfNeeded() {
            var needScrollToEnd = ContentSize - _contentPos <= ViewportSize;
            var needScrollToStart = _contentPos < 0f || _filteredItems.Count <= (int)(ViewportSize / CellSize);
            if (needScrollToEnd) {
                _scrollArea.ScrollToEnd(true);
            } else if (needScrollToStart) {
                _scrollArea.ScrollToStart(true);
            }
        }

        protected override void OnRectDimensionsChanged() {
            var averageCellsCount = ViewportSize / CellSize;
            var cellCount = Mathf.CeilToInt(averageCellsCount);
            //adding because we need two more cells to fill the free space when scrolling
            _visibleCellsCount = cellCount + 1;
            CalculateVisibleCellsRange(_contentPos);
            RefreshVisibleCells(_contentPos);
        }

        #endregion

        #region Construct

        protected RectTransform ScrollContent => _scrollContent;

        private RectTransform _scrollContent = null!;
        private RectTransform _viewport = null!;
        private ScrollArea _scrollArea = null!;

        protected sealed override GameObject Construct() {
            //constructing
            var content = new Dummy {
                Children = {
                    //area
                    new ScrollArea {
                            ScrollContent = new Dummy().Bind(ref _scrollContent)
                        }
                        .WithRectExpand()
                        .Bind(ref _viewport)
                        .Bind(ref _scrollArea),
                    //empty label
                    new Label {
                        Text = "The monkey left you on your own!",
                        FontSize = 3.2f,
                        FontSizeMin = 1f,
                        FontSizeMax = 5f,
                        EnableAutoSizing = true,
                        EnableWrapping = true
                    }.WithRectExpand().Export(out var label)
                }
            };
            //initializing here instead of OnInitialize to leave it for inheritors
            EmptyLabel = label;
            RefreshEmptyText();
            var cell = _cellsPool.Spawn();
            _cellSize = cell.ContentTransform.rect.size;
            _scrollArea.ScrollSize = CellSize;
            ScrollbarScrollSize = 4;
            _cellsPool.Despawn(cell);
            _scrollArea.ScrollPosChangedEvent += HandlePosChanged;
            _scrollArea.ScrollDestinationPosChangedEvent += HandleDestinationPosChanged;
            _scrollArea.ScrollWithJoystickFinishedEvent += HandleJoystickScrollFinished;
            //returning
            return content.Use();
        }

        #endregion

        #region Callbacks

        private void HandleCellWantsToChangeSelection(ITableCell<TItem> cell, bool selected) {
            var idx = _cachedIndexes[cell];
            if (selected) {
                Select(idx);
            } else {
                ClearSelection(idx);
            }
        }

        private void HandleJoystickScrollFinished() {
            _destinationPos = MathUtils.RoundStepped(_destinationPos, CellSize);
            _scrollArea.ScrollTo(_destinationPos);
        }

        private void HandleDestinationPosChanged(float pos) {
            _destinationPos = pos;
        }

        private void HandlePosChanged(float pos) {
            _contentPos = pos;
            RefreshVisibleCells();
        }

        #endregion
    }
}