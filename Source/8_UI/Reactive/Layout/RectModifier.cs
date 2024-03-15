using BeatLeader.UI.Reactive;
using UnityEngine;

namespace ReactiveUI.Layout {
    internal class RectModifier : ModifierBase<RectModifier> {
        public static RectModifier Expand => new() {
            AnchorMin = Vector2.zero,
            AnchorMax = Vector2.one,
            SizeDelta = Vector2.zero
        };

        public Vector2? SizeDelta {
            get => _sizeDelta;
            set {
                _sizeDelta = value;
                Refresh();
            }
        }

        public Vector2 AnchorMin {
            get => _anchorMin;
            set {
                _anchorMin = value;
                Refresh();
            }
        }

        public Vector2 AnchorMax {
            get => _anchorMax;
            set {
                _anchorMax = value;
                Refresh();
            }
        }

        private Vector2? _sizeDelta;
        private Vector2 _anchorMin = Vector2.zero;
        private Vector2 _anchorMax = Vector2.one;
        
        public override void CopyFromSimilar(RectModifier similar) {
            SizeDelta = similar.SizeDelta;
            AnchorMin = similar.AnchorMin;
            AnchorMax = similar.AnchorMax;
        }
    }
}