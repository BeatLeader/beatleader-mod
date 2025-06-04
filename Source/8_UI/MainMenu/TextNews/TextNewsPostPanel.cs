using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class TextNewsPostPanel : ListViewCell<NewsPost> {

        #region Setup

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new TextNewsPostHeaderPanel()
                    .Animate(ObservableItem, (panel, item) => {
                        panel.Name = item.owner;
                        panel.Timestamp = item.timepost;
                        panel.AvatarUrl = item.ownerIcon;
                    })
                    .AsFlexItem(),

                    new Label {
                        Text = "Loading...",
                        FontSize = 3,
                        EnableWrapping = true
                    }
                    .AsFlexItem(flexGrow: 1)
                    .Animate(ObservableItem, (label, item) => {
                        label.Text = item.body;
                    }),

                    new WebImage {
                        PreserveAspect = true,
                        Material = GameResources.UINoGlowMaterial
                    }
                    .Animate(ObservableItem, (image, item) => {
                        image.Enabled = !string.IsNullOrEmpty(item.image);
                        image.Src = item.image;
                    })
                    .AsFlexItem(
                        size: new() { y = 30 }
                    )
                }
            }.AsFlexGroup(
                gap: 2,
                direction: FlexDirection.Column
            ).AsFlexItem()
            .Use();
        }

        #endregion
    }
}