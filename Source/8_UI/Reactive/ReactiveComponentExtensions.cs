using System;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal static class ReactiveComponentExtensions {
        public static T With<T>(this T comp, Action<T> action) where T : IReactiveComponent {
            action(comp);
            return comp;
        }
        
        public static T WithNativeComponent<T, TComp>(this T comp, out TComp ncomp) where T : IReactiveComponent where TComp : Component {
            ncomp = comp.Content.AddComponent<TComp>();
            return comp;
        }

        public static T WithModifier<T>(this T comp, ILayoutModifier modifier) where T : ILayoutItem {
            comp.LayoutModifier = modifier;
            return comp;
        }

        public static T Bind<T>(this T comp, ref T variable) where T : IReactiveComponent {
            variable = comp;
            return comp;
        }
        
        public static T Export<T>(this T comp, out T variable) where T : IReactiveComponent {
            variable = comp;
            return comp;
        }
        
        public static T Bind<T>(this T comp, ref RectTransform variable) where T : IReactiveComponent {
            variable = comp.ContentTransform;
            return comp;
        }

        public static RectTransform Use(this ReactiveComponentBase comp, GameObject parent) {
            comp.Use(parent.GetOrAddComponent<RectTransform>());
            return comp.ContentTransform;
        }
    }
}