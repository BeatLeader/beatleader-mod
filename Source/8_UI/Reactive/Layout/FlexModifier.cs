using BeatLeader.Components;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal class FlexModifier : ModifierBase<FlexModifier>, IFlexItem {
        public FlexModifier() {
            AnchorMin = Vector2.up;
            AnchorMax = Vector2.up;
        }

        public static FlexModifier Expand => new() {
            FlexGrow = 1
        };

        public float FlexShrink { get; set; } = -1;
        public float FlexGrow { get; set; } = -1;
        public Vector2 FlexBasis { get; set; } = Vector2.one * -1;
        public Vector2 MinSize { get; set; } = Vector2.one * -1;
        public Vector2 MaxSize { get; set; } = Vector2.one * -1;
        public AlignSelf AlignSelf { get; set; }
        public int Order { get; set; }

        public override void CopyFromSimilar(FlexModifier modifier) {
            FlexShrink = modifier.FlexShrink;
            FlexGrow = modifier.FlexGrow;
            FlexBasis = modifier.FlexBasis;
            MinSize = modifier.MinSize;
            MaxSize = modifier.MaxSize;
            AlignSelf = modifier.AlignSelf;
            Order = modifier.Order;
        }
    }
}