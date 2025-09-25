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
                    Sprite = BundleLoader.Sprites.background,
                    Color = Color.black.ColorWithAlpha(0.5f),
                    PixelsPerUnit = 10f,
                    Children = {
                        new Label {
                            FontSize = 4f,
                            FontStyle = FontStyles.Italic,
                            Alignment = TextAlignmentOptions.Center
                        }.AsFlexItem().Bind(ref _textLabel)
                    }
                }.AsFlexGroup(
                    direction: FlexDirection.Row,
                    justifyContent: Justify.Center,
                    alignItems: Align.Center,
                    padding: new() { left = 1, right = 1 }
                )
                .Bind(ref _bg)
                .AsBlurBackground()
                .AsFlexItem(
                    size: new() { y = 5 },
                    margin: new() { bottom = 1f }
                ).Use();
        }
    }
}