using BeatLeader.UI.Reactive.Yoga;
using ReactiveUI.Layout;

namespace BeatLeader.UI.Reactive {
    internal static class LayoutExtensions {
        #region Flex

        public static T FlexItem<T>(
            this T component,
            float grow = 0f,
            float shrink = 1,
            YogaValue? basis = null,
            YogaVector? size = null,
            YogaVector? minSize = null,
            YogaVector? maxSize = null,
            YogaFrame? margin = null,
            Align alignSelf = Align.Auto
        ) where T : ReactiveComponent {
            if (component.LayoutModifier is not YogaModifier modifier) {
                modifier = new YogaModifier();
                component.LayoutModifier = modifier;
            }
            if (size == null && !ExpandFlexChild(component)) {
                modifier.Size = YogaVector.Undefined;
            }
            modifier.FlexShrink = shrink;
            modifier.FlexGrow = grow;
            modifier.FlexBasis = basis ?? YogaValue.Undefined;
            modifier.MinSize = minSize ?? YogaVector.Undefined;
            modifier.MaxSize = maxSize ?? YogaVector.Undefined;
            modifier.AlignSelf = alignSelf;
            modifier.Margin = margin ?? YogaFrame.Zero;
            return component;
        }

        public static T FlexGroup<T>(
            this T component,
            FlexDirection direction = FlexDirection.Row,
            Justify justifyContent = Justify.SpaceAround,
            Align alignItems = Align.Stretch,
            Wrap wrap = Wrap.NoWrap,
            YogaFrame? padding = null,
            bool expandUnspecifiedChildren = true
        ) where T : ReactiveComponent {
            component.SetLayoutController(
                new YogaLayoutController {
                    FlexDirection = direction,
                    JustifyContent = justifyContent,
                    AlignItems = alignItems,
                    FlexWrap = wrap,
                    Padding = padding ?? YogaFrame.Zero
                }
            );
            if (expandUnspecifiedChildren) {
                foreach (var comp in component.Children) {
                    ExpandFlexChild(comp);
                }
            }
            return component;
        }

        private static bool ExpandFlexChild(ReactiveComponent component) {
            if (component.LayoutModifier is not YogaModifier modifier) return false;
            var sizeIsUndefined = modifier.Size == YogaVector.Auto || modifier.Size == YogaVector.Undefined;

            if (component.Parent?.LayoutController is not YogaLayoutController controller || !sizeIsUndefined) return false;
            var row = controller.FlexDirection is FlexDirection.Row or FlexDirection.RowReverse;
            modifier.Size = row ? new(0f, "100%") : new("100%", 0f);

            return true;
        }

        #endregion

        #region Rect

        public static T Expand<T>(this T component) where T : ReactiveComponent {
            if (component.LayoutModifier is not RectModifier modifier) {
                modifier = new();
            }
            modifier.With(RectModifier.Expand);
            component.LayoutModifier = modifier;
            return component;
        }

        #endregion

        public static T With<T>(this T modifier, ILayoutModifier apModifier) where T : ILayoutModifier {
            modifier.CopyFrom(apModifier);
            return modifier;
        }
    }
}