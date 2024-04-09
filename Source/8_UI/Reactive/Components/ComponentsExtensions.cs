using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.Reactive.Components {
    internal static class ComponentsExtensions {
        public static T WithGraphicMask<T>(this T component) where T : ReactiveComponentBase {
            component.Content.AddComponent<Mask>();
            return component;
        }

        public static T WithClickListener<T>(this T button, Action listener) where T : Button {
            button.ClickEvent += listener;
            return button;
        }
        
        public static T WithClickListener<T>(this T button, Action<bool> listener) where T : Button {
            button.StateChangedEvent += listener;
            return button;
        }

        public static T WithLabel<T>(
            this T button,
            string text,
            float fontSize = 4f,
            bool richText = true,
            TextOverflowModes overflow = TextOverflowModes.Overflow
        ) where T : Button {
            button.Children.Add(new Label {
                Text = text,
                FontSize = fontSize,
                RichText = richText,
                Overflow = overflow
            }.WithRectExpand());
            return button;
        }

        public static T WithAccentColor<T>(
            this T button,
            Color color
        ) where T : ColoredButton {
            button.HoverColor = color.ColorWithAlpha(0.7f);
            button.Color = color.ColorWithAlpha(0.4f);
            button.DisabledColor = color.ColorWithAlpha(0.25f);
            return button;
        }
    }
}