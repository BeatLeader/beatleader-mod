using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.Components {
    internal interface ILayoutEditor {
        ILayoutGridModel LayoutGrid { get; }
        RectTransform EditorZone { get; }

        Vector2 Map(Vector2 pos, Vector2 size, Vector2 anchor);
    }

    internal class LayoutEditor : ReeUIComponentV2, ILayoutEditor {
        #region ILayoutEditor

        public bool EditModeEnabled { get; private set; }
        public RectTransform EditorZone { get; private set; }
        public ILayoutGridModel LayoutGrid => _layoutGrid;

        #endregion

        #region Events

        public event Action<bool> EditModeChangedEvent;

        #endregion

        #region Setup

        public ILayoutMapsSource layoutMapsSource;

        private readonly List<EditableElement> _editableElements = new();
        private EditableElement _selectedElement;
        private bool _wasApplied;

        public void SetEditorEnabled(bool enabled) {
            if (EditModeEnabled == enabled || EditorZone == null) return;
            foreach (var element in _editableElements) {
                if (enabled) {
                    element.tempLayoutMap = element.DefaultLayoutMap;
                    element.State = enabled;
                } else {
                    element.ApplyMap(_wasApplied ? element.tempLayoutMap : element.DefaultLayoutMap);
                }
                element.Wrapper.State = enabled;
            }
            if (enabled) {
                ReloadTable();
                RefreshGrid();
            } else 
                SaveLayout();
            SetWindowEnabled(enabled);
            SetGridEnabled(enabled);
            EditModeEnabled = enabled;
            _wasApplied = false;
            EditModeChangedEvent?.Invoke(enabled);
        }

        public void Setup(RectTransform zoneRect) {
            if (!IsParsed) return;
            EditorZone = zoneRect;
            SetupGrid();
            SetGridEnabled(false);
        }

        #endregion

        #region Editable Handling

        public void Add(EditableElement element) {
            if (EditModeEnabled || !IsParsed) return;
            _editableElements.Add(element);
            _tableView.Add(element);
            element.ElementPositionChangedEvent += HandleEditablePositionChanged;
            element.ElementSelectedEvent += HandleEditableSelected;
            element.Setup(this);
        }

        public void Remove(EditableElement element) {
            if (EditModeEnabled || !IsParsed || !_editableElements.Remove(element)) return;
            _tableView.Remove(element);
            element.ElementPositionChangedEvent -= HandleEditablePositionChanged;
            element.ElementSelectedEvent -= HandleEditableSelected;
            element.Setup(this);
        }

        #endregion

        #region Mapping

        public Vector2 Map(Vector2 pos, Vector2 size, Vector2 anchor) {
            pos = MathUtils.ToAbsPos(pos, EditorZone.rect.size);
            return LayoutTool.MapPosition(pos, size, anchor, _layoutGrid);
        }

        public void RefreshLayout() {
            if (EditModeEnabled || EditorZone == null) return;
            foreach (var editable in _editableElements) {
                var map = new LayoutMapData();
                if (!layoutMapsSource?.Maps.TryGetValue(editable.Name, out map) ?? true) {
                    map = editable.DefaultLayoutMap;
                }
                editable.ApplyMap(map);
            }
        }

        private void SaveLayout() {
            if (layoutMapsSource == null) return;
            foreach (var editable in _editableElements) {
                layoutMapsSource.Maps[editable.Name] = editable.DefaultLayoutMap;
            }
        }

        #endregion

        #region UI Components

        [UIComponent("window")]
        private readonly RectTransform _window;

        [UIComponent("window-handle")]
        private readonly RectTransform _windowHandle;

        [UIValue("table-view")]
        private EditorTableView _tableView;

        [UIValue("editable-controller")]
        private EditableController _editController;

        private HandleHighlighter _handleHighlighter;
        private GridLayoutWindow _layoutWindow;
        private LayoutGrid _layoutGrid;

        #endregion

        #region UI Setup

        private void SetWindowEnabled(bool value) {
            Content.gameObject.SetActive(value);
        }

        private void SetGridEnabled(bool value) {
            _layoutGrid.enabled = value;
        }

        protected override void OnInstantiate() {
            _tableView = Instantiate<EditorTableView>(transform);
            _editController = Instantiate<EditableController>(transform);
            _tableView.CellSelectedEvent += HandleCellSelected;
            _editController.TableReloadRequestedEvent += ReloadTable;
        }

        protected override void OnInitialize() {
            _layoutWindow = _windowHandle.gameObject.AddComponent<GridLayoutWindow>();
            _layoutWindow.Setup(_window, _windowHandle);
            _layoutWindow.adjustByGrid = false;
            _handleHighlighter = _windowHandle.gameObject.AddComponent<HandleHighlighter>();
            _handleHighlighter.Setup(Color.cyan, Color.yellow);
            SetWindowEnabled(false);
        }

        #endregion

        #region UI Grid

        public int gridCellSize;
        public int gridLineThickness;

        private void SetupGrid() {
            _layoutGrid?.TryDestroy();
            _layoutGrid = EditorZone.gameObject.AddComponent<LayoutGrid>();
            _layoutWindow.gridModel = _layoutGrid;
            RefreshGrid();
        }

        private void RefreshGrid() {
            _layoutGrid.LineThickness = gridLineThickness;
            _layoutGrid.CellSize = gridCellSize;
            CoroutinesHandler.instance.StartCoroutine(RefreshGridCoroutine());
        }

        private IEnumerator RefreshGridCoroutine() {
            yield return new WaitForEndOfFrame();
            _layoutGrid.Refresh();
        }

        #endregion

        #region TableView

        private void ReloadTable() {
            _tableView.ReloadTable();
            SelectTableCell(_selectedElement);
        }

        private void SelectTableCell(EditableElement element) {
            if (element == null) return;
            _selectedElement = element;
            _tableView.SelectCell(element);
        }

        #endregion

        #region Callbacks

        [UIAction("apply-button-clicked")]
        private void HandleApplyButtonClicked() {
            _wasApplied = true;
            SetEditorEnabled(false);
        }

        [UIAction("cancel-button-clicked")]
        private void HandleCancelButtonClicked() {
            SetEditorEnabled(false);
        }

        private void HandleCellSelected(EditorTableCell model) {
            HandleEditableSelected(model.Element);
        }

        private void HandleEditablePositionChanged(EditableElement element) {
            element.tempLayoutMap.position = MathUtils.ToRelPos(element.LastWindowCursorPos, EditorZone.rect.size);
        }

        private void HandleEditableSelected(EditableElement element) {
            if (_selectedElement != null) {
                _selectedElement.Wrapper.BgColor = GlassWrapper.DefaultColor;
            }
            SelectTableCell(element);
            _editController.SetEditable(element);
            element.Wrapper.BgColor = GlassWrapper.SelectedColor;
        }

        #endregion
    }
}
