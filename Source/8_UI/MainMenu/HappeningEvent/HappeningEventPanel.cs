using BeatLeader.API;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = Reactive.BeatSaber.Components.Image;

namespace BeatLeader.UI.MainMenu {
    internal class HappeningEventPanel : ReactiveComponent {
        #region Construct

        private ObservableValue<PlatformEventStatus> _event = null!;

        protected override GameObject Construct() {
            _event = Remember<PlatformEventStatus>(null!);

            return new Background {
                    Children = {
                        new NewsHeader {
                                Accent = Color.red.ColorWithAlpha(0.6f)
                            }
                            .Animate(_event, (x, y) => x.Text = y.headerText)
                            .AsFlexItem(size: new() { y = 6.pt() }),

                        new Layout {
                                Children = {
                                    new Label {
                                        Text = "Today",
                                        FontSize = 4.5f
                                    }.AsFlexItem(margin: new() { top = 1.pt(), bottom = 1.pt() }),

                                    new Image {
                                            Material = BundleLoader.RoundTextureMaterial
                                        }
                                        .Animate(_event, (x, y) => x.WithWebSource(y.today.song.coverImage))
                                        .AsFlexItem(size: 25.pt()),

                                    new Label {
                                            FontStyle = FontStyles.Italic,
                                            FontSize = 5f
                                        }
                                        .Animate(_event, (x, y) => x.Text = y.today.song.name)
                                        .AsFlexItem(),

                                    new Label {
                                            Color = Color.white * 0.7f,
                                            FontStyle = FontStyles.Italic,
                                            FontSize = 4f
                                        }
                                        .Animate(_event, (x, y) => x.Text = y.today.song.author)
                                        .AsFlexItem(margin: new() { top = -1.5f })
                                }
                            }
                            .AsFlexGroup(direction: FlexDirection.Column, justifyContent: Justify.Center, alignItems: Align.Center)
                            .AsFlexItem(flex: 1),

                        new Background {
                                Children = {
                                    new Label {
                                            Color = Color.white * 0.7f,
                                            Text = ""
                                        }
                                        .Animate(_event, (x, y) => x.Text = FormatUtils.GetRemainingTime(y.today.ExpiresIn()))
                                        .AsFlexItem(),

                                    new Layout {
                                            Children = {
                                                new BsButton {
                                                    Text = " Play ",
                                                    OnClick = () => { }
                                                }.AsFlexItem(margin: 1.pt()),
                                            }
                                        }
                                        .AsFlexGroup(justifyContent: Justify.Center)
                                        .WithRectExpand(),

                                    new BsButton {
                                        Text = "📅",
                                        ShowUnderline = false,
                                        Skew = 0f,
                                        OnClick = () => { }
                                    }.AsFlexItem(aspectRatio: 1f)
                                }
                            }
                            .AsBackground(color: Color.black.ColorWithAlpha(0.5f))
                            .AsFlexGroup(
                                justifyContent: Justify.SpaceBetween,
                                alignItems: Align.Stretch,
                                padding: 1.pt(),
                                gap: 2.pt()
                            )
                            .AsFlexItem(
                                size: new() { y = 10.pt() },
                                minSize: new() { x = 50.pt() }
                            ),
                    }
                }
                .AsBackground(color: Color.black.ColorWithAlpha(0.5f))
                .WithNativeComponent(out RectMask2D _)
                .AsFlexGroup(direction: FlexDirection.Column, padding: 1.pt())
                .AsFlexItem(flex: 1f)
                .Use();
        }

        #endregion

        #region Logic

        protected override async void OnInitialize() {
            var req = await PlatformEventStatusRequest.Send("75").Join();
            
            SetData(req.Result!);
        }

        public void SetData(PlatformEventStatus evt) {
            _event.Value = evt;
        }

        #endregion
    }
}