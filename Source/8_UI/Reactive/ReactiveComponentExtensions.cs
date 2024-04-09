using System;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal static class ReactiveComponentExtensions {
        public static T With<T>(this T comp, Action<T> action) where T : ReactiveComponentBase {
            action(comp);
            return comp;
        }

        public static T WithModifier<T>(this T comp, ILayoutModifier modifier) where T : ReactiveComponentBase {
            comp.LayoutModifier = modifier;
            return comp;
        }

        public static T Bind<T>(this T comp, ref T variable) where T : ReactiveComponentBase {
            variable = comp;
            return comp;
        }
        
        public static T Bind<T>(this T comp, ref RectTransform variable) where T : ReactiveComponentBase {
            variable = comp.ContentTransform;
            return comp;
        }

        public static RectTransform Use(this ReactiveComponentBase comp, GameObject parent) {
            comp.Use(parent.GetOrAddComponent<RectTransform>());
            return comp.ContentTransform;
        }
    }
}