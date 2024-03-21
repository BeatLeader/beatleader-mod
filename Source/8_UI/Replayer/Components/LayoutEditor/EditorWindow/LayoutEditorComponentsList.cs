using System.Collections.Generic;
using System.Diagnostics;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class LayoutEditorComponentsList : ReactiveListComponentBase<ILayoutComponent, LayoutEditorComponentsList.Cell> {
        #region Cell

        public class Cell : ReactiveTableCell<ILayoutComponent> {
            #region Setup

            private ILayoutComponent _layoutComponent = null!;

            protected override void Init(ILayoutComponent component) {
                _layoutComponent = component;
                _componentNameText.Text = component.ComponentName;
                _componentLayerText.Text = component.ComponentController.ComponentLayer.ToString();
                _componentStateButton.Click(component.ComponentController.ComponentActive);
            }

            #endregion

            #region Construct

            private UI.Reactive.Components.ImageButton _componentStateButton = null!;
            private Label _componentNameText = null!;
            private Label _componentLayerText = null!;
            private Image _backgroundImage = null!;

            protected override GameObject Construct() {
                return new BeatLeader.UI.Reactive.Components.Dummy {
                    Children = {
                        new Image {
                            Sprite = BundleLoader.WhiteBG,
                            PixelsPerUnit = 8f,
                            Children = {
                                //
                                new UI.Reactive.Components.Dummy {
                                    Children = {
                                        new Label {
                                            FontSize = 3.5f,
                                            Alignment = TextAlignmentOptions.MidlineLeft
                                        }.AsFlexItem(grow: 1f).Bind(ref _componentNameText),
                                        new Label {
                                            FontSize = 2.6f,
                                            Alignment = TextAlignmentOptions.TopRight
                                        }.AsFlexItem(
                                            minSize: new(2, YogaValue.Undefined),
                                            margin: new() { right = 1f }
                                        ).Bind(ref _componentLayerText)
                                    }
                                }.AsFlexGroup().AsFlexItem(grow: 1f),
                                //
                                new Image {
                                    Sprite = BundleLoader.WhiteBG,
                                    PixelsPerUnit = 8f,
                                    Color = new(0.03f, 0.03f, 0.03f),
                                    Children = {
                                        new UI.Reactive.Components.ImageButton {
                                            GrowOnHover = false,
                                            Sticky = true,
                                            HoverColor = new(0.3f, 0.3f, 0.3f),
                                            ActiveColor = Color.white,
                                            Image = {
                                                Sprite = BundleLoader.EyeIcon,
                                                PreserveAspect = true
                                            }
                                        }.AsFlexItem(grow: 1f).Bind(
                                            ref _componentStateButton
                                        ).WithClickListener(HandleVisibilityButtonClicked)
                                    }
                                }.AsFlexGroup(padding: 1f).AsFlexItem(aspectRatio: 1f)
                                //
                            }
                        }.AsFlexGroup(
                            padding: 1f,
                            justifyContent: Justify.FlexStart
                        ).AsFlexItem(grow: 1f).Bind(ref _backgroundImage)
                    }
                }.AsFlexGroup(
                    padding: new() { bottom = 1f }
                ).WithRectExpand().Use();
            }
            
            #endregion

            #region Colors

            private static readonly Color baseColor = new(0.3f, 0.3f, 0.3f);
            private static readonly Color selectedColor = Color.cyan;

            private static readonly Color baseTextColor = Color.white;
            private static readonly Color selectedTextColor = Color.black;

            private void RefreshColors(bool selected) {
                _backgroundImage.Color = selected ? selectedColor : baseColor;
                var textColor = selected ? selectedTextColor : baseTextColor;
                _componentNameText.Color = textColor;
                _componentLayerText.Color = textColor;
            }

            #endregion

            #region Callbacks

            public override void OnCellStateChange(bool selected, bool highlighted) {
                RefreshColors(selected);
            }

            private void HandleVisibilityButtonClicked(bool state) {
                _layoutComponent.ComponentController.ComponentActive = state;
            }

            #endregion
        }

        #endregion

        #region Sorting

        private class ComponentComparator : IComparer<ILayoutComponent> {
            public int Compare(ILayoutComponent x, ILayoutComponent y) => Comparer<int>.Default
                .Compare(y.ComponentController.ComponentLayer, x.ComponentController.ComponentLayer);
        }

        private readonly ComponentComparator _componentComparator = new();

        private void RefreshSorting() {
            Items.Sort(_componentComparator);
        }

        #endregion

        #region Setup

        protected override float CellSize => 9.5f;

        protected override void OnEarlyRefresh() {
            RefreshSorting();
        }

        #endregion
    }
}