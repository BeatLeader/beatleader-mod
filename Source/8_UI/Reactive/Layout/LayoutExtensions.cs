using BeatLeader.Components;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal static class LayoutExtensions {
        public static T FlexItem<T>(
            this T component,
            float grow = 1,
            Vector2? basis = null,
            Vector2? minSize = null,
            Vector2? maxSize = null,
            float shrink = 1,
            AlignSelf alignSelf = AlignSelf.Auto
        ) where T : ReactiveComponent {
            component.Modifier = new FlexModifier {
                FlexShrink = shrink,
                FlexGrow = grow,
                FlexBasis = basis ?? Vector2.one * -1,
                MinSize = minSize ?? Vector2.one * -1,
                MaxSize = maxSize ?? Vector2.one * -1,
                AlignSelf = alignSelf
            };
            return component;
        }
        
        public static T FlexGroup<T>(
            this T component,
            FlexDirection direction = FlexDirection.Row,
            JustifyContent justifyContent = JustifyContent.SpaceAround,
            AlignItems alignItems = AlignItems.Stretch
        ) where T : ReactiveComponent {
            component.Apply<T, Components.Flexbox>(
                x => {
                    x.FlexDirection = direction;
                    x.JustifyContent = justifyContent;
                    x.AlignItems = alignItems;
                });
            return component;
        }
        
        public static T Expand<T>(this T component) where T : ReactiveComponent {
            var modifier = component.Modifier;
            if (modifier is RectModifier rect) {
                rect.With(RectModifier.Expand);
            } else if (modifier is FlexModifier flex) {
                flex.With(FlexModifier.Expand);
            }
            modifier.Refresh();
            return component;
        }
        
        public static T With<T>(this T modifier, ILayoutModifier apModifier) where T : ILayoutModifier {
            modifier.CopyFrom(apModifier);
            return modifier;
        }
    }
}