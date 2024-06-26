using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ColoredButton : ButtonBase {
        #region UI Properties

        public IColorSet? Colors {
            get => _stateColorSet;
            set {
                if (_stateColorSet != null) {
                    _stateColorSet.SetUpdatedEvent -= UpdateColor;
                }
                _stateColorSet = value;
                if (_stateColorSet != null) {
                    _stateColorSet.SetUpdatedEvent += UpdateColor;
                }
                UpdateColor();
                NotifyPropertyChanged();
            }
        }

        private IColorSet? _stateColorSet = UIStyle.ButtonColorSet;

        #endregion

        #region Color

        protected void UpdateColor() {
            ApplyColor(GetColor(Colors));
        }

        protected Color GetColor(IColorSet? colorSet) {
            return GetColor(colorSet, AnimationProgress);
        }

        protected Color GetColor(IColorSet? colorSet, float progress) {
            if (colorSet == null) {
                return Color.clear;
            }
            var hovered = progress > 0f;
            var state = new GraphicElementState {
                active = Active,
                interactable = Interactable,
                hovered = hovered
            };
            var color = colorSet.GetColor(state);
            if (hovered) {
                state.hovered = false;
                var standardColor = colorSet.GetColor(state);
                color = Color.Lerp(standardColor, color, progress);
            }
            return color;
        }

        protected override void OnHoverProgressChange(float progress) {
            UpdateColor();
        }

        protected override void OnButtonStateChange(bool state) {
            UpdateColor();
        }

        protected virtual void ApplyColor(Color color) { }

        #endregion
    }
}