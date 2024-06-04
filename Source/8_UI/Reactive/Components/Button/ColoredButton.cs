using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ColoredButton : Button {
        #region UI Properties

        public StateColorSet? Colors {
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

        private StateColorSet? _stateColorSet = UIStyle.ButtonColorSet;

        #endregion

        #region Color

        protected void UpdateColor() {
            ApplyColor(GetColor(Colors));
        }

        protected Color GetColor(StateColorSet? colorSet) {
            if (colorSet == null) {
                return Color.clear;
            }
            if (Active) {
                return colorSet.ActiveColor;
            }
            if (!Interactable) {
                return colorSet.DisabledColor;
            }
            return Color.Lerp(colorSet.Color, colorSet.HoveredColor, AnimationProgress);
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