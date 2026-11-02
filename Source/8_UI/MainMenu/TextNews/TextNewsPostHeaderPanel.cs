using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class TextNewsPostHeaderPanel : ReactiveComponent {
        public string Owner {
            get => _nameLabel.Text;
            set => _nameLabel.Text = value;
        }

        public long Timestamp {
            get;
            set {
                field = value;
                _dateLabel.Text = FormatUtils.GetRelativeTimeString(value, false);
            }
        }

        public string? AvatarUrl {
            get => _avatarImage.Src;
            set => _avatarImage.Src = value;
        }

        private Label _nameLabel = null!;
        private Label _dateLabel = null!;
        private WebImage _avatarImage = null!;

        protected override GameObject Construct() {
            return new Background {
                Sprite = BundleLoader.Sprites.background,
                Color = Color.black.ColorWithAlpha(0.5f),
                PixelsPerUnit = 10f,
                Children = {
                    new Layout {
                        Children = {
                            new WebImage {
                                Sprite = BundleLoader.UnknownIcon,
                                Material = BundleLoader.RoundTextureMaterial
                            }.AsFlexItem(
                                aspectRatio: 1f, 
                                size: new() { x = 3, y = 3 }
                            ).Bind(ref _avatarImage),
                            new Label {
                                FontSize = 3f,
                                Text = "Loading News..."
                            }.AsFlexItem().Bind(ref _nameLabel),
                        }
                    }
                    .AsFlexGroup(
                        direction: FlexDirection.Row, 
                        gap: 1,
                        alignItems: Align.Center
                    )
                    .AsFlexItem(),
                    new Label {
                        FontSize = 3f,
                        Color = new Color(0.53f, 0.53f, 0.53f),
                        Text = "Loading News..."
                    }.AsFlexItem().Bind(ref _dateLabel)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Row, 
                justifyContent: Justify.SpaceBetween,
                alignItems: Align.Center,
                padding: new() { left = 1, right = 1 }
            ).AsBlurBackground().AsFlexItem(size: new() { y = 5 }).Use();
        }
    }
}