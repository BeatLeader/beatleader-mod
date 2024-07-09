using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class AvatarPartCategory : ReactiveComponent {
        #region Setup

        public void Setup(ReactivePool<AvatarPartControl> pool, AvatarPartConfigsGroup group) {
            _nameLabel.Text = group.GroupName ?? "Uncategorized";
            _controlsContainer.Children.Clear();
            foreach (var model in group.Configs) {
                var control = pool.Spawn();
                control.Setup(model);
                control.AsFlexItem();
                _controlsContainer.Children.Add(control);
            }
        }

        #endregion

        #region Construct

        private Label _nameLabel = null!;
        private Dummy _controlsContainer = null!;

        protected override GameObject Construct() {
            return new Image {
                Children = {
                    //header
                    new Image {
                        Sprite = BundleLoader.Sprites.backgroundTop,
                        PixelsPerUnit = 10f,
                        Color = new(0.08f, 0.08f, 0.08f, 1f),
                        Children = {
                            new Label {
                                FontStyle = FontStyles.Bold,
                                Alignment = TextAlignmentOptions.Center
                            }.AsFlexItem(size: new() { y = "auto" }).Bind(ref _nameLabel),
                        }
                    }.AsFlexGroup().AsFlexItem(),
                    //
                    new Dummy()
                        .AsFlexGroup(
                            direction: FlexDirection.Column,
                            padding: 1f,
                            gap: 1f
                        )
                        .AsFlexItem()
                        .Bind(ref _controlsContainer)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column
            ).AsBackground(
                color: new(0.1f, 0.1f, 0.1f, 1f)
            ).Use();
        }

        #endregion
    }
}