using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal struct ReadOnlyColorSet : IColorSet {
        public Color DisabledColor;
        public Color ActiveColor;
        public Color HoveredColor;
        public Color Color;

        public event Action? SetUpdatedEvent;
        
        public Color GetColor(GraphicElementState state) {
            if (state.active) {
                return ActiveColor;
            }
            if (state.hovered) {
                return HoveredColor;
            }
            if (!state.interactable) {
                return DisabledColor;
            }
            return Color;
        }
    }
}