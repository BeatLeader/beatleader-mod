using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    /// <summary>
    /// LayoutEditor controller that is implemented as LayoutEditor component.
    /// </summary>
    internal class LayoutEditorWindow : ReactiveComponent, ILayoutComponent {
        #region Setup

        public string ComponentName => "LayoutEditorWindow";

        private ILayoutComponentHandler? _handler;
        private ILayoutEditor? _editor;

        public void Setup(ILayoutComponentHandler? handler) {
            if (handler is not null and not ILayoutEditor) {
                throw new NotSupportedException("Only ILayoutEditor is supported");
            }
            _handler = handler;
            _editor = handler as ILayoutEditor;
            if (_handler != null) {
                RefreshComponents();
                ContentTransform.SetParent(_handler!.AreaTransform);
            } else {
                ContentTransform.SetParent(null);
            }
        }

        #endregion

        #region LayoutData

        public ref LayoutData LayoutData => ref _layoutData;

        private LayoutData _layoutData;

        public void ApplyLayoutData(bool notify) {
            // do nothing because why would we
        }

        public void LoadLayoutData() {
            // same as ApplyLayoutData
        }

        #endregion

        #region Movement

        private Vector2 _componentOriginPos;
        private Vector2 _componentPosOffset;
        private bool _isMoving;

        protected override void OnUpdate() {
            if (_isMoving) UpdateMovement();
        }

        private void UpdateMovement() {
            var componentDestinationPos = _handler!.PointerPosition + _componentPosOffset;
            if (componentDestinationPos == _componentOriginPos) return;
            ContentTransform.localPosition = LayoutTool.ApplyBorders(
                componentDestinationPos,
                _imageTransform.rect.size,
                _handler!.AreaTransform!.rect.size
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

        #region Construct

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
                                                    OnClick = () => ModifySelectedComponentLayer(1)
                                                }
                                                .WithLabel("+")
                                                .AsFlexItem(basis: 7f)
                                                .WithAccentColor(Color.red)
                                                .Bind(ref _layerUpButton),
                                            //layer down button

                                            new BsButton {
                                                    Skew = 0f,
                                                    OnClick = () => ModifySelectedComponentLayer(-1)
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
                                                    OnClick = () => {
                                                        _editor!.CancelChanges();
                                                        _editor!.Mode = LayoutEditorMode.View;
                                                    }
                                                }
                                                .WithLabel("Cancel")
                                                .AsFlexItem(basis: 7f),
                                            //apply button
                                            new BsPrimaryButton {
                                                    Skew = 0f,
                                                    OnClick = () => {
                                                        _editor!.Mode = LayoutEditorMode.View;
                                                    }
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
                static x => x.SelectedIndexes,
                HandleListComponentSelected
            );
            var eventsHandler = _windowHandle.Content.AddComponent<PointerEventsHandler>();
            eventsHandler.PointerUpEvent += OnComponentPointerUp;
            eventsHandler.PointerDownEvent += OnComponentPointerDown;
            eventsHandler.PointerEnterEvent += OnComponentPointerEnter;
            eventsHandler.PointerExitEvent += OnComponentPointerExit;
            RefreshWindowHandleColor();
        }

        private void RefreshComponents() {
            _layoutEditorComponentsList.Items.Clear();
            _layoutEditorComponentsList.Items.AddRange(_editor!.LayoutComponents);
            _layoutEditorComponentsList.Items.Remove(this);
            _layoutEditorComponentsList.Refresh();
        }

        #endregion

        #region Component Handling

        private static readonly Func<ILayoutComponent, int> layerSelector = static x => x.LayoutData.layer;
        private ILayoutComponent? _selectedComponent;

        private void RefreshLayerButtons() {
            var isComponentAvailable = _selectedComponent is null;
            var layer = _selectedComponent?.LayoutData.layer;

            var minLayer = isComponentAvailable ? 0 : _layoutEditorComponentsList.Items.Min(layerSelector);
            var maxLayer = isComponentAvailable ? 0 : _layoutEditorComponentsList.Items.Max(layerSelector);

            _layerUpButton.Interactable = !isComponentAvailable && layer < maxLayer;
            _layerDownButton.Interactable = !isComponentAvailable && layer > minLayer;
        }

        private void ModifySelectedComponentLayer(int layer) {
            if (_selectedComponent is null) return;
            _selectedComponent.LayoutData.layer += layer;
            _selectedComponent.ApplyLayoutData();
            _layoutEditorComponentsList.Refresh();
            _layoutEditorComponentsList.Select(_selectedComponent);
            RefreshLayerButtons();
        }

        #endregion

        #region Callbacks

        private bool _applyingSelection;

        public void OnEditorModeChanged(LayoutEditorMode mode) {
            Content.SetActive(mode is LayoutEditorMode.Edit);
        }

        public void OnSelectedComponentChanged(ILayoutComponent? component) {
            _selectedComponent = component;
            RefreshLayerButtons();
            if (component == null) {
                _layoutEditorComponentsList.ClearSelection();
            } else if (!_applyingSelection) {
                _layoutEditorComponentsList.Select(component);
            }
        }

        private void HandleListComponentSelected(IReadOnlyCollection<int> indexes) {
            if (indexes.Count is 0) return;
            var comp = _layoutEditorComponentsList.Items[indexes.First()];
            // Using _applyingSelection to prevent endless loop
            _applyingSelection = true;
            _handler!.OnSelect(comp);
            _applyingSelection = false;
        }

        #endregion

        #region PointerEventsHandler Callbacks

        private void OnComponentPointerUp(PointerEventsHandler handler, PointerEventData data) {
            _isMoving = false;
            RefreshWindowHandleColor();
        }

        private void OnComponentPointerDown(PointerEventsHandler handler, PointerEventData data) {
            var compPos = (Vector2)ContentTransform.localPosition;
            var pointerPos = _handler!.PointerPosition;
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