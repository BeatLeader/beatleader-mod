using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ColorPicker : ReactiveComponent {
        #region Color

        public Color Color {
            get => _color;
            set {
                _color = value;
                _colorSampleImage.Color = value;
                if (_modalOpened) {
                    _circleModal.Modal.ColorCircle.SetColor(value);
                }
            }
        }

        private Color _color;
        private bool _modalOpened;

        #endregion

        #region Construct

        public RelativePlacement CirclePlacement { get; set; } = RelativePlacement.Center;

        private SharedModal<ColorCircleModal> _circleModal = null!;
        private Image _colorSampleImage = null!;

        protected override GameObject Construct() {
            return new AeroButtonLayout {
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
                    new SharedModal<ColorCircleModal>()
                        .WithAnchor(
                            this,
                            Lazy(() => CirclePlacement, false),
                            unbindOnceOpened: false
                        )
                        .WithShadow()
                        .WithOpenListener(HandleModalOpened)
                        .WithCloseListener(HandleModalClosed)
                        .Bind(ref _circleModal)
                }
            }.AsFlexGroup(
                justifyContent: Justify.FlexStart,
                padding: new() { left = 2f, top = 1f, right = 2f, bottom = 1f },
                gap: 1f
            ).WithModal(_circleModal).Use();
        }

        #endregion

        #region Callbacks

        private void HandleModalOpened(IModal modal, bool finished) {
            if (finished) return;
            _modalOpened = true;
            _circleModal.Modal.ColorCircle.WithListener(
                x => x.Color,
                HandleColorChanged
            );
            Color = _color;
        }

        private void HandleModalClosed(IModal modal, bool finished) {
            if (finished) return;
            _modalOpened = false;
            _circleModal.Modal.ColorCircle.WithoutListener(
                x => x.Color,
                HandleColorChanged
            );
        }

        private void HandleColorChanged(Color color) {
            _colorSampleImage.Color = color;
            _color = color;
            NotifyPropertyChanged(nameof(Color));
        }

        #endregion
    }
}