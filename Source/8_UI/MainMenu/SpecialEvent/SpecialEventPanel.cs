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
    internal class SpecialEventPanel : ReactiveComponent {
        #region Construct

        private ObservableValue<PlatformEventStatus> _event = null!;

        protected override GameObject Construct() {
            _event = Remember<PlatformEventStatus>(null!);

            return new Background {
                    Children = {
                        new NewsHeader()
                            .Animate(_event, (x, y) => x.Accent = y.eventDescription.MainColor() ?? x.Accent)
                            .Animate(_event, (x, y) => x.Text = y.eventDescription.name),

                        new Layout {
                                Children = {
                                    new Label {
                                        Text = "Today",
                                        FontSize = 4.5f
                                    }.AsFlexItem(margin: new() { top = 1.pt(), bottom = 1.pt() }),

                                    new Image {
                                            Material = BundleLoader.RoundTextureMaterial
                                        }
                                        .Animate(_event, (x, y) => x.WithWebSource(y.today?.song.coverImage ?? ""))
                                        .AsFlexItem(size: 25.pt()),

                                    new Label {
                                            FontStyle = FontStyles.Italic,
                                            FontSize = 5f
                                        }
                                        .Animate(_event, (x, y) => x.Text = y.today?.song.name ?? "")
                                        .AsFlexItem(),

                                    new Label {
                                            Color = Color.white * 0.7f,
                                            FontStyle = FontStyles.Italic,
                                            FontSize = 4f
                                        }
                                        .Animate(_event, (x, y) => x.Text = y.today?.song.author ?? "")
                                        .AsFlexItem(margin: new() { top = -1.5f })
                                }
                            }
                            .AsFlexGroup(direction: FlexDirection.Column, justifyContent: Justify.Center, alignItems: Align.Center)
                            .AsFlexItem(flex: 1),

                        new SpecialEventBar()
                            .Animate(_event, (x, y) => x.SetData(y))
                    }
                }
                .AsBackground(color: Color.black.ColorWithAlpha(0.5f))
                .WithNativeComponent(out RectMask2D _)
                .AsFlexGroup(direction: FlexDirection.Column, padding: 1.pt())
                .AsFlexItem(flex: 1f)
                .Use();
        }

        #endregion

        #region Public API

        public void SetData(PlatformEventStatus evt) {
            _event.Value = evt;
        }

        #endregion
    }
}