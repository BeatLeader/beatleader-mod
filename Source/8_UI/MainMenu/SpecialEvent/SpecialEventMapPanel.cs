using System;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu;

internal class SpecialEventMapPanel : ReactiveComponent {
    #region Public API

    public void SetData(PlatformEventMap? map) {
        _map.Value = map;

        if (map != null) {
            _mapDetail.Value = map.song;
        }
    }

    #endregion

    #region Constuct

    private ObservableValue<PlatformEventMap?> _map = null!;
    private ObservableValue<MapDetail> _mapDetail = null!;

    protected override GameObject Construct() {
        _map = Remember<PlatformEventMap?>(null);
        _mapDetail = Remember<MapDetail>(null!);

        return new Layout {
                Children = {
                    new Label {
                            FontSize = 4.5f,
                            EnableWrapping = true
                        }
                        .Animate(
                            _map,
                            onEffect: (x, y) => {
                                string text;

                                if (y == null) {
                                    text = "Oops, seems like Monke has stolen the map";
                                } else {
                                    var date = y.StartDate();
                                    var today = date.Date == DateTime.UtcNow.Date;

                                    text = today ? "Today" : FormatUtils.GetRelativeTimeString(date, false);
                                }

                                x.Text = text;
                            }
                        )
                        .AsFlexItem(margin: new() { top = 1.pt(), bottom = 1.pt() }),

                    new WebImage {
                            Material = BundleLoader.RoundTextureMaterial
                        }
                        .Animate(_map, (x, y) => x.Enabled = y != null)
                        .Animate(_mapDetail, (x, y) => x.Src = y.coverImage)
                        .AsFlexItem(size: 25.pt()),

                    new Label {
                            FontStyle = FontStyles.Italic,
                            Overflow = TextOverflowModes.Ellipsis,
                            FontSize = 5f
                        }
                        .Animate(_map, (x, y) => x.Enabled = y != null)
                        .Animate(_mapDetail, (x, y) => x.Text = y.name)
                        .AsFlexItem(margin: new() { left = 1.pt(), right = 1.pt() }),

                    new Label {
                            Color = Color.white * 0.7f,
                            FontStyle = FontStyles.Italic,
                            Overflow = TextOverflowModes.Ellipsis,
                            FontSize = 4f
                        }
                        .Animate(_map, (x, y) => x.Enabled = y != null)
                        .Animate(_mapDetail, (x, y) => x.Text = y.author)
                        .AsFlexItem(margin: new() { top = -1.5f, left = 1.pt(), right = 1.pt() })
                }
            }
            .AsFlexGroup(direction: FlexDirection.Column, justifyContent: Justify.Center, alignItems: Align.Center)
            .AsFlexItem(flex: 1)
            .Use();
    }

    #endregion
}