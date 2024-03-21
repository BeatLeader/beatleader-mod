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
            }
        }

        public Color ActiveColor { get; set; } = DefaultHoveredColor;

        public Color HoverColor { get; set; } = DefaultHoveredColor;

        public Color DisabledColor { get; set; } = DefaultColor;

        public Color Color {
            get => _color;
            set {
                _color = value;
                UpdateColor(0);
            }
        }

        private bool _colorizeOnHover = true;
        private Color _color = DefaultColor;

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
            return ButtonActive ? ActiveColor :
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