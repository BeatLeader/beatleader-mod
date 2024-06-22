using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal class StateColorSet : IColorSet {
        public Color ActiveColor {
            get => _activeColor;
            set {
                _activeColor = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        public Color HoveredColor {
            get => _hoveredColor;
            set {
                _hoveredColor = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        public Color DisabledColor {
            get => _disabledColor;
            set {
                _disabledColor = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        public Color Color {
            get => _color;
            set {
                _color = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        private Color _disabledColor;
        private Color _activeColor;
        private Color _hoveredColor;
        private Color _color;

        public event Action? SetUpdatedEvent;
        
        public Color GetColor(GraphicElementState state) {
            if (state.active) {
                return _activeColor;
            }
            if (state.hovered) {
                return _hoveredColor;
            }
            if (!state.interactable) {
                return _disabledColor;
            }
            return _color;
        }
    }
}