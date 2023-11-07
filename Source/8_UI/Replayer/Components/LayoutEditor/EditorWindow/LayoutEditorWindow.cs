using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeatLeader.Components {
    /// <summary>
    /// LayoutEditor controller which implemented as LayoutEditor component
    /// </summary>
    internal class LayoutEditorWindow : ReeUIComponentV3<LayoutEditorWindow>,
        ILayoutComponent,
        ILayoutComponentController,
        ILayoutComponentWrapperController {
        #region UI Components

        [UIComponent("components-list"), UsedImplicitly]
        private LayoutEditorComponentsList _layoutEditorComponentsList = null!;

        [UIComponent("window-handle"), UsedImplicitly]
        private Image _windowHandle = null!;

        [UIComponent("layer-up-button"), UsedImplicitly]
        private Button _layerUpButton = null!;

        [UIComponent("layer-down-button"), UsedImplicitly]
        private Button _layerDownButton = null!;

        [UIComponent("grid-alignment-button"), UsedImplicitly]
        private ImageButton _gridAlignmentButton = null!;

        private RectTransform _imageTransform = null!;

        #endregion

        #region LayoutComponent

        public ILayoutComponentHandler? ComponentHandler { get; private set; }
        ILayoutComponentWrapperController ILayoutComponent.WrapperController => this;
        ILayoutComponentController ILayoutComponent.ComponentController => this;

        public string ComponentName => "LayoutEditorWindow";

        private ILayoutEditor? _layoutEditor;
        private LayoutGrid? _layoutGrid;

        public void Setup(ILayoutComponentHandler? handler) {
            if (_layoutEditor is not null) {
                _layoutEditor.ComponentSelectedEvent -= HandleComponentSelectedExternal;
            }
            if (handler is not ILayoutEditor editor) {
                throw new InvalidOperationException(
                    "LayoutEditorWindow can be used only in LayoutEditor context"
                );
            }
            _layoutEditor = editor;
            _layoutEditor.ComponentSelectedEvent += HandleComponentSelectedExternal;
            _layoutGrid = _layoutEditor.AdditionalComponentHandler as LayoutGrid;
            ComponentHandler = handler;
            ContentTransform!.SetParent(handler?.AreaTransform);
        }

        #endregion

        #region ComponentController

        Vector2 ILayoutComponentController.ComponentPosition => ContentTransform!.localPosition;
        Vector2 ILayoutComponentController.ComponentSize => ComponentSizeInternal;
        Vector2 ILayoutComponentController.ComponentAnchor { get; } = Vector2.zero;

        int ILayoutComponentController.ComponentLayer {
            get => int.MaxValue;
            set { }
        }

        bool ILayoutComponentController.ComponentActive {
            get => true;
            set => throw new NotImplementedException();
        }

        private Vector2 ComponentSizeInternal => _imageTransform.rect.size;

        public void SetComponentActive(bool active) { }

        #endregion

        #region WrapperController

        public void SetWrapperActive(bool active) {
            Content!.SetActive(active);
            if (active) RefreshComponents();
        }

        public void SetWrapperSelected(bool selected) { }

        #endregion

        #region Movement

        private Vector2 _componentOriginPos;
        private Vector2 _componentPosOffset;
        private bool _isMoving;

        private void Update() {
            if (_isMoving) UpdateMovement();
        }

        private void UpdateMovement() {
            var componentDestinationPos = ComponentHandler!.PointerPosition + _componentPosOffset;
            if (componentDestinationPos == _componentOriginPos) return;
            ContentTransform!.localPosition = LayoutTool.ApplyBorders(
                componentDestinationPos, ComponentSizeInternal, ComponentHandler.AreaTransform!.rect.size
            );
        }

        #endregion

        #region Window Handle

        private static readonly Color handleBaseColor = Color.cyan;
        private static readonly Color handleHoverColor = Color.yellow;

        private bool _isWindowHandleHovered;

        private void RefreshWindowHandleColor(bool? hovered = null) {
            if (hovered.HasValue) _isWindowHandleHovered = hovered.Value;
            _windowHandle.color = _isWindowHandleHovered || _isMoving ? handleHoverColor : handleBaseColor;
        }

        #endregion

        #region Setup

        protected override void OnInitialize() {
            _layoutEditorComponentsList.ItemsWithIndexesSelectedEvent += HandleComponentsSelected;
            var eventsHandler = _windowHandle.gameObject.AddComponent<PointerEventsHandler>();
            eventsHandler.PointerUpEvent += OnComponentPointerUp;
            eventsHandler.PointerDownEvent += OnComponentPointerDown;
            eventsHandler.PointerEnterEvent += OnComponentPointerEnter;
            eventsHandler.PointerExitEvent += OnComponentPointerExit;
            Content!.GetComponent<LayoutGroup>().enabled = false;
            _imageTransform = (RectTransform)ContentTransform!.GetChild(0);
        }

        protected override bool OnValidation() => ComponentHandler is not null;

        #endregion

        #region ComponentsList

        private void RefreshComponents() {
            _layoutEditorComponentsList.items.Clear();
            _layoutEditorComponentsList.items.AddRange(_layoutEditor!.LayoutComponents);
            _layoutEditorComponentsList.items.Remove(this);
            _layoutEditorComponentsList.Refresh();
        }

        #endregion

        #region Component Handling

        private ILayoutComponent? _selectedComponent;

        private void RefreshLayerButtons() {
            var isComponentAvailable = _selectedComponent is null;
            var layer = _selectedComponent?.ComponentController.ComponentLayer;

            var selector = new Func<ILayoutComponent, int>(static x => x.ComponentController.ComponentLayer);
            var minLayer = isComponentAvailable ? 0 : _layoutEditorComponentsList.items.Min(selector);
            var maxLayer = isComponentAvailable ? 0 : _layoutEditorComponentsList.items.Max(selector);

            _layerUpButton.interactable = !isComponentAvailable && layer < maxLayer;
            _layerDownButton.interactable = !isComponentAvailable && layer > minLayer;
        }

        private void ModifySelectedComponentLayer(int layer) {
            if (_selectedComponent is null) return;
            _selectedComponent.ComponentController.ComponentLayer += layer;
            _layoutEditorComponentsList.Refresh();
            _layoutEditorComponentsList.Select(_selectedComponent);
            RefreshLayerButtons();
        }

        #endregion

        #region Callbacks

        private void HandleComponentSelected(ILayoutComponent? component) {
            _selectedComponent?.WrapperController.SetWrapperSelected(false);
            _selectedComponent = component;
            _selectedComponent?.WrapperController.SetWrapperSelected(true);
            RefreshLayerButtons();
        }

        private void HandleComponentSelectedExternal(ILayoutComponent? component) {
            HandleComponentSelected(component);
            if (component is null) _layoutEditorComponentsList.ClearSelection();
            else _layoutEditorComponentsList.Select(component);
        }

        private void HandleComponentsSelected(ICollection<int> indexes) {
            if (indexes.Count is 0) return;
            HandleComponentSelected(_layoutEditorComponentsList.items[indexes.First()]);
        }

        [UIAction("grid-button-clicked"), UsedImplicitly]
        private void HandleGridButtonClicked(bool state) {
            
        }

        [UIAction("layer-up-button-click"), UsedImplicitly]
        private void HandleLayerUpButtonClicked() {
            ModifySelectedComponentLayer(1);
        }

        [UIAction("layer-down-button-click"), UsedImplicitly]
        private void HandleLayerDownButtonClicked() {
            ModifySelectedComponentLayer(-1);
        }

        [UIAction("apply-button-click"), UsedImplicitly]
        private void HandleApplyButtonClicked() {
            _layoutEditor!.SetEditorActive(false, true);
        }

        [UIAction("cancel-button-click"), UsedImplicitly]
        private void HandleCancelButtonClicked() {
            _layoutEditor!.SetEditorActive(false, false);
        }

        #endregion

        #region PointerEventsHandler Callbacks

        private void OnComponentPointerUp(PointerEventsHandler handler, PointerEventData data) {
            _isMoving = false;
            RefreshWindowHandleColor();
        }

        private void OnComponentPointerDown(PointerEventsHandler handler, PointerEventData data) {
            ValidateAndThrow();
            var compPos = (Vector2)ContentTransform!.localPosition;
            var pointerPos = ComponentHandler!.PointerPosition;
            _componentPosOffset = compPos - pointerPos;
            _componentOriginPos = compPos;
            _isMoving = true;
            RefreshWindowHandleColor();
        }

        private void OnComponentPointerEnter(PointerEventsHandler handler, PointerEventData data) {
            RefreshWindowHandleColor(true);
        }

        private void OnComponentPointerExit(PointerEventsHandler handler, PointerEventData data) {
            RefreshWindowHandleColor(false);
        }

        #endregion
    }
}