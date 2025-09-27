using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UI;

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

                        new SpecialEventMapPanel()
                            .Animate(_event, (x, y) => x.SetData(y.today))
                            .Export(out var mapPanel),

                        new SpecialEventBar {
                            OnDayChanged = x => mapPanel.SetData(x)
                        }.Animate(_event, (x, y) => x.SetData(y))
                    }
                }
                .AsBackground(color: Color.black.ColorWithAlpha(0.5f))
                .WithNativeComponent(out RectMask2D _)
                .AsFlexGroup(direction: FlexDirection.Column, padding: 1.pt())
                .AsFlexItem(flex: 1f, size: new() { x = 52.pt() })
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