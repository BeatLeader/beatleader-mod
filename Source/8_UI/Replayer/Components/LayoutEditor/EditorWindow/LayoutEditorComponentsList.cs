using System.Collections.Generic;
using BeatLeader.Utils;
using Reactive;
using Reactive.Components;
using Reactive.Components.Basic;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Image = Reactive.BeatSaber.Components.Image;
using ImageButton = Reactive.BeatSaber.Components.ImageButton;
using Label = Reactive.BeatSaber.Components.Label;

namespace BeatLeader.Components {
    internal class LayoutEditorComponentsList : Reactive.BeatSaber.Components.Table<ILayoutComponent, LayoutEditorComponentsList.Cell> {
        #region Cell

        public class Cell : TableComponentCell<ILayoutComponent> {
            #region Setup

            private ILayoutComponent _layoutComponent = null!;

            protected override void OnInit(ILayoutComponent component) {
                _layoutComponent = component;
                _componentNameText.Text = component.ComponentName;
                _componentLayerText.Text = component.ComponentController.ComponentLayer.ToString();
                _componentStateButton.Click(component.ComponentController.ComponentActive);
            }

            #endregion

            #region Construct

            private ImageButton _componentStateButton = null!;
            private Label _componentNameText = null!;
            private Label _componentLayerText = null!;
            private Image _backgroundImage = null!;

            protected override GameObject Construct() {
                return new Button {
                    OnClick = () => SelectSelf(true),
                    Children = {
                        new Image {
                            Sprite = BundleLoader.Sprites.background,
                            PixelsPerUnit = 8f,
                            Children = {
                                //
                                new Dummy {
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
                                    Sprite = BundleLoader.Sprites.background,
                                    PixelsPerUnit = 8f,
                                    Color = new(0.03f, 0.03f, 0.03f),
                                    Children = {
                                        new ImageButton {
                                            Latching = true,
                                            Colors = new SimpleColorSet {
                                                HoveredColor = new(0.3f, 0.3f, 0.3f),
                                                ActiveColor = Color.white
                                            },
                                            OnStateChanged = HandleVisibilityButtonClicked,
                                            Image = {
                                                Sprite = BundleLoader.EyeIcon,
                                                PreserveAspect = true
                                            }
                                        }.AsFlexItem(grow: 1f).Bind(
                                            ref _componentStateButton
                                        )
                                    }
                                }.AsFlexGroup(padding: 1f).AsFlexItem(aspectRatio: 1f)
                                //
                            }
                        }.AsFlexGroup(
                            padding: 1f,
                            justifyContent: Justify.FlexStart
                        ).AsFlexItem(grow: 1f).Bind(ref _backgroundImage)
                    }
                }.AsFlexGroup(padding: new() { bottom = 1f }).WithSizeDelta(0f, 9.5f).Use();
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

            protected override void OnCellStateChange(bool selected) {
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

        protected override void OnEarlyRefresh() {
            RefreshSorting();
        }

        #endregion
    }
}