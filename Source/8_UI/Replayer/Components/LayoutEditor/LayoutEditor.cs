using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatLeader.Components {
    internal class LayoutEditor : ReeUIComponentV2 {
        public RectTransform EditorZone { get; private set; } = null!;
        public bool PartialDisplayEnabled { get; private set; }
        public bool EditorEnabled { get; private set; }

        public event Action<bool>? PartialDisplayModeStateWasChangedEvent;
        public event Action<bool>? EditModeStateWasChangedEvent;

        public KeyCode antiSnapKeyCode = KeyCode.LeftShift;
        public ILayoutMapsSource? layoutMapsSource;
        public ILayoutGridModel? layoutGridModel;

        #region Setup

        private readonly Dictionary<EditableElement, LayoutMap> _editablesWithMaps = new();
        private EditableElement? _selectedElement;
        private bool _wasApplied;

        public void SetEnabled(bool enabled = true) {
            if (EditorEnabled == enabled || PartialDisplayEnabled || EditorZone == null) return;
            foreach (var pair in _editablesWithMaps.ToList()) {
                var element = pair.Key;
                if (_wasApplied && !enabled) {
                    var map = element.TempLayoutMap;
                    _editablesWithMaps[element] = map;
                    layoutMapsSource?.OverrideLayoutMap(element, map);
                } else if (enabled || !_wasApplied) {
                    var map = pair.Value;
                    element.TempLayoutMap = map;
                    MapElement(element, map);
                }
                element.WrapperSelectionState = false;
                element.WrapperState = enabled;
            }
            if (enabled) {
                ReloadTable();
                RefreshGrid();
                HandleEditableWasSelected(null!);
            }
            SetWindowEnabled(enabled);
            SetGridEnabled(enabled);
            EditorEnabled = enabled;
            _wasApplied = false;
            EditModeStateWasChangedEvent?.Invoke(enabled);
        }
        public void SetPartialModeEnabled(bool enabled = true) {
            if (EditorEnabled) return;
            PartialDisplayEnabled = enabled;
            foreach (var pair in _editablesWithMaps) {
                var element = pair.Key;
                element.Root.gameObject.SetActive(!enabled || pair.Value.enabled);
            }
            PartialDisplayModeStateWasChangedEvent?.Invoke(enabled);
        }

        public void Setup(RectTransform zoneRect) {
            if (!IsParsed) return;
            EditorZone = zoneRect;
            _layoutWindow.boundsRect = zoneRect;
            SetupGrid();
            SetGridEnabled(false);
        }

        #endregion

        #region Editable Handling

        public void Add(params EditableElement[] elements) {
            foreach (var element in elements) {
                Add(element);
            }
        }

        public void Add(EditableElement element) {
            if (EditorEnabled && !_editablesWithMaps.ContainsKey(element)) return;
            //element.Root.SetParent(EditorZone);
            SubscribeEditable(element);
            _tableView.Add(element);
            var map = GetElementMap(element);
            _editablesWithMaps.TryAdd(element, map);
        }

        public void Remove(EditableElement element) {
            if (!_editablesWithMaps.Remove(element)) return;
            UnsubscribeEditable(element);
            _tableView.Remove(element);
            element.WrapperState = false;
            element.Root.gameObject.SetActive(false);
            //element.Root.SetParent(null);
        }

        private void SubscribeEditable(EditableElement element) {
            element.ElementWasSelectedEvent += HandleEditableWasSelected;
            element.ElementWasGrabbedEvent += HandleEditableWasGrabbed;
            element.ElementWasReleasedEvent += HandleEditableWasReleased;
            element.ElementDraggingEvent += HandleEditableDragging;
        }
        private void UnsubscribeEditable(EditableElement element) {
            element.ElementWasSelectedEvent -= HandleEditableWasSelected;
            element.ElementWasGrabbedEvent -= HandleEditableWasGrabbed;
            element.ElementWasReleasedEvent -= HandleEditableWasReleased;
            element.ElementDraggingEvent -= HandleEditableDragging;
        }

        #endregion

        #region Mapping

        public void ForceMapLayout() {
            if (EditorEnabled) return;
            foreach (var pair in _editablesWithMaps) {
                MapElement(pair.Key, pair.Value);
            }
        }

        private void MapElement(EditableElement element, LayoutMap map) {
            var content = element.Root;
            var contentSize = content.rect.size;
            var pos = MathUtils.ToAbsPos(map.position, EditorZone.rect.size);
            pos = LayoutMapper.ToActualPosition(pos, contentSize, map.anchor);
            content.localPosition = pos;
            content.SetSiblingIndex(map.layer);
            element.State = map.enabled;
        }

        private LayoutMap GetElementMap(EditableElement element) {
            if (_editablesWithMaps.TryGetValue(element, out var map)) {
                return map;
            }
            if (!(layoutMapsSource?.TryRequestLayoutMap(element, out map) ?? false)) {
                map = element.LayoutMap;
            }
            _editablesWithMaps[element] = map;
            return map;
        }

        #endregion

        #region UI Components

        [UIComponent("window")]
        private readonly RectTransform _window = null!;

        [UIComponent("window-handle")]
        private readonly RectTransform _windowHandle = null!;

        [UIValue("table-view")]
        private EditorTableView _tableView = null!;

        [UIValue("editable-controller")]
        private EditableController _editController = null!;

        private HandleHighlighter _handleHighlighter = null!;
        private BoundedLayoutWindow _layoutWindow = null!;
        private LayoutGrid _layoutGrid = null!;

        #endregion

        #region UI Window

        private void SetWindowEnabled(bool value) {
            Content.gameObject.SetActive(value);
        }

        protected override void OnInstantiate() {
            _tableView = Instantiate<EditorTableView>(transform);
            _editController = Instantiate<EditableController>(transform);
            _tableView.CellSelectedEvent += HandleCellSelected;
            _editController.TableReloadRequestedEvent += ReloadTable;
        }

        protected override void OnInitialize() {
            _layoutWindow = _windowHandle.gameObject.AddComponent<BoundedLayoutWindow>();
            _layoutWindow.Setup(_window, _windowHandle);
            _handleHighlighter = _windowHandle.gameObject.AddComponent<HandleHighlighter>();
            _handleHighlighter.Setup(Color.cyan, Color.yellow);
            SetWindowEnabled(false);
        }

        #endregion

        #region UI Grid

        private void SetupGrid() {
            _layoutGrid.TryDestroy();
            _layoutGrid = EditorZone.gameObject.AddComponent<LayoutGrid>();
            RefreshGrid();
        }

        private void SetGridEnabled(bool value) {
            _layoutGrid.enabled = value;
        }

        private void RefreshGrid() {
            _layoutGrid.LineThickness = layoutGridModel?.LineThickness ?? _layoutGrid.LineThickness;
            _layoutGrid.CellSize = layoutGridModel?.CellSize ?? _layoutGrid.CellSize;
            CoroutinesHandler.Instance.StartCoroutine(RefreshGridCoroutine());
        }

        private IEnumerator RefreshGridCoroutine() {
            yield return new WaitForEndOfFrame();
            _layoutGrid.Refresh();
        }

        #endregion

        #region TableView

        private void ReloadTable() {
            _tableView.ReloadTable();
            SelectTableCell(_selectedElement!);
        }

        private void SelectTableCell(EditableElement element) {
            _selectedElement = element;
            _tableView.SelectCell(element);
        }

        #endregion

        #region Callbacks

        [UIAction("apply-button-clicked")]
        private void HandleApplyButtonClicked() {
            _wasApplied = true;
            SetEnabled(false);
        }

        [UIAction("cancel-button-clicked")]
        private void HandleCancelButtonClicked() {
            SetEnabled(false);
        }

        private void HandleCellSelected(EditorTableCell model) {
            HandleEditableWasSelected(model.Element!);
        }

        private void HandleEditableWasSelected(EditableElement element) {
            if (_selectedElement != element && _selectedElement != null) {
                _selectedElement!.WrapperSelectionState = false;
            }
            _selectedElement = element;
            if (_selectedElement != null) {
                _selectedElement.WrapperSelectionState = true;
            }
            SelectTableCell(element);
            _editController.SetEditable(element);
        }

        private void HandleEditableDragging(Vector2 position) {
            var content = _selectedElement!.Root;
            var contentSize = content.rect.size;
            if (!Input.GetKey(antiSnapKeyCode)) {
                position = LayoutMapper.MapByGridUnclamped(position,
                    contentSize, _selectedElement.TempLayoutMap.anchor, _layoutGrid!);
            }
            content.localPosition = LayoutMapper
                .ClampInZone(position, contentSize, _layoutGrid.Size);
        }

        private void HandleEditableWasReleased() {
            var content = _selectedElement!.Root;
            var pos = LayoutMapper.ToAnchoredPosition(content.localPosition,
                content.rect.size, _selectedElement.TempLayoutMap.anchor);
            pos = MathUtils.ToRelPos(pos, EditorZone.rect.size);
            _selectedElement!.TempLayoutMap.position = pos;
        }

        private void HandleEditableWasGrabbed() {

        }

        #endregion
    }
}
