using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ColoredButton : Button {
        #region UI Properties

        public bool ColorizeOnHover {
            get => _colorizeOnHover;
            set {
                _colorizeOnHover = value;
                if (!IsInitialized) return;
                UpdateColor(0);
                NotifyPropertyChanged();
            }
        }

        public Color ActiveColor {
            get => _activeColor;
            set {
                _activeColor = value;
                NotifyPropertyChanged();
            }
        }

        public Color HoverColor {
            get => _hoverColor;
            set {
                _hoverColor = value;
                NotifyPropertyChanged();
            }
        }

        public Color DisabledColor {
            get => _disabledColor;
            set {
                _disabledColor = value;
                NotifyPropertyChanged();
            }
        }

        public Color Color {
            get => _color;
            set {
                _color = value;
                UpdateColor(0);
                NotifyPropertyChanged();
            }
        }

        private bool _colorizeOnHover = true;
        private Color _color = DefaultColor;
        private Color _activeColor = DefaultHoveredColor;
        private Color _hoverColor = DefaultHoveredColor;
        private Color _disabledColor = DefaultColor;

        #endregion

        #region Abstraction

        protected virtual void ApplyColor(Color color, float progress) { }

        #endregion

        #region Color

        public static readonly Color DefaultHoveredColor = new(0.0f, 0.4f, 1.0f, 1.0f);
        public static readonly Color DefaultColor = new(0.8f, 0.8f, 0.8f, 0.2f);

        protected void UpdateColor(float hoverProgress) {
            ApplyColor(LerpColor(hoverProgress, Color, HoverColor), hoverProgress);
        }

        protected Color LerpColor(float t, Color color, Color color2) {
            return Active ? ActiveColor :
                Interactable ? Color.Lerp(color, color2, _colorizeOnHover ? t : 0) :
                DisabledColor;
        }

        protected override void OnHoverProgressChange(float progress) {
            UpdateColor(progress);
        }

        protected override void OnButtonStateChange(bool state) {
            UpdateColor(state ? 1f : 0f);
        }

        #endregion
    }
}