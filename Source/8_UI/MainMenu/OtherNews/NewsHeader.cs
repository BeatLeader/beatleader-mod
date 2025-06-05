using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class NewsHeader : ReactiveComponent {
        private string _textValue = "";
        public string Text {
            get => _textValue;
            set {
                if (_textValue.Equals(value)) return;
                _textValue = value;
                if (_textLabel != null) _textLabel.Text = value;
            }
        }

        private Label _textLabel = null!;

        protected override GameObject Construct() {
            return new Background {
                Sprite = BundleLoader.Sprites.background,
                Color = Color.black.ColorWithAlpha(0.5f),
                PixelsPerUnit = 10f,
                Skew = BeatSaberStyle.Skew,
                Children = {
                    new Label {
                        Text = _textValue,
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
            .AsBlurBackground()
            .AsFlexItem(
                size: new() { y = 5 }, 
                flexGrow: 1
            ).Use();
        }
    }
} 