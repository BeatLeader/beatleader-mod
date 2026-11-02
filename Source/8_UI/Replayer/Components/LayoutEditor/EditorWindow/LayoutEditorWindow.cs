using System;
using System.Linq;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

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

            if (handler != null) {
                _handler = handler;
                _editor = handler as ILayoutEditor;

                _editor!.ComponentAddedEvent += HandleComponentAdded;
                _editor.ComponentRemovedEvent += HandleComponentRemoved;
                RefreshComponents();
            } else {
                if (_editor != null) {
                    _editor!.ComponentAddedEvent -= HandleComponentAdded;
                    _editor.ComponentRemovedEvent -= HandleComponentRemoved;
                }
                ContentTransform.SetParent(null);

                _editor = null;
                _handler = null;
            }
        }

        #endregion

        #region LayoutData

        public ref LayoutData LayoutData => ref _layoutData;

        private LayoutData _layoutData;

        public void ApplyLayoutData(bool notify = true) {
            // do nothing because why would we
        }

        public void LoadLayoutData() {
            // same as ApplyLayoutData
        }

        #endregion

        #region Movement

        private Vector2 _componentOriginPos;
        private Vector2 _componentPosOffset;

        protected override void OnUpdate() {
            if (_windowHandleState.IsActive) UpdateMovement();
        }

        private void UpdateMovement() {
            var componentDestinationPos = _handler!.PointerPosition + _componentPosOffset;
            if (componentDestinationPos == _componentOriginPos) return;
            ContentTransform.localPosition = LayoutTool.ApplyBorders(
                componentDestinationPos,
                ContentTransform.rect.size,
                _handler!.AreaTransform!.rect.size
            );
        }

        #endregion

        #region Window Handle

        private static readonly ColorSet windowHandleColors = new() {
            Color = Color.cyan,
            HoveredColor = Color.yellow,
            ActiveColor = Color.yellow
        };

        private State<GraphicState> _windowHandleState = null!;

        #endregion

        #region Construct

        private LayoutEditorComponentsList _layoutEditorComponentsList = null!;
        private BsTextButton _layerUpButton = null!;
        private BsTextButton _layerDownButton = null!;

        protected override GameObject Construct() {
            _windowHandleState = Remember(GraphicState.None);

            return new Background {
                Sprite = BundleLoader.Sprites.background,
                PixelsPerUnit = 7f,
                Color = new(0.14f, 0.14f, 0.14f),

                FlexItem = {
                    Size = new() { x = 48.pt, y = 70.pt }
                },

                FlexController = {
                    FlexDirection = FlexDirection.Column
                },

                Children = {
                    //handle
                    new Background {
                        FlexController = {
                            JustifyContent = Justify.FlexStart
                        },

                        FlexItem = {
                            FlexBasis = 4f
                        },

                        Sprite = BundleLoader.Sprites.backgroundTop,
                        PixelsPerUnit = 7f,
                        sColor = _windowHandleState.MapColorSet(windowHandleColors),
                        Children = {
                            new Label {
                                FlexItem = {
                                    Size = new() { x = "auto" },
                                    Margin = new() { left = 2f }
                                },

                                Text = "Layout Editor",
                                FontSize = 3f,
                                Color = Color.black
                            }
                        }
                    }.WithPointerEvents(
                        onEnter: _ => _windowHandleState.IsHovered = true,
                        onLeave: _ => _windowHandleState.IsHovered = false,
                        onDown: HandleWindowHandlePointerDown,
                        onUp: _ => _windowHandleState.IsActive = false
                    ),

                    //list
                    new Layout {
                        FlexController = {
                            Padding = 1f
                        },

                        FlexItem = {
                            FlexGrow = 1f
                        },

                        Children = {
                            new LayoutEditorComponentsList {
                                FlexItem = {
                                    FlexGrow = 1f
                                },

                                OnComponentSelected = HandleListComponentSelected
                            }.Bind(ref _layoutEditorComponentsList),
                        }
                    },

                    //buttons
                    new Background {
                        Sprite = BundleLoader.Sprites.background,
                        Color = new(0.22f, 0.22f, 0.22f),
                        PixelsPerUnit = 10f,

                        FlexItem = {
                            FlexBasis = 18.pt,
                            Margin = 1.pt
                        },

                        FlexController = {
                            Padding = 2.pt
                        },

                        Children = {
                            //layer buttons
                            new Layout {
                                FlexController = {
                                    FlexDirection = FlexDirection.Column,
                                    Gap = 0.5f
                                },

                                FlexItem = {
                                    FlexGrow = 1f
                                },

                                Children = {
                                    //layer up button
                                    new BsTextButton {
                                        FlexItem = {
                                            FlexBasis = 6f
                                        },

                                        Text = "Up",
                                        Skew = 0f,
                                        OnClick = () => ModifySelectedComponentLayer(1)
                                    }.Bind(ref _layerUpButton),

                                    //layer down button
                                    new BsTextButton {
                                        FlexItem = {
                                            FlexBasis = 6f
                                        },

                                        Text = "Down",
                                        Skew = 0f,
                                        OnClick = () => ModifySelectedComponentLayer(-1)
                                    }.Bind(ref _layerDownButton),
                                }
                            },
                            //exit & apply buttons
                            new Layout {
                                FlexController = {
                                    FlexDirection = FlexDirection.Column,
                                    Padding = new() { left = 2f },
                                    Gap = 0.5f
                                },

                                FlexItem = {
                                    FlexGrow = 2f
                                },

                                Children = {
                                    //cancel button
                                    new BsTextButton {
                                        FlexItem = {
                                            FlexBasis = 6f
                                        },

                                        Text = "Cancel",
                                        Skew = 0f,
                                        OnClick = () => {
                                            _editor!.CancelChanges();
                                            _editor!.Mode = _editor.PreviousMode;
                                        }
                                    },

                                    //apply button
                                    new BsPrimaryTextButton {
                                        FlexItem = {
                                            FlexBasis = 6f
                                        },

                                        Text = "Apply",
                                        Skew = 0f,
                                        OnClick = () => {
                                            _editor!.Mode = _editor.PreviousMode;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }.Use();
        }

        private void RefreshComponents() {
            _layoutEditorComponentsList.SetItems(_editor!.LayoutComponents.Where(x => x != this));
        }

        #endregion

        #region Component Handling

        private ILayoutComponent? _selectedComponent;

        private void RefreshLayerButtons() {
            if (_selectedComponent is null) {
                _layerUpButton.Interactable = false;
                _layerDownButton.Interactable = false;
                return;
            }

            var layer = _selectedComponent.LayoutData.layer;
            var items = _layoutEditorComponentsList.Items;

            _layerUpButton.Interactable = layer < items.Max(static x => x.LayoutData.layer);
            _layerDownButton.Interactable = layer > items.Min(static x => x.LayoutData.layer);
        }

        private void ModifySelectedComponentLayer(int layer) {
            if (_selectedComponent is null) return;
            _selectedComponent.LayoutData.layer += layer;
            _selectedComponent.ApplyLayoutData();
            _layoutEditorComponentsList.Refresh();
            RefreshLayerButtons();
        }

        #endregion

        #region Callbacks

        private bool _applyingSelection;

        public void OnEditorModeChanged(LayoutEditorMode mode) {
            var editing = mode is LayoutEditorMode.Edit;
            Content.SetActive(editing);
            if (editing) _layoutEditorComponentsList.Refresh();
        }

        public void OnSelectedComponentChanged(ILayoutComponent? component) {
            _selectedComponent = component;
            RefreshLayerButtons();
            if (component == null) {
                _layoutEditorComponentsList.Select(null);
            } else if (!_applyingSelection) {
                _layoutEditorComponentsList.Select(component);
            }
        }

        private void HandleListComponentSelected(ILayoutComponent component) {
            // Using _applyingSelection to prevent endless loop
            _applyingSelection = true;
            _handler!.OnSelect(component);
            _applyingSelection = false;
        }

        private void HandleComponentAdded(ILayoutComponent component) {
            if (component == this) {
                return;
            }
            _layoutEditorComponentsList.AddItem(component);
            RefreshLayerButtons();
        }

        private void HandleComponentRemoved(ILayoutComponent component) {
            if (_layoutEditorComponentsList.RemoveItem(component)) {
                RefreshLayerButtons();
            }
        }

        #endregion

        #region PointerEventsHandler Callbacks

        private void HandleWindowHandlePointerDown(PointerEventsHandler handler) {
            var compPos = (Vector2)ContentTransform.localPosition;
            _componentPosOffset = compPos - _handler!.PointerPosition;
            _componentOriginPos = compPos;
            _windowHandleState.IsActive = true;
        }

        #endregion
    }
}