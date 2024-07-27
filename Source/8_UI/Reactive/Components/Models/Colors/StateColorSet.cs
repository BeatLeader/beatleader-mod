using System;
using BeatLeader.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal class StateColorSet : IColorSet {
        public SerializableColor ActiveColor {
            get => _activeColor;
            set {
                _activeColor = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        public SerializableColor HoveredColor {
            get => _hoveredColor;
            set {
                _hoveredColor = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        public SerializableColor DisabledColor {
            get => _disabledColor;
            set {
                _disabledColor = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        public SerializableColor Color {
            get => _color;
            set {
                _color = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        public SerializableColor? HoveredActiveColor {
            get => _hoveredActiveColor;
            set {
                _hoveredActiveColor = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        private SerializableColor _disabledColor;
        private SerializableColor _activeColor;
        private SerializableColor _hoveredColor;
        private SerializableColor _color;
        private SerializableColor? _hoveredActiveColor;

        public event Action? SetUpdatedEvent;

        public Color GetColor(GraphicElementState state) {
            if (state.hovered) {
                return state.active ? _hoveredActiveColor.GetValueOrDefault(ActiveColor) : _hoveredColor;
            }
            if (state.active) {
                return _activeColor;
            }
            if (!state.interactable) {
                return _disabledColor;
            }
            return _color;
        }
    }
}