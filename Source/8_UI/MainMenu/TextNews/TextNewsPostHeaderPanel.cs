using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class TextNewsPostHeaderPanel : ReactiveComponent {
        private string _nameValue = "Loading...";
        public string Name {
            get => _nameValue;
            set => _nameLabel.Text = value;
        }

        private Label _nameLabel = null!;

        public long Timestamp {
            get => 0;
            set => _dateLabel.Text = FormatUtils.GetRelativeTimeString(value, false);
        }

        private Label _dateLabel = null!;

        public string AvatarUrl {
            get => "";
            set => _avatarImage.WithWebSource(value);
        }

        private Image _avatarImage = null!;

        protected override GameObject Construct() {
            return new Background {
                Sprite = BundleLoader.Sprites.background,
                Color = Color.black.ColorWithAlpha(0.5f),
                PixelsPerUnit = 10f,
                Children = {
                    new Layout {
                        Children = {
                            new Image {
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