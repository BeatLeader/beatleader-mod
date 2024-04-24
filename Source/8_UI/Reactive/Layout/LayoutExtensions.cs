using System;
using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal static class LayoutExtensions {
        #region Flex

        public static T AsFlexItem<T>(
            this T component,
            out YogaModifier modifier,
            float grow = 0f,
            float shrink = 1f,
            YogaValue? basis = null,
            YogaVector? size = null,
            YogaVector? minSize = null,
            YogaVector? maxSize = null,
            YogaFrame? margin = null,
            YogaValue? aspectRatio = null,
            YogaFrame? position = null,
            PositionType? positionType = null,
            Align alignSelf = Align.Auto
        ) where T : ReactiveComponentBase {
            return AsFlexItem(
                component,
                component.LayoutModifier,
                static (comp, mod) => comp.LayoutModifier = mod,
                out modifier,
                grow,
                shrink,
                basis,
                size,
                minSize,
                maxSize,
                margin,
                aspectRatio,
                position,
                positionType,
                alignSelf
            );
        }

        public static T AsFlexItem<T>(
            this T component,
            float grow = 0f,
            float shrink = 1f,
            YogaValue? basis = null,
            YogaVector? size = null,
            YogaVector? minSize = null,
            YogaVector? maxSize = null,
            YogaFrame? margin = null,
            YogaValue? aspectRatio = null,
            YogaFrame? position = null,
            PositionType? positionType = null,
            Align alignSelf = Align.Auto
        ) where T : ReactiveComponentBase {
            return AsFlexItem(
                component,
                out _,
                grow,
                shrink,
                basis,
                size,
                minSize,
                maxSize,
                margin,
                aspectRatio,
                position,
                positionType,
                alignSelf
            );
        }

        public static T AsFlexItem<T>(
            this T component,
            ILayoutModifier? layoutModifier,
            Action<T, ILayoutModifier> setModifierCallback,
            out YogaModifier yogaModifier,
            float grow = 0f,
            float shrink = 1f,
            YogaValue? basis = null,
            YogaVector? size = null,
            YogaVector? minSize = null,
            YogaVector? maxSize = null,
            YogaFrame? margin = null,
            YogaValue? aspectRatio = null,
            YogaFrame? position = null,
            PositionType? positionType = null,
            Align alignSelf = Align.Auto
        ) where T : ReactiveComponentBase {
            if (layoutModifier is not YogaModifier modifier) {
                modifier = new YogaModifier();
                setModifierCallback(component, modifier);
            }
            if (position != null) {
                modifier.PositionType = PositionType.Absolute;
                modifier.Position = position.Value;
            }
            if (positionType != null) {
                modifier.PositionType = positionType.Value;
            }
            modifier.Size = size ?? modifier.Size;
            modifier.FlexShrink = shrink;
            modifier.FlexGrow = grow;
            modifier.FlexBasis = basis ?? modifier.FlexBasis;
            modifier.MinSize = minSize ?? modifier.MinSize;
            modifier.MaxSize = maxSize ?? modifier.MaxSize;
            modifier.Margin = margin ?? modifier.Margin;
            modifier.AspectRatio = aspectRatio ?? modifier.AspectRatio;
            modifier.AlignSelf = alignSelf;
            yogaModifier = modifier;
            return component;
        }

        public static T AsFlexGroup<T>(
            this T component,
            FlexDirection direction = FlexDirection.Row,
            Justify justifyContent = Justify.SpaceAround,
            Align alignItems = Align.Stretch,
            Align alignContent = Align.Auto,
            Wrap wrap = Wrap.NoWrap,
            Overflow overflow = Overflow.Visible,
            YogaFrame? padding = null,
            YogaVector? gap = null,
            bool independentLayout = false
        ) where T : ILayoutDriver {
            return AsFlexGroup(
                component,
                out _,
                direction,
                justifyContent,
                alignItems,
                alignContent,
                wrap,
                overflow,
                padding,
                gap,
                independentLayout
            );
        }

        public static T AsFlexGroup<T>(
            this T component,
            out YogaLayoutController layoutController,
            FlexDirection direction = FlexDirection.Row,
            Justify justifyContent = Justify.SpaceAround,
            Align alignItems = Align.Stretch,
            Align alignContent = Align.Auto,
            Wrap wrap = Wrap.NoWrap,
            Overflow overflow = Overflow.Visible,
            YogaFrame? padding = null,
            YogaVector? gap = null,
            bool independentLayout = false
        ) where T : ILayoutDriver {
            if (component.LayoutController is not YogaLayoutController controller) {
                controller = new();
            }
            component.LayoutController = controller;
            controller.FlexDirection = direction;
            controller.JustifyContent = justifyContent;
            controller.AlignContent = alignContent;
            controller.AlignItems = alignItems;
            controller.FlexWrap = wrap;
            controller.Overflow = overflow;
            controller.Padding = padding ?? YogaFrame.Zero;
            controller.Gap = gap ?? YogaVector.Undefined;
            controller.UseIndependentLayout = independentLayout;

            layoutController = controller;
            return component;
        }

        #endregion

        #region Rect

        public static T WithRectSize<T>(
            this T component,
            float height,
            float width
        ) where T : ReactiveComponentBase {
            return component.AsRectItem(new(width, height));
        }

        public static T AsRectItem<T>(
            this T component,
            Vector2 sizeDelta = default,
            Vector2 anchorMin = default,
            Vector2 anchorMax = default
        ) where T : ReactiveComponentBase {
            if (component.LayoutModifier is not RectModifier modifier) {
                modifier = new();
            }
            component.LayoutModifier = modifier;
            modifier.AnchorMin = anchorMin;
            modifier.AnchorMax = anchorMax;
            modifier.SizeDelta = sizeDelta;
            return component;
        }

        public static T WithRectExpand<T>(this T component) where T : ReactiveComponentBase {
            if (component.LayoutModifier is not RectModifier modifier) {
                modifier = new();
            }
            modifier.With(RectModifier.Expand);
            component.LayoutModifier = modifier;
            return component;
        }

        public static RectTransform WithRectExpand(this RectTransform component) {
            component.anchorMin = Vector2.zero;
            component.anchorMax = Vector2.one;
            component.sizeDelta = Vector2.zero;
            return component;
        }

        #endregion

        public static T With<T>(this T modifier, ILayoutModifier apModifier) where T : ILayoutModifier {
            modifier.CopyFrom(apModifier);
            return modifier;
        }
    }
}