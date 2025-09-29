using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class TextNewsPostPanel : ListCell<NewsPost> {
        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new TextNewsPostHeaderPanel()
                        .Animate(
                            ObservableItem,
                            (panel, item) => {
                                panel.Name = item.owner;
                                panel.Timestamp = item.timepost;
                                panel.AvatarUrl = item.ownerIcon;
                            }
                        ),

                    new Label {
                            Text = "Loading...",
                            FontSize = 3,
                            EnableWrapping = true,
                            Alignment = TMPro.TextAlignmentOptions.Justified
                        }
                        .AsFlexItem(margin: new() { left = 1.pt(), right = 1.pt() })
                        .Animate(ObservableItem, (label, item) => label.Text = item.body),

                    new WebImage {
                            PreserveAspect = true,
                            Material = GameResources.UINoGlowMaterial
                        }
                        .Animate(
                            ObservableItem,
                            (image, item) => {
                                if (string.IsNullOrEmpty(item.image)) {
                                    image.Enabled = false;
                                } else {
                                    image.Src = item.image;
                                    image.Enabled = true;
                                }
                            }
                        )
                        .AsFlexItem(size: new() { y = 30 })
                }
            }.AsFlexGroup(
                gap: 1,
                direction: FlexDirection.Column
            ).AsFlexItem().Use();
        }
    }
}