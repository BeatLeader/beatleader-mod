using System;
using System.Collections.Generic;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Tags;
using HMUI;
using IPA.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BeatLeader.UI.Reactive.Components {
    /// <summary>
    /// Abstraction for lists
    /// </summary>
    internal interface IListComponent {
        void Refresh(bool clearSelection = true);

        void ScrollTo(int idx, bool animated = true);

        void Select(int idx);

        /// <summary>
        /// Clears selected cell with the specified index
        /// </summary>
        /// <param name="idx">Index of the cell. -1 for all cells</param>
        void ClearSelection(int idx = -1);
    }

    /// <summary>
    /// Abstraction for lists with value
    /// </summary>
    /// <typeparam name="TItem">Data type</typeparam>
    internal interface IListComponent<TItem> : IListComponent {
        IReadOnlyList<TItem> Items { get; }

        void ScrollTo(TItem item, bool animated = true);

        void Select(TItem item);

        void ClearSelection(TItem item);
    }

    /// <summary>
    /// Abstraction for modifiable lists with value
    /// </summary>
    /// <typeparam name="TItem">Data type</typeparam>
    internal interface IModifiableListComponent<TItem> : IListComponent<TItem> {
        new IList<TItem> Items { get; }
    }

    /// <summary>
    /// Cell base for <c>ListComponentBase</c>
    /// </summary>
    internal abstract class ListComponentBaseCell : TableCell {
        public event Action? LateSelectionDidChangeEvent;

        private new void Start() {
            if (GetComponent<Graphic>() == null) {
                gameObject.AddComponent<Touchable>();
            }
        }

        protected sealed override void InternalToggle() {
            base.InternalToggle();
            LateSelectionDidChangeEvent?.Invoke();
        }
    }

    /// <summary>
    /// Universal ReactiveComponent base for lists
    /// </summary>
    /// <typeparam name="TItem">Data type</typeparam>
    /// TODO: migrate all remaining lists to reactive list and remove this BEFORE MERGE!!
    internal abstract class ListComponentBase<TItem> : ReactiveComponent,
        TableView.IDataSource,
        IModifiableListComponent<TItem>,
        IFilteringListComponent<TItem> {
        #region Events

        public event Action<ICollection<int>>? ItemsWithIndexesSelectedEvent;

        private void NotifySelectedItemsChanged(ICollection<int> items) {
            ItemsWithIndexesSelectedEvent?.Invoke(items);
            NotifyPropertyChanged(nameof(SelectedItemsIndexes));
            NotifyPropertyChanged(nameof(SelectedItems));
        }

        #endregion

        #region DataSource

        float TableView.IDataSource.CellSize() => CellSize;

        int TableView.IDataSource.NumberOfCells() => VisibleItems.Count;

        TableCell TableView.IDataSource.CellForIdx(TableView tableView, int idx) {
            //since TableView does not notify us when cells are unselected, we need to do it in that hacky-wacky way
            var cell = ConstructCell(VisibleItems[idx]);
            cell.LateSelectionDidChangeEvent -= HandleLateSelectionChanged;
            cell.LateSelectionDidChangeEvent += HandleLateSelectionChanged;
            return cell;
        }

        #endregion

        #region Filtering

        public IListFilter<TItem>? Filter {
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

        public IReadOnlyList<TItem> VisibleItems => _visibleItems;

        private readonly List<TItem> _visibleItems = new();
        private IListFilter<TItem>? _filter;

        private void HandleFilterUpdated() {
            Refresh();
        }

        //TODO: optimize using chunked filtering
        private void RefreshFilter() {
            _visibleItems.Clear();
            if (_filter == null) {
                _visibleItems.AddRange(Items);
                return;
            }
            foreach (var item in Items) {
                if (!_filter.Matches(item)) continue;
                _visibleItems.Add(item);
                OnItemMatched(item);
            }
        }

        protected virtual void OnItemMatched(TItem item) { }

        #endregion

        #region ModifiableListComponent

        IList<TItem> IModifiableListComponent<TItem>.Items => Items;
        IReadOnlyList<TItem> IListComponent<TItem>.Items => Items;

        public void ScrollTo(TItem item, bool animated = true) {
            var idx = FindItemIndex(item);
            if (idx is -1) return;
            ScrollTo(idx, animated);
        }

        public void Select(TItem item) {
            Select(FindItemIndex(item));
        }

        public void ClearSelection(TItem item) {
            var idx = -1;
            if (CellSelectionType is TableViewSelectionType.Multiple) {
                idx = FindItemIndex(item);
                if (idx is -1) return;
            }
            ClearSelection(idx);
        }

        private int FindItemIndex(TItem item) {
            return Items.FindIndex(x => x!.Equals(item));
        }

        #endregion

        #region UI Components

        private TableView _tableView = null!;
        private ScrollView _scrollView = null!;

        private GameObject _listObject = null!;
        private GameObject _emptyTextObject = null!;

        #endregion

        #region TableView

        public List<TItem> Items { get; } = new();
        public IEnumerable<TItem> SelectedItems => Items.TakeIndexes(SelectedItemsIndexes);
        public IReadOnlyCollection<int> SelectedItemsIndexes => _selectedCellIndexes;

        public void ScrollTo(int idx, bool animated = true) {
            _tableView.ScrollToCellWithIdx(idx, TableView.ScrollPositionType.Center, animated);
        }

        public void Refresh(bool clearSelection = true) {
            OnEarlyRefresh();
            RefreshFilter();
            if (clearSelection) {
                ClearSelectionInternal(-1);
                _tableView.ReloadData();
            } else _tableView.ReloadDataKeepingPosition();
            ShowEmptyScreen(VisibleItems.Count is 0);
            OnRefresh();
            if (clearSelection) {
                NotifySelectedItemsChanged(Array.Empty<int>());
            }
        }

        public void Select(int idx) {
            //TODO: reimplement since it clears other selected cells
            _tableView.SelectCellWithIdx(idx, true);
        }

        public void ClearSelection(int idx = -1) {
            ClearSelectionInternal(idx);
            Refresh(false);
            NotifySelectedItemsChanged(Array.Empty<int>());
        }

        protected TableCell? DequeueReusableCell(string identifier) {
            return _tableView.DequeueReusableCellForIdentifier(identifier);
        }

        private void ClearSelectionInternal(int idx) {
            if (CellSelectionType is TableViewSelectionType.None) return;
            if (idx is not -1 && CellSelectionType is TableViewSelectionType.Multiple) _selectedCellIndexes.Remove(idx);
            else _tableView.ClearSelection();
        }

        private void ShowEmptyScreen(bool show) {
            _listObject.SetActive(!show);
            _emptyTextObject.SetActive(show);
            _scrollbar?.SetActive(!show);
        }

        #endregion

        #region Abstraction

        protected virtual TableViewSelectionType CellSelectionType => TableViewSelectionType.Single;

        protected virtual ScrollView.ScrollViewDirection ScrollDirection => ScrollView.ScrollViewDirection.Vertical;

        protected virtual string EmptyText => "The monkey left you on your own!";

        protected abstract float CellSize { get; }

        protected abstract ListComponentBaseCell ConstructCell(TItem data);

        protected virtual void OnRefresh() { }

        protected virtual void OnEarlyRefresh() { }

        #endregion

        #region Scrollbar

        public IScrollbar? Scrollbar {
            get => _scrollbar;
            set {
                //if (_scrollbar is not null) _scrollbar.ScrollEvent -= HandleScrollbarScroll;
                _scrollbar = value;
                //if (_scrollbar is not null) _scrollbar.ScrollEvent += HandleScrollbarScroll;
            }
        }

        private IScrollbar? _scrollbar;

        private void UpdateScrollbar(float pos) {
            if (_scrollbar is null) return;
            //TODO: asm pub; pub ver:
            //var contentRect = _scrollView._contentRectTransform.rect;
            //var contentRect = _scrollView._contentRectTransform.rect;
            //var viewportRect = _scrollView._viewport.rect;
            //_scrollbar.PageHeight = ScrollDirection is ScrollView.ScrollViewDirection.Vertical ?
            //    viewportRect.height / contentRect.height :
            //    viewportRect.width / contentRect.width;
            //var progress = pos / (_scrollView.contentSize - _scrollView.scrollPageSize);
            //_scrollbar.Progress = progress;
            //_scrollbar.CanScrollUp = _scrollView._destinationPos > 1f / 1000;
            //_scrollbar.CanScrollDown = _scrollView._destinationPos < _scrollView.contentSize - _scrollView.scrollPageSize - 1f / 1000;

            var contentRect = _scrollView.GetField<RectTransform, ScrollView>("_contentRectTransform").rect;
            var viewportRect = _scrollView.GetField<RectTransform, ScrollView>("_viewport").rect;
            _scrollbar.PageHeight = ScrollDirection is ScrollView.ScrollViewDirection.Vertical ?
                viewportRect.height / contentRect.height :
                viewportRect.width / contentRect.width;
            var progress = pos / (_scrollView.GetProperty<float, ScrollView>("contentSize") -
                _scrollView.GetProperty<float, ScrollView>("scrollPageSize"));
            _scrollbar.Progress = progress;
            _scrollbar.CanScrollUp = _scrollView.GetField<float, ScrollView>("_destinationPos") > 1f / 1000;
            _scrollbar.CanScrollDown = _scrollView.GetField<float, ScrollView>("_destinationPos")
                < _scrollView.GetProperty<float, ScrollView>("contentSize")
                - _scrollView.GetProperty<float, ScrollView>("scrollPageSize") - 1f / 1000;
        }

        private void HandleScrollbarScroll(ScrollView.ScrollDirection scrollDirection) {
            switch (scrollDirection) {
                case ScrollView.ScrollDirection.Up:
                    _scrollView.PageUpButtonPressed();
                    break;
                case ScrollView.ScrollDirection.Down:
                    _scrollView.PageDownButtonPressed();
                    break;
            }
        }

        #endregion

        #region Setup

        // ReSharper disable once StaticMemberInGenericType
        private static readonly CustomListTag customListTag = new();

        private HashSet<int> _selectedCellIndexes = null!;

        protected override void Construct(RectTransform rect) {
            _listObject = customListTag.CreateObject(rect);
            var listTransform = _listObject.GetComponent<RectTransform>();
            listTransform.WithRectExpand();
            var tableData = _listObject.GetComponent<CustomCellListTableData>();

            _emptyTextObject = new Label {
                FontSize = 3.2f,
                Text = EmptyText,
                Alignment = TextAlignmentOptions.Center,
                FontSizeMin = 2f,
                FontSizeMax = 5f,
                EnableWrapping = true,
                EnableAutoSizing = true
            }.WithRectExpand().Use(rect);

            _tableView = tableData.tableView;
            _tableView.SetDataSource(this, true);
            _tableView.selectionType = CellSelectionType;
            Object.Destroy(tableData);

            //TODO: asm pub; pub ver:
            //_scrollView = _tableView._scrollView;
            //_scrollView.SetField("_platformHelper", BeatSaberUI.PlatformHelper);
            //_scrollView._scrollViewDirection = ScrollDirection;
            //_scrollView.scrollPositionChangedEvent += UpdateScrollbar;
            //_tableView._tableType = ScrollDirection switch {
            //    ScrollView.ScrollViewDirection.Horizontal => TableView.TableType.Horizontal,
            //    _ => TableView.TableType.Vertical
            //};
            //_selectedCellIndexes = _tableView._selectedCellIdxs;
            //_tableView.didSelectCellWithIdxEvent += HandleCellWithIndexSelected;
            //_tableView.gameObject.SetActive(true);
            //ShowEmptyScreen(false);

            _scrollView = _tableView.GetField<ScrollView, TableView>("_scrollView");
            _scrollView.gameObject.AddComponent<GraphicRaycaster>();
            _scrollView.SetField("_platformHelper", BeatSaberUI.PlatformHelper);
            _scrollView.SetField("_scrollViewDirection", ScrollDirection);
            _scrollView.scrollPositionChangedEvent += UpdateScrollbar;

            _tableView.SetField(
                "_tableType",
                ScrollDirection switch {
                    ScrollView.ScrollViewDirection.Horizontal => TableView.TableType.Horizontal,
                    _ => TableView.TableType.Vertical
                }
            );
            _selectedCellIndexes = _tableView.GetField<HashSet<int>, TableView>("_selectedCellIdxs");
            _tableView.gameObject.SetActive(true);
            ShowEmptyScreen(true);
        }

        #endregion

        #region Callbacks

        private void HandleLateSelectionChanged() {
            NotifySelectedItemsChanged(_selectedCellIndexes);
        }

        #endregion
    }
}