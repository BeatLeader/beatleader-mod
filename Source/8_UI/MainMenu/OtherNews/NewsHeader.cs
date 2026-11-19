using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class NewsHeader : ReactiveComponent {
        public string Text {
            get => _textLabel.Text;
            set => _textLabel.Text = value;
        }

        public Color Accent {
            get => _bg.Color;
            set => _bg.Color = value;
        }

        private Label _textLabel = null!;
        private Background _bg = null!;

        protected override GameObject Construct() {
            return new Background {
                    FlexController = {
                        FlexDirection = FlexDirection.Row,
                        JustifyContent = Justify.Center,
                        AlignItems = Align.Center,
                        Padding = new() { left = 1, right = 1 }
                    },

                    FlexItem = {
                        Size = new() { y = 5 },
                        Margin = new() { bottom = 0.5f }
                    },

                    Sprite = BundleLoader.Sprites.background,
                    PixelsPerUnit = 10f,
                    Children = {
                        new Label {
                            FontSize = 3.5f,
                            FontStyle = FontStyles.Italic,
                            Alignment = TextAlignmentOptions.Capline
                        }.Bind(ref _textLabel)
                    }
                }
                .Bind(ref _bg)
                .AsBackground(color: Color.black.ColorWithAlpha(0.5f)).Use();
        }
    }
}