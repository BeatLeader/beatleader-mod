using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatLeader.UI.MainMenu;

internal class SpecialEventMapPanel : ReactiveComponent {
    #region Public API

    public void SetData(PlatformEventMap? map) {
        _map.Value = map;

        if (map != null) {
            _mapDetail.Value = map.song;
            LoadAndPlayPreview(CancellationToken.None).RunCatching();
        }
    }

    #endregion

    #region Constuct

    public ObservableValue<PlatformEventMap?> _map = null!;
    private ObservableValue<MapDetail> _mapDetail = null!;

    private AudioClip? _previewClip;
    private SongPreviewPlayer _songPreviewPlayer = null!;

    protected override GameObject Construct() {
        _map = Remember<PlatformEventMap?>(null);
        _mapDetail = Remember<MapDetail>(null!);

        _songPreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First();

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
                                    text = FormatUtils.GetDateOfMonthTimeString(y.startTime);
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
                        .AsFlexItem(margin: new() { top = -1.5f, left = 1.pt(), right = 1.pt() }),

                    new Label {
                            Color = Color.white * 0.5f,
                            Overflow = TextOverflowModes.Ellipsis,
                            FontSize = 3.5f
                        }
                        .Animate(_map, (x, y) => { 
                            if (y == null) { 
                                x.Enabled = false;
                                return;
                            }
                            x.Enabled = true;
                            if (y.points != null) {
                                x.Text = $"#{y.points.rank} place - {y.points.points} point{(y.points.points > 1 ? "s" : "")}";
                                if (y.IsHappening()) {
                                    x.Color = Color.yellow * 0.85f;
                                } else {
                                    x.Color = Color.green * 0.85f;
                                }
                            } else if (y.IsHappening()) {
                                x.Color = Color.white * 0.7f;
                                x.Text = FormatUtils.GetRemainingTime(-FormatUtils.GetRelativeTime(y.endTime));
                            } else {
                                x.Enabled = false;
                            }
                        })
                        .AsFlexItem(margin: new() { top = 0.5f, left = 1.pt(), right = 1.pt() })
                }
            }
            .AsFlexGroup(direction: FlexDirection.Column, justifyContent: Justify.Center, alignItems: Align.Center)
            .AsFlexItem(flex: 1)
            .Use();
    }

    protected override void OnDisable() {
        base.OnDisable();

        if (_songPreviewPlayer != null) {
            _songPreviewPlayer.CrossfadeToDefault();
        }
    }

    #endregion

    private async Task LoadAndPlayPreview(CancellationToken cancellationToken) {
        try {
            var previewUrl = $"https://eu.cdn.beatsaver.com/{_mapDetail.Value.hash.ToLowerInvariant()}.mp3";

            using (var www = UnityWebRequestMultimedia.GetAudioClip(previewUrl, AudioType.MPEG)) {
                var operation = www.SendWebRequest();

                while (!operation.isDone) {
                    if (cancellationToken.IsCancellationRequested) {
                        www.Abort();
                        return;
                    }
                    await Task.Delay(50);
                }

                if (www.result == UnityWebRequest.Result.Success) {
                    _previewClip = DownloadHandlerAudioClip.GetContent(www);
                    if (_previewClip != null && _songPreviewPlayer != null) {
                        _songPreviewPlayer.CrossfadeTo(_previewClip, 0, 1, _previewClip.length, null);
                    }
                }
            }
        } catch (Exception) {
            // Ignore preview loading errors
        }
    }
}