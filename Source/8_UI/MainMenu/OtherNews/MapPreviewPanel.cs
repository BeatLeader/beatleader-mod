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
    internal class MapPreviewPanel : ReactiveComponent {
        public MapPreviewPanel(CellContext<TrendingMapData> context) : base(false) {
            _context = context;
            ConstructAndInit();
        }

        public Action<TrendingMapData>? ButtonAction { get; set; }
        public Action<TrendingMapData>? BackgroundAction { get; set; }

        private readonly CellContext<TrendingMapData> _context;

        protected override GameObject Construct() {
            var graphicState = Remember(GraphicState.None);

            var colors = new ColorSet {
                Color = Color.clear,
                HoveredColor = new Color(0, 0, 0, 0.5f + 0.4f)
            };

            return new Background {
                    Sprite = BeatSaberResources.Sprites.background,
                    PixelsPerUnit = 10,
                    Skew = UIStyle.Skew,
                    sColor = graphicState.MapColorSet(colors),

                    Children = {
                        new WebImage {
                                Sprite = BundleLoader.UnknownIcon,
                                Material = BundleLoader.RoundTextureMaterial,
                                Skew = UIStyle.Skew
                            }
                            .AsFlexItem(size: new() { x = 8, y = 8 })
                            .On(
                                _context,
                                (img, ctx) => {
                                    var coverImage = ctx.Item.song.coverImage;

                                    if (string.IsNullOrEmpty(coverImage)) {
                                        img.Src = null;
                                        img.Sprite = BundleLoader.UnknownIcon;
                                    } else {
                                        img.Src = coverImage;
                                    }
                                },
                                lazy: true
                            ),

                        new Layout {
                                Children = {
                                    new Label {
                                            FontSize = 3f,
                                            FontStyle = FontStyles.Italic,
                                            Overflow = TextOverflowModes.Ellipsis,
                                            Alignment = TextAlignmentOptions.CaplineLeft,
                                            sText = _context.Map(x => x.Item.song.name)
                                        }
                                        .AsFlexItem(),

                                    new Label {
                                            FontSize = 2.5f,
                                            FontStyle = FontStyles.Italic,
                                            Overflow = TextOverflowModes.Ellipsis,
                                            Alignment = TextAlignmentOptions.TopLeft,
                                            Color = new Color(0.53f, 0.53f, 0.53f),
                                            sText = _context.Map(x => x.Item.song.mapper)
                                        }
                                        .AsFlexItem(margin: new() { left = -0.6f })
                                }
                            }
                            .AsFlexGroup(direction: FlexDirection.Column)
                            .AsFlexItem(margin: new() { left = 1 }, flexGrow: 1, flexShrink: 1),

                        new BsTextButton {
                            Text = "Play",
                            OnClick = () => ButtonAction?.Invoke(_context.Item)
                        }.AsFlexItem(size: new() { x = 12, y = 8 })
                    }
                }
                .WithPointerEvents(
                    onEnter: _ => graphicState.IsHovered = true,
                    onLeave: _ => graphicState.IsHovered = false,
                    onDown: _ => {
                        global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                        BackgroundAction?.Invoke(_context.Item);
                    }
                )
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