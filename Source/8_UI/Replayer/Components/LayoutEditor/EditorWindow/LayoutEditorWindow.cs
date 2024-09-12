using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.UI.Reactive.Components;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    /// <summary>
    /// LayoutEditor controller which implemented as LayoutEditor component
    /// </summary>
    internal class LayoutEditorWindow : ReactiveComponent,
        ILayoutComponent,
        ILayoutComponentController,
        ILayoutComponentWrapperController {
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
            ContentTransform.SetParent(handler?.AreaTransform);
        }

        void ILayoutComponent.RequestRefresh() { }

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

        protected override void OnUpdate() {
            if (_isMoving) UpdateMovement();
        }

        private void UpdateMovement() {
            var componentDestinationPos = ComponentHandler!.PointerPosition + _componentPosOffset;
            if (componentDestinationPos == _componentOriginPos) return;
            ContentTransform!.localPosition = LayoutTool.ApplyBorders(
                componentDestinationPos,
                ComponentSizeInternal,
                ComponentHandler.AreaTransform!.rect.size
            );
        }

        #endregion

        #region Window Handle

        private static readonly Color handleBaseColor = Color.cyan;
        private static readonly Color handleHoverColor = Color.yellow;

        private bool _isWindowHandleHovered;

        private void RefreshWindowHandleColor(bool? hovered = null) {
            if (hovered.HasValue) _isWindowHandleHovered = hovered.Value;
            _windowHandle.Color = _isWindowHandleHovered || _isMoving ? handleHoverColor : handleBaseColor;
        }

        #endregion

        #region Setup

        private LayoutEditorComponentsList _layoutEditorComponentsList = null!;
        private Image _windowHandle = null!;
        private ButtonBase _layerUpButton = null!;
        private ButtonBase _layerDownButton = null!;
        private RectTransform _imageTransform = null!;

        protected override GameObject Construct() {
            return new Image {
                Sprite = BundleLoader.Sprites.background,
                PixelsPerUnit = 7f,
                Color = new(0.14f, 0.14f, 0.14f),
                Children = {
                    //handle
                    new Image {
                        Sprite = BundleLoader.Sprites.backgroundTop,
                        PixelsPerUnit = 7f,
                        Children = {
                            new Label {
                                Text = "Layout Editor",
                                FontSize = 3f,
                                Color = Color.black
                            }.AsFlexItem(
                                size: new() { x = "auto" },
                                margin: new() { left = 2f }
                            )
                        }
                    }.Bind(ref _windowHandle).AsFlexGroup(
                        justifyContent: Justify.FlexStart
                    ).AsFlexItem(basis: 4f),
                    //list
                    new Dummy {
                        Children = {
                            new LayoutEditorComponentsList()
                                .Bind(ref _layoutEditorComponentsList)
                                .AsFlexItem(grow: 1f),
                        }
                    }.AsFlexGroup(padding: 1f).AsFlexItem(grow: 1f),
                    //buttons
                    new Dummy {
                        Children = {
                            new Image {
                                Sprite = BundleLoader.Sprites.background,
                                Color = new(0.22f, 0.22f, 0.22f),
                                PixelsPerUnit = 10f,
                                Children = {
                                    //layer buttons
                                    new Dummy {
                                        Children = {
                                            //layer up button
                                            new BsButton {
                                                    Skew = 0f,
                                                    OnClick = HandleLayerUpButtonClicked
                                                }
                                                .WithLabel("+")
                                                .AsFlexItem(basis: 7f)
                                                .WithAccentColor(Color.red)
                                                .Bind(ref _layerUpButton),
                                            //layer down button

                                            new BsButton {
                                                    Skew = 0f,
                                                    OnClick = HandleLayerDownButtonClicked
                                                }
                                                .WithLabel("-")
                                                .AsFlexItem(basis: 7f)
                                                .WithAccentColor(Color.blue)
                                                .Bind(ref _layerDownButton),
                                        }
                                    }.AsFlexGroup(
                                        direction: FlexDirection.Column,
                                        gap: 0.5f
                                    ).AsFlexItem(grow: 1f),
                                    //exit & apply buttons
                                    new Dummy {
                                        Children = {
                                            //cancel button
                                            new BsButton {
                                                    Skew = 0f,
                                                    OnClick = () => _layoutEditor!.SetEditorActive(false)
                                                }
                                                .WithLabel("Cancel")
                                                .AsFlexItem(basis: 7f),
                                            //apply button
                                            new BsPrimaryButton {
                                                    Skew = 0f,
                                                    OnClick = () => _layoutEditor!.SetEditorActive(false, true)
                                                }
                                                .WithLabel("Apply")
                                                .AsFlexItem(basis: 7f)
                                        }
                                    }.AsFlexGroup(
                                        direction: FlexDirection.Column,
                                        padding: new() { left = 2f },
                                        gap: 0.5f
                                    ).AsFlexItem(grow: 2f)
                                }
                            }.AsFlexGroup(
                                padding: 2f
                            ).AsFlexItem(
                                grow: 1f,
                                size: new() { x = 7f }
                            )
                        }
                    }.AsFlexGroup(padding: 1f).AsFlexItem(basis: 18f)
                }
            }.WithSizeDelta(48f, 70f).AsFlexGroup(
                direction: FlexDirection.Column
            ).Bind(ref _imageTransform).Use();
        }

        protected override void OnInitialize() {
            _layoutEditorComponentsList.WithListener(
                x => x.SelectedIndexes,
                HandleComponentsSelected
            );
            var eventsHandler = _windowHandle.Content.AddComponent<PointerEventsHandler>();
            eventsHandler.PointerUpEvent += OnComponentPointerUp;
            eventsHandler.PointerDownEvent += OnComponentPointerDown;
            eventsHandler.PointerEnterEvent += OnComponentPointerEnter;
            eventsHandler.PointerExitEvent += OnComponentPointerExit;
            RefreshWindowHandleColor();
        }

        #endregion

        #region ComponentsList

        private void RefreshComponents() {
            _layoutEditorComponentsList.Items.Clear();
            _layoutEditorComponentsList.Items.AddRange(_layoutEditor!.LayoutComponents);
            _layoutEditorComponentsList.Items.Remove(this);
            _layoutEditorComponentsList.Refresh();
        }

        #endregion

        #region Component Handling

        private ILayoutComponent? _selectedComponent;

        private void RefreshLayerButtons() {
            var isComponentAvailable = _selectedComponent is null;
            var layer = _selectedComponent?.ComponentController.ComponentLayer;

            var selector = new Func<ILayoutComponent, int>(static x => x.ComponentController.ComponentLayer);
            var minLayer = isComponentAvailable ? 0 : _layoutEditorComponentsList.Items.Min(selector);
            var maxLayer = isComponentAvailable ? 0 : _layoutEditorComponentsList.Items.Max(selector);

            _layerUpButton.Interactable = !isComponentAvailable && layer < maxLayer;
            _layerDownButton.Interactable = !isComponentAvailable && layer > minLayer;
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

        private void HandleComponentsSelected(IReadOnlyCollection<int> indexes) {
            if (indexes.Count is 0) return;
            HandleComponentSelected(_layoutEditorComponentsList.Items[indexes.First()]);
        }

        [UIAction("grid-button-clicked"), UsedImplicitly]
        private void HandleGridButtonClicked(bool state) { }

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
            var compPos = (Vector2)ContentTransform.localPosition;
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