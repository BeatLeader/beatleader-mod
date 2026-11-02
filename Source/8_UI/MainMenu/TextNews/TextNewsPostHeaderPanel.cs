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
                FlexController = {
                    FlexDirection = FlexDirection.Row,
                    JustifyContent = Justify.SpaceBetween,
                    AlignItems = Align.Center,
                    Padding = new() { left = 1, right = 1 }
                },

                FlexItem = {
                    Size = new() { y = 5 }
                },

                Sprite = BundleLoader.Sprites.background,
                Color = Color.black.ColorWithAlpha(0.5f),
                PixelsPerUnit = 10f,
                Children = {
                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Row,
                            Gap = 1,
                            AlignItems = Align.Center
                        },

                        Children = {
                            new WebImage {
                                FlexItem = {
                                    AspectRatio = 1f,
                                    Size = new() { x = 3, y = 3 }
                                },

                                Sprite = BundleLoader.UnknownIcon,
                                Material = BundleLoader.RoundTextureMaterial
                            }.Bind(ref _avatarImage),
                            new Label {
                                FontSize = 3f,
                                Text = "Loading News..."
                            }.Bind(ref _nameLabel),
                        }
                    },
                    new Label {
                        FontSize = 3f,
                        Color = new Color(0.53f, 0.53f, 0.53f),
                        Text = "Loading News..."
                    }.Bind(ref _dateLabel)
                }
            }.AsBlurBackground().Use();
        }
    }
}