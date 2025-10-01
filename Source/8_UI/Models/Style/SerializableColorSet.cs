using System;
using BeatLeader.Models;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader {
    /// <summary>
    /// A simple serializable color set for general purposes.
    /// </summary>
    internal class SerializableColorSet : IColorSet {
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

        public SerializableColor NotInteractableColor {
            get => _notInteractableColor;
            set {
                _notInteractableColor = value;
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

        private SerializableColor _notInteractableColor;
        private SerializableColor _activeColor;
        private SerializableColor _hoveredColor;
        private SerializableColor _color;

        public event Action? SetUpdatedEvent;

        public Color GetColor(GraphicState state) {
            if (state.IsHovered()) {
                return state.IsActive() ? ActiveColor : _hoveredColor;
            }
            if (state.IsActive()) {
                return _activeColor;
            }
            if (!state.IsInteractable()) {
                return _notInteractableColor;
            }
            return _color;
        }

        public static implicit operator SerializableColorSet(SimpleColorSet set) {
            return new SerializableColorSet {
                _activeColor = set.ActiveColor,
                _hoveredColor = set.HoveredColor,
                _notInteractableColor = set.NotInteractableColor,
                _color = set.Color
            };
        }
    }
}