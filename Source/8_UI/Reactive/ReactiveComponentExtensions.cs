using System;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal static class ReactiveComponentExtensions {
        public static T With<T>(this T comp, Action<T> action) {
            action(comp);
            return comp;
        }

        public static T WithModifier<T>(this T comp, ILayoutModifier modifier) where T : ReactiveComponent {
            comp.Modifier = modifier;
            return comp;
        }

        public static T Bind<T>(this T comp, ref T variable) where T : ReactiveComponent {
            variable = comp;
            return comp;
        }

        public static T Apply<T>(this T comp, ReactiveComponent toApply) where T : ReactiveComponent {
            toApply.Apply(comp.ContentTransform);
            return comp;
        }

        public static T Apply<T, U>(this T comp, Action<U> callback) where T : ReactiveComponent where U : ReactiveComponent, new() {
            var toApply = ReactiveComponent.Lazy<U>();
            toApply.Apply(comp);
            callback(toApply);
            return comp;
        }

        public static RectTransform Use(this ReactiveComponent comp, GameObject parent) {
            comp.Use(parent.GetOrAddComponent<RectTransform>());
            return comp.ContentTransform;
        }
    }
}