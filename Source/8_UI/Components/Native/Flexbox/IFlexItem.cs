using UnityEngine;

namespace BeatLeader.Components {
    internal interface IFlexItem {
        float FlexShrink { get; set; }
        float FlexGrow { get; set; }
        Vector2 FlexBasis { get; set; }
        Vector2 MinSize { get; set; } 
        Vector2 MaxSize { get; set; }
        AlignSelf AlignSelf { get; set; }
        int Order { get; set; }
    }
}