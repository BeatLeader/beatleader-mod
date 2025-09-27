using System;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class MapPreviewPanel : ListCell<TrendingMapData> {
        public Action<TrendingMapData>? ButtonAction { get; set; }
        public Action<TrendingMapData>? BackgroundAction { get; set; }

        protected override GameObject Construct() {
            return new BackgroundButton {
                    Colors = new StateColorSet {
                        States = {
                            GraphicState.None.WithColor(Color.clear),
                            GraphicState.Hovered.WithColor(new Color(0, 0, 0, 0.5f + 0.4f))
                        }
                    },

                    Image = {
                        Sprite = BeatSaberResources.Sprites.background,
                        PixelsPerUnit = 10,
                        Skew = UIStyle.Skew
                    },

                    OnClick = () => BackgroundAction?.Invoke(Item),

                    Children = {
                        new Image {
                                Sprite = BundleLoader.UnknownIcon,
                                Material = BundleLoader.RoundTextureMaterial,
                                Skew = UIStyle.Skew
                            }
                            .AsFlexItem(size: new() { x = 10, y = 10 })
                            .Animate(
                                ObservableItem,
                                (img, item) => {
                                    if (string.IsNullOrEmpty(item.song.coverImage)) {
                                        img.Sprite = BundleLoader.UnknownIcon;
                                    } else {
                                        img.WithWebSource(item.song.coverImage);
                                    }
                                }
                            ),

                        new Layout {
                                Children = {
                                    new Label {
                                            FontSize = 4f,
                                            FontStyle = FontStyles.Italic,
                                            Overflow = TextOverflowModes.Ellipsis,
                                            Alignment = TextAlignmentOptions.CaplineLeft
                                        }
                                        .AsFlexItem()
                                        .Animate(ObservableItem, (lbl, item) => lbl.Text = item.song.name),

                                    new Label {
                                            FontSize = 3.5f,
                                            FontStyle = FontStyles.Italic,
                                            Overflow = TextOverflowModes.Ellipsis,
                                            Alignment = TextAlignmentOptions.TopLeft,
                                            Color = new Color(0.53f, 0.53f, 0.53f)
                                        }
                                        .AsFlexItem(margin: new() { left = -0.6f })
                                        .Animate(ObservableItem, (lbl, item) => lbl.Text = item.song.mapper)
                                }
                            }
                            .AsFlexGroup(direction: FlexDirection.Column)
                            .AsFlexItem(margin: new() { left = 1 }, flexGrow: 1, flexShrink: 1),

                        new BsButton {
                            Text = "Play",
                            OnClick = () => ButtonAction?.Invoke(Item)
                        }.AsFlexItem(size: new() { x = 12, y = 8 })
                    }
                }
                .AsFlexGroup(
                    direction: FlexDirection.Row,
                    gap: 1f,
                    padding: new() { left = 1.5f, top = 1f, right = 2f, bottom = 1f },
                    alignItems: Align.Center
                )
                .AsFlexItem(margin: new() { left = 1f, right = 1f })
                .Use();
        }
    }
}