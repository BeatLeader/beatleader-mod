using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal struct ReadOnlyColorSet : IColorSet {
        public Color DisabledColor;
        public Color Color;
        public Color ActiveColor;
        public Color HoveredColor;
        public Color? HoveredActiveColor;

        public event Action? SetUpdatedEvent;
        
        public Color GetColor(GraphicElementState state) {
            if (state.hovered) {
                return state.active ? HoveredActiveColor.GetValueOrDefault(ActiveColor) : HoveredColor;
            }
            if (state.active) {
                return ActiveColor;
            }
            if (!state.interactable) {
                return DisabledColor;
            }
            return Color;
        }
    }
}