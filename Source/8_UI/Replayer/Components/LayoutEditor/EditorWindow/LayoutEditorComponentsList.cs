using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;
using Reactive;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Components.Basic;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Background = Reactive.BeatSaber.Components.Background;
using Image = Reactive.BeatSaber.Components.Image;
using Label = Reactive.BeatSaber.Components.Label;

namespace BeatLeader.Components {
    internal class LayoutEditorComponentsList : ReactiveComponent {
        #region Public API

        public IReadOnlyList<ILayoutComponent> Items => _items;

        public Action<ILayoutComponent>? OnComponentSelected { get; set; }

        private readonly List<ILayoutComponent> _items = new();

        public void SetItems(IEnumerable<ILayoutComponent> items) {
            _items.Clear();
            _items.AddRange(items);
            Refresh();
        }

        public void AddItem(ILayoutComponent item) {
            _items.Add(item);
            Refresh();
        }

        public bool RemoveItem(ILayoutComponent item) {
            if (!_items.Remove(item)) {
                return false;
            }
            Refresh();
            return true;
        }

        public void Refresh() {
            _items.Sort(CompareLayersDescending);
            _table.Items = _items.ToArray();
            _dataRevision.Value++;
        }

        public void Select(ILayoutComponent? component) {
            _settingSelection = true;
            _table.SelectedItems = component == null ? [] : [component];
            _settingSelection = false;
        }

        #endregion

        #region Sorting

        private static int CompareLayersDescending(ILayoutComponent x, ILayoutComponent y) {
            return y.LayoutData.layer.CompareTo(x.LayoutData.layer);
        }

        #endregion

        #region Construct

        private Table<ILayoutComponent, IReactiveComponent> _table = null!;
        private State<int> _dataRevision = null!;
        private bool _settingSelection;

        protected override GameObject Construct() {
            _dataRevision = Remember(0);

            return new Table<ILayoutComponent, IReactiveComponent> {
                ScrollContext = new ScrollContext(),
                Items = [],
                ConstructCell = ConstructCell,
                OnSelectedItemsChanged = HandleSelectedItemsChanged
            }.Bind(ref _table).Use();
        }

        #endregion

        #region Cell

        private static readonly Color baseColor = new(0.3f, 0.3f, 0.3f);
        private static readonly Color selectedColor = Color.cyan;

        private static readonly Color baseTextColor = Color.white;
        private static readonly Color selectedTextColor = Color.black;

        private static readonly ColorSet visibilityButtonColors = new() {
            Color = Color.clear,
            HoveredColor = new(0.3f, 0.3f, 0.3f),
            ActiveColor = Color.white
        };

        [StateGen]
        private IReactiveComponent ConstructCell(CellContext<ILayoutComponent> context) {
            var layoutData = RememberDerived(context, _dataRevision, static (x, _) => x.Item.LayoutData);
            var textColor = context.Map(static x => x.Selected ? selectedTextColor : baseTextColor);
            var visibilityState = Remember(GraphicState.None);

            void HandleVisibilityButtonClicked() {
                var visible = !visibilityState.IsActive;
                visibilityState.IsActive = visible;

                var component = context.Item;
                component.LayoutData.visible = visible;
                component.ApplyLayoutData();
            }

            return new Layout {
                Children = {
                    new Background {
                        Sprite = BundleLoader.Sprites.background,
                        PixelsPerUnit = 8f,
                        sColor = context.Map(static x => x.Selected ? selectedColor : baseColor),
                        Children = {
                            //
                            new Layout {
                                Children = {
                                    new Label {
                                        FontSize = 3.5f,
                                        Alignment = TextAlignmentOptions.MidlineLeft,
                                        sText = context.Map(static x => x.Item.ComponentName),
                                        sColor = textColor
                                    }.AsFlexItem(flexGrow: 1f),

                                    new Label {
                                        FontSize = 2.6f,
                                        Alignment = TextAlignmentOptions.TopRight,
                                        sText = layoutData.Map(static x => x.layer.ToString()),
                                        sColor = textColor
                                    }.AsFlexItem(
                                        minSize: new(2, YogaValue.Undefined),
                                        margin: new() { right = 1f }
                                    )
                                }
                            }.AsFlexGroup().AsFlexItem(flexGrow: 1f),
                            //
                            new Background {
                                Sprite = BundleLoader.Sprites.background,
                                PixelsPerUnit = 8f,
                                Color = new(0.03f, 0.03f, 0.03f),
                                Children = {
                                    new Image {
                                        Sprite = BundleLoader.EyeIcon,
                                        PreserveAspect = true,
                                        sColor = visibilityState.MapColorSet(visibilityButtonColors)
                                    }.WithPointerEvents(
                                        onEnter: _ => visibilityState.IsHovered = true,
                                        onLeave: _ => visibilityState.IsHovered = false,
                                        onDown: _ => {
                                            HandleVisibilityButtonClicked();
                                            global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                                        }
                                    ).On(
                                        layoutData,
                                        (_, x) => visibilityState.IsActive = x.visible
                                    ).AsFlexItem(flexGrow: 1f)
                                }
                            }.AsFlexGroup(padding: 1f).AsFlexItem(aspectRatio: 1f)
                            //
                        }
                    }.AsFlexGroup(
                        padding: 1f,
                        justifyContent: Justify.FlexStart
                    ).AsFlexItem(flexGrow: 1f).WithPointerEvents(
                        onDown: _ => context.Selected = true
                    )
                }
            }.AsFlexGroup(
                padding: new() { bottom = 1f }
            ).AsFlexItem(
                size: new() { y = 9.5f.pt }
            );
        }

        #endregion

        #region Callbacks

        private void HandleSelectedItemsChanged(IReadOnlyCollection<ILayoutComponent> items) {
            if (_settingSelection || items.Count is 0) {
                return;
            }
            OnComponentSelected?.Invoke(items.First());
        }

        #endregion
    }
}