using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal abstract class ModifierBase<T> : ModifierBase where T : ILayoutModifier {
        public override void CopyFrom(Reactive.ILayoutModifier mod) {
            base.CopyFrom(mod);
            if (mod is T similar) CopyFromSimilar(similar);
        }

        public virtual void CopyFromSimilar(T similar) { }
    }

    internal class ModifierBase : ILayoutModifier {
        public Vector2 Pivot { get; set; } = Vector2.one * 0.5f;

        public event Action? ModifierUpdatedEvent;

        public void Refresh() {
            ModifierUpdatedEvent?.Invoke();
        }

        public virtual void CopyFrom(Reactive.ILayoutModifier mod) {
            Pivot = mod.Pivot;
        }
    }
}