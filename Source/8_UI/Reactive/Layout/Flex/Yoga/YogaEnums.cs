// Reference: https://github.com/facebook/yoga/blob/main/yoga/YGEnums.h
using System;

namespace BeatLeader.UI.Reactive.Yoga {
    internal enum Align {
        Auto,
        FlexStart,
        Center,
        FlexEnd,
        Stretch,
        Baseline,
        SpaceBetween,
        SpaceAround,
        SpaceEvenly,
    }

    internal enum Dimension {
        Width,
        Height,
    }

    internal enum Direction {
        Inherit,
        LeftToRight,
        RightToLeft,
    }

    internal enum Display {
        Flex,
        None,
    }

    internal enum Edge {
        Left,
        Top,
        Right,
        Bottom,
        Start,
        End,
        Horizontal,
        Vertical,
        All,
    }

    [Flags]
    internal enum Errata {
        None = 0,
        StretchFlexBasis = 1,
        StartingEndingEdgeFromFlexDirection = 2,
        PositionStaticBehavesLikeRelative = 4,
        All = 2147483647,
        Classic = 2147483646,
    }

    internal enum ExperimentalFeature {
        WebFlexBasis,
        AbsolutePercentageAgainstPaddingEdge,
    }

    [Flags]
    internal enum ExperimentalFeatureFlags {
        WebFlexBasis = 1 << ExperimentalFeature.WebFlexBasis,
        AbsolutePercentageAgainstPaddingEdge = 1 << ExperimentalFeature.AbsolutePercentageAgainstPaddingEdge,
    }

    internal enum FlexDirection {
        Column,
        ColumnReverse,
        Row,
        RowReverse,
    }

    internal enum Justify {
        FlexStart,
        Center,
        FlexEnd,
        SpaceBetween,
        SpaceAround,
        SpaceEvenly,
    }

    internal enum Gutter {
        Column,
        Row,
        All
    }

    internal enum Overflow {
        Visible,
        Hidden,
        Scroll,
    }

    internal enum PositionType {
        Static,
        Relative,
        Absolute,
    }

    [Flags]
    internal enum PrintOptions {
        Layout = 1,
        Style = 2,
        Children = 4,
    }

    internal enum Unit {
        Undefined,
        Point,
        Percent,
        Auto,
    }

    internal enum Wrap {
        NoWrap,
        Wrap,
        WrapReverse,
    }
}