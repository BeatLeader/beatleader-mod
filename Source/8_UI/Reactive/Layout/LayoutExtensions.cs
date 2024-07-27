using System;
using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal static class LayoutExtensions {
        #region Flex Item

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
        ) where T : ILayoutItem {
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
        ) where T : ILayoutItem {
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
        ) where T : ILayoutItem {
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

        #endregion

        #region Flex Group

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
            return AsYogaFlexGroup<T, YogaLayoutController>(
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
            return AsYogaFlexGroup(
                component,
                out layoutController,
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

        #endregion

        #region Flex Root Group

        public static T AsRootFlexGroup<T>(
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
            return AsYogaFlexGroup<T, YogaSelfLayoutController>(
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

        public static T AsRootFlexGroup<T>(
            this T component,
            out YogaSelfLayoutController layoutController,
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
            return AsYogaFlexGroup(
                component,
                out layoutController,
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

        #endregion

        #region Flex Group Tools

        private static T AsYogaFlexGroup<T, TController>(
            this T component,
            out TController layoutController,
            FlexDirection direction = FlexDirection.Row,
            Justify justifyContent = Justify.SpaceAround,
            Align alignItems = Align.Stretch,
            Align alignContent = Align.Auto,
            Wrap wrap = Wrap.NoWrap,
            Overflow overflow = Overflow.Visible,
            YogaFrame? padding = null,
            YogaVector? gap = null,
            bool independentLayout = false
        ) where T : ILayoutDriver where TController : YogaLayoutController, new() {
            if (component.LayoutController is not TController controller) {
                controller = new();
            }
            component.LayoutController = controller;
            controller.FlexDirection = direction;
            controller.JustifyContent = justifyContent;
            controller.AlignContent = alignContent;
            controller.AlignItems = alignItems;
            controller.FlexWrap = wrap;
            controller.Overflow = overflow;
            controller.Padding = padding ?? controller.Padding;
            controller.Gap = gap ?? controller.Gap;
            controller.UseIndependentLayout = independentLayout;

            layoutController = controller;
            return component;
        }

        #endregion

        #region Rect

        public static T WithSizeDelta<T>(
            this T component,
            float width,
            float height
        ) where T : IReactiveComponent {
            return component.AsRectItem(new(width, height));
        }

        public static T AsRectItem<T>(
            this T component,
            Vector2? sizeDelta = null,
            Vector2? anchorMin = null,
            Vector2? anchorMax = null,
            Vector2? pivot = null
        ) where T : IReactiveComponent {
            var transform = component.ContentTransform;
            transform.anchorMin = anchorMin ?? transform.anchorMin;
            transform.anchorMax = anchorMax ?? transform.anchorMax;
            transform.sizeDelta = sizeDelta ?? transform.sizeDelta;
            transform.pivot = pivot ?? transform.pivot;
            return component;
        }

        public static T WithRectExpand<T>(this T component) where T : IReactiveComponent {
            component.ContentTransform.WithRectExpand();
            return component;
        }

        public static ILayoutItem WithRectExpand(this ILayoutItem component) {
            component.ApplyTransforms(x => x.WithRectExpand());
            return component;
        }

        public static RectTransform WithRectExpand(this RectTransform component) {
            component.anchorMin = Vector2.zero;
            component.anchorMax = Vector2.one;
            component.sizeDelta = Vector2.zero;
            component.anchoredPosition = Vector2.zero;
            return component;
        }

        #endregion

        public static T With<T>(this T modifier, ILayoutModifier apModifier) where T : ILayoutModifier {
            modifier.CopyFrom(apModifier);
            return modifier;
        }
    }
}