using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ColorPicker : ReactiveComponent {
        #region Color

        public Color Color {
            get => _color;
            set {
                _color = value;
                _circleModal.ColorCircle.SetColor(value);
                _colorSampleImage.Color = value;
            }
        }

        private Color _color;

        #endregion

        #region Construct

        public RelativePlacement CirclePlacement { get; set; } = RelativePlacement.Center;

        protected override float? DesiredHeight => 8f;
        protected override float? DesiredWidth => 13f;

        private ColorCircleDialog _circleModal = null!;
        private Image _colorSampleImage = null!;

        protected override GameObject Construct() {
            return new AeroButton {
                GrowOnHover = false,
                HoverLerpMul = float.MaxValue,
                Children = {
                    //icon
                    new Image {
                        Sprite = GameResources.Sprites.EditIcon,
                        PreserveAspect = true,
                        Skew = UIStyle.Skew,
                        Color = UIStyle.SecondaryTextColor
                    }.AsFlexItem(
                        size: new() { x = 4f, y = "auto" }
                    ),
                    //color sample
                    new Image {
                        Sprite = GameResources.Sprites.Circle,
                        PreserveAspect = true
                    }.AsFlexItem(
                        size: new() { x = 4f, y = "auto" }
                    ).Bind(ref _colorSampleImage),
                    //color circle
                    new ColorCircleDialog()
                        .WithAnchor(this, () => CirclePlacement)
                        .With(
                            x => x.ColorCircle.WithListener(
                                y => y.Color,
                                y => {
                                    _colorSampleImage.Color = y;
                                    _color = y;
                                    NotifyPropertyChanged(nameof(Color));
                                }
                            )
                        ).Bind(ref _circleModal)
                }
            }.AsFlexGroup(
                justifyContent: Justify.FlexStart,
                padding: new() { left = 2f, top = 1f, right = 2f, bottom = 1f },
                gap: 1f
            ).WithModal(_circleModal).Use();
        }

        #endregion
    }
}