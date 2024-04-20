using System;
using BeatLeader.UI.Reactive.Yoga;
using HMUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UImage = UnityEngine.UI.Image;

namespace BeatLeader.UI.Reactive.Components {
    internal static class ComponentsExtensions {
        #region Button

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
            button.Children.Add(
                new Label {
                    Text = text,
                    FontSize = fontSize,
                    RichText = richText,
                    Overflow = overflow
                }.WithRectExpand()
            );
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

        #endregion

        #region Segmented Control

        public static ReactiveSegmentedControl<TKey, TComponent> WithCellModifier<TKey, TComponent>(
            this ReactiveSegmentedControl<TKey, TComponent> control,
            ILayoutModifier modifier
        ) where TComponent : ReactiveComponentBase, IReactiveSegmentedControlCell<TKey>, new() {
            control.CellLayoutModifier = modifier;
            return control;
        }

        public static ReactiveSegmentedControl<TKey, TComponent> WithCellAsFlexItem<TKey, TComponent>(
            this ReactiveSegmentedControl<TKey, TComponent> control,
            float grow = 0f,
            float shrink = 1,
            YogaValue? basis = null,
            YogaVector? size = null,
            YogaVector? minSize = null,
            YogaVector? maxSize = null,
            YogaFrame? margin = null,
            YogaValue? aspectRatio = null,
            Align alignSelf = Align.Auto
        ) where TComponent : ReactiveComponentBase, IReactiveSegmentedControlCell<TKey>, new() {
            return control.AsFlexItem(
                null,
                static (comp, mod) => comp.CellLayoutModifier = mod,
                grow,
                shrink,
                basis,
                size,
                minSize,
                maxSize,
                margin,
                aspectRatio,
                alignSelf
            );
        }

        #endregion

        #region Other

        public static T WithGraphicMask<T>(this T component) where T : ReactiveComponentBase {
            component.Content.AddComponent<Mask>();
            return component;
        }

        public static T WithBackground<T>(
            this T component,
            Optional<Sprite> sprite = default,
            Optional<Material> material = default,
            Color? color = null,
            UImage.Type type = UImage.Type.Sliced,
            float pixelsPerUnit = 10f,
            float skew = 0f,
            ImageView.GradientDirection? gradientDirection = null,
            Color gradientColor0 = default,
            Color gradientColor1 = default
        ) where T : ReactiveComponentBase {
            sprite.SetValueIfNotSet(BundleLoader.WhiteBG);
            material.SetValueIfNotSet(GameResources.UINoGlowMaterial);
            //adding image
            var img = component.Content.AddComponent<FixedImageView>();
            img.sprite = sprite;
            img.material = material;
            img.color = color ?? Color.white;
            img.type = type;
            img.pixelsPerUnitMultiplier = pixelsPerUnit;
            img.Skew = skew;
            //applying gradient if needed
            if (gradientDirection.HasValue) {
                img.gradient = true;
                img.GradientDirection = gradientDirection.Value;
                img.color0 = gradientColor0;
                img.color1 = gradientColor1;
            }

            return component;
        }

        public static T WithBlurBackground<T>(
            this T component,
            Color? color = null,
            float pixelsPerUnit = 100f,
            float skew = 0f,
            ImageView.GradientDirection? gradientDirection = null,
            Color gradientColor0 = default,
            Color gradientColor1 = default
        ) where T : ReactiveComponentBase {
            return component.WithBackground(
                sprite: BundleLoader.WhiteBG,
                material: GameResources.UIFogBackgroundMaterial,
                color: color,
                pixelsPerUnit: pixelsPerUnit,
                skew: skew,
                gradientDirection: gradientDirection,
                gradientColor0: gradientColor0,
                gradientColor1: gradientColor1
            );
        }

        #endregion
    }
}