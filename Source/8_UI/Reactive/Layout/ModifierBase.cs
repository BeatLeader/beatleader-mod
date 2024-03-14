using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal abstract class ModifierBase<T> : ILayoutModifier where T : ILayoutModifier {
        public Vector3 Scale { get; set; } = Vector3.one;
        public Vector2? SizeDelta { get; set; } 
        public Vector2 AnchorMax { get; set; } = Vector2.one;
        public Vector2 AnchorMin { get; set; } = Vector2.zero;
        public Vector2 Pivot { get; set; } = Vector2.one * 0.5f;

        public event Action? ModifierUpdatedEvent;

        public void Refresh() {
            ModifierUpdatedEvent?.Invoke();
        }

        public virtual void CopyFrom(ILayoutModifier mod) {
            Scale = mod.Scale;
            SizeDelta = mod.SizeDelta;
            AnchorMin = mod.AnchorMin;
            AnchorMax = mod.AnchorMax;
            Pivot = mod.Pivot;
            if (mod is T similar) CopyFromSimilar(similar);
        }

        public virtual void CopyFromSimilar(T similar) { }
    }
}