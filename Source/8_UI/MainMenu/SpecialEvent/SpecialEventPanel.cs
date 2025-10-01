using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UI;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.MainMenu {
    internal class SpecialEventPanel : ReactiveComponent {
        #region Public API

        public Action? OnBackClick { get; set; }

        public void SetData(PlatformEventStatus evt) {
            _event.Value = evt;
            _spinnerAlpha.Value = 0f;
        }

        public void SetLoading() {
            _spinnerAlpha.Value = 1f;
        }

        #endregion

        #region Construct

        private ObservableValue<PlatformEventStatus> _event = null!;
        private AnimatedValue<float> _spinnerAlpha = null!;

        protected override GameObject Construct() {
            _event = Remember<PlatformEventStatus>(null!);
            _spinnerAlpha = RememberAnimated(0f, 200.ms(), AnimationCurve.EaseInOut);

            return new Background {
                    Children = {
                        new NewsHeader()
                            .WithAlpha(_spinnerAlpha, invert: true)
                            .Animate(_event, (x, y) => x.Accent = y.eventDescription.MainColor() ?? x.Accent)
                            .Animate(_event, (x, y) => x.Text = y.eventDescription.name),

                        new SpecialEventMapPanel()
                            .WithAlpha(_spinnerAlpha, invert: true)
                            .Animate(_event, (x, y) => x.SetData(y.today))
                            .Export(out var mapPanel),

                        new SpecialEventBar {
                                OnBackClick = () => OnBackClick?.Invoke(),
                                OnPlayClick = () => MapDownloadDialog.OpenSongOrDownloadDialog(mapPanel._map.Value.song, ContentTransform),
                                OnDayChanged = x => mapPanel.SetData(x)
                            }
                            .WithAlpha(_spinnerAlpha, invert: true)
                            .Animate(_event, (x, y) => x.SetData(y)),

                        new Spinner {
                                Image = {
                                    RaycastTarget = false
                                }
                            }
                            .WithAlpha(_spinnerAlpha)
                            .AsFlexItem(
                                position: new() { left = 0.pt(), right = 0.pt() },
                                size: new() { y = 15.pt() },
                                margin: YogaFrame.Auto
                            )
                    }
                }
                .AsBackground(color: Color.black.ColorWithAlpha(0.5f))
                .AsRectMask()
                .AsFlexGroup(direction: FlexDirection.Column, justifyContent: Justify.Center, padding: 1.pt())
                .AsFlexItem(flex: 1f, size: new() { x = 52.pt() })
                .Use();
        }

        #endregion
    }
}