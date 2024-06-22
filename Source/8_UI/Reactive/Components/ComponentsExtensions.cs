using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
using HMUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UImage = UnityEngine.UI.Image;

namespace BeatLeader.UI.Reactive.Components {
    internal static class ComponentsExtensions {
        #region Button

        public static T WithModal<T, TModal>(
            this T button,
            TModal modal,
            RectTransform? anchor = null,
            ModalSystemHelper.RelativePlacement placement = ModalSystemHelper.RelativePlacement.Center,
            Vector2 offset = default,
            bool animateBackground = false,
            Optional<DynamicShadowSettings> shadowSettings = default
        ) where T : Button where TModal : IModal, IReactiveComponent, new() {
            shadowSettings.SetValueIfNotSet(new());
            button.ClickEvent += () => {
                ModalSystemHelper.OpenModalRelatively(
                    modal,
                    button.ContentTransform,
                    anchor ?? button.ContentTransform,
                    placement,
                    offset,
                    animateBackground,
                    shadowSettings: shadowSettings
                );
            };
            return button;
        }

        public static T WithModal<T>(this T button, Action<Transform> listener) where T : Button {
            button.ClickEvent += () => listener(button.ContentTransform);
            return button;
        }

        public static T WithClickListener<T>(this T button, Action listener) where T : Button {
            button.ClickEvent += listener;
            return button;
        }

        public static T WithStateListener<T>(this T button, Action<bool> listener) where T : Button {
            button.StateChangedEvent += listener;
            return button;
        }

        [Obsolete]
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
            button.Colors = new StateColorSet {
                DisabledColor = color.ColorWithAlpha(0.25f),
                HoveredColor = color.ColorWithAlpha(0.7f),
                Color = color.ColorWithAlpha(0.4f),
            };
            return button;
        }

        #endregion

        #region ReeWrapper

        public static ReeWrapperV2<TRee> BindRee<TRee>(this ReeWrapperV2<TRee> comp, ref TRee variable)
            where TRee : ReeUIComponentV2 {
            variable = comp.ReeComponent;
            return comp;
        }

        public static ReeWrapperV3<TRee> BindRee<TRee>(this ReeWrapperV3<TRee> comp, ref TRee variable)
            where TRee : ReeUIComponentV3<TRee> {
            variable = comp.ReeComponent;
            return comp;
        }

        #endregion

        #region Image

        public static T AsBlurBackground<T>(this T comp, float pixelsPerUnit = 12f, Color? color = null) where T : Image {
            comp.Sprite ??= BundleLoader.Sprites.background;
            comp.Material = GameResources.UIFogBackgroundMaterial;
            comp.Color = color ?? comp.Color;
            comp.PixelsPerUnit = pixelsPerUnit;
            return comp;
        }

        public static T AsBackground<T>(
            this T component,
            Optional<Sprite> sprite = default,
            Optional<Material> material = default,
            Color? color = null,
            UImage.Type type = UImage.Type.Sliced,
            float pixelsPerUnit = 10f,
            ImageView.GradientDirection? gradientDirection = null,
            Color gradientColor0 = default,
            Color gradientColor1 = default
        ) where T : Image {
            sprite.SetValueIfNotSet(BundleLoader.Sprites.background);
            material.SetValueIfNotSet(GameResources.UINoGlowMaterial);
            //adding image
            component.Sprite = sprite;
            component.Material = material;
            component.Color = color ?? Color.white;
            component.ImageType = type;
            component.PixelsPerUnit = pixelsPerUnit;
            //applying gradient if needed
            if (gradientDirection.HasValue) {
                component.UseGradient = true;
                component.GradientDirection = gradientDirection.Value;
                component.GradientColor0 = gradientColor0;
                component.GradientColor1 = gradientColor1;
            }

            return component;
        }

        #endregion
        
        #region Other

        public static T WithAnimation<T>(this T component, Action<float> animator) where T : IObservableHost, IAnimationProgressProvider {
            return component.WithListener(static x => x.AnimationProgress, animator);
        }

        public static T WithGraphicMask<T>(this T component) where T : ReactiveComponentBase {
            component.Content.AddComponent<Mask>();
            return component;
        }

        [Obsolete("This method is unsafe and obsolete. Use Image.AsBackground instead")]
        public static T WithBackground<T>(
            this T component,
            out FixedImageView image,
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
            image = component.Content.AddComponent<FixedImageView>();
            image.sprite = sprite;
            image.material = material;
            image.color = color ?? Color.white;
            image.type = type;
            image.pixelsPerUnitMultiplier = pixelsPerUnit;
            image.Skew = skew;
            //applying gradient if needed
            if (gradientDirection.HasValue) {
                image.gradient = true;
                image.GradientDirection = gradientDirection.Value;
                image.color0 = gradientColor0;
                image.color1 = gradientColor1;
            }

            return component;
        }

        [Obsolete("This method is unsafe and obsolete. Use Image.AsBackground instead")]
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
            return component.WithBackground(
                out _,
                sprite,
                material,
                color,
                type,
                pixelsPerUnit,
                skew,
                gradientDirection,
                gradientColor0,
                gradientColor1
            );
        }

        [Obsolete("This way is unsafe and obsolete. Use Image.AsBlurBackground instead")]
        public static T WithBlurBackground<T>(
            this T component,
            Color? color = null,
            float pixelsPerUnit = 12f,
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

        [Obsolete("This way is unsafe and obsolete. Use Image.AsBlurBackground instead")]
        public static T WithBlurBackground<T>(
            this T component,
            out FixedImageView image,
            Color? color = null,
            float pixelsPerUnit = 12f,
            float skew = 0f,
            ImageView.GradientDirection? gradientDirection = null,
            Color gradientColor0 = default,
            Color gradientColor1 = default
        ) where T : ReactiveComponentBase {
            return component.WithBackground(
                out image,
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