using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
using BeatLeader.UI.Reactive.Yoga;
using HMUI;
using JetBrains.Annotations;
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
        ) where T : ButtonBase where TModal : IModal, IReactiveComponent, new() {
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

        public static T WithCenteredModal<T, TModal>(
            this T button,
            TModal modal,
            bool animateBackground = false,
            Optional<DynamicShadowSettings> shadowSettings = default
        ) where T : ButtonBase where TModal : IModal, IReactiveComponent, new() {
            shadowSettings.SetValueIfNotSet(new());
            button.ClickEvent += () => ModalSystem.OpenModal(
                modal,
                button.ContentTransform,
                settings: new(
                    Vector2.zero,
                    Vector2.one * 0.5f,
                    animateBackground,
                    ShadowSettings: shadowSettings
                )
            );
            return button;
        }
        
        public static T WithModal<T>(this T button, Action<Transform> listener) where T : IClickableComponent, IReactiveComponent {
            button.ClickEvent += () => listener(button.ContentTransform);
            return button;
        }

        public static T WithClickListener<T>(this T button, Action listener) where T : IClickableComponent {
            button.ClickEvent += listener;
            return button;
        }

        public static T WithStateListener<T>(this T button, Action<bool> listener) where T : ButtonBase {
            button.StateChangedEvent += listener;
            return button;
        }

        [Obsolete]
        public static T WithClickListener<T>(this T button, Action<bool> listener) where T : Button {
            button.StateChangedEvent += listener;
            return button;
        }

        public static T WithLocalizedLabel<T>(
            this T button,
            string token,
            float fontSize = 4f,
            bool richText = true,
            TextOverflowModes overflow = TextOverflowModes.Overflow
        ) where T : ButtonBase, IChildrenProvider {
            return WithLocalizedLabel(
                button,
                out _,
                token,
                fontSize,
                richText,
                overflow
            );
        }
        
        
        public static T WithLocalizedLabel<T>(
            this T button,
            out Label label,
            string token,
            float fontSize = 4f,
            bool richText = true,
            TextOverflowModes overflow = TextOverflowModes.Overflow
        ) where T : ButtonBase, IChildrenProvider {
            return WithLabel(
                button,
                out label,
                BLLocalization.GetTranslation(token),
                fontSize,
                richText,
                overflow
            );
        }
        
        public static T WithLabel<T>(
            this T button,
            string text,
            float fontSize = 4f,
            bool richText = true,
            TextOverflowModes overflow = TextOverflowModes.Overflow
        ) where T : ButtonBase, IChildrenProvider {
            return WithLabel(
                button,
                out _,
                text,
                fontSize,
                richText,
                overflow
            );
        }

        public static T WithLabel<T>(
            this T button,
            out Label label,
            string text,
            float fontSize = 4f,
            bool richText = true,
            TextOverflowModes overflow = TextOverflowModes.Overflow
        ) where T : ButtonBase, IChildrenProvider {
            button.AsFlexGroup(alignItems: Align.Center);
            button.Children.Add(
                new Label {
                    Text = text,
                    FontSize = fontSize,
                    RichText = richText,
                    Overflow = overflow
                }.With(
                    x => {
                        if (button is not ISkewedComponent skewed) return;
                        x.Skew = skewed.Skew;
                    }
                ).AsFlexItem(
                    size: "auto",
                    margin: new() { left = 2f, right = 2f }
                ).Export(out label)
            );
            return button;
        }

        public static T WithImage<T>(
            this T button,
            Sprite? sprite,
            Color? color = null,
            float? pixelsPerUnit = null,
            bool preserveAspect = true,
            UImage.Type type = UImage.Type.Simple,
            Optional<Material>? material = default
        ) where T : ButtonBase, IChildrenProvider {
            return WithImage(
                button,
                out _,
                sprite,
                color,
                pixelsPerUnit,
                preserveAspect,
                type,
                material
            );
        }

        public static T WithImage<T>(
            this T button,
            out Image image,
            Sprite? sprite,
            Color? color = null,
            float? pixelsPerUnit = null,
            bool preserveAspect = true,
            UImage.Type type = UImage.Type.Simple,
            Optional<Material>? material = default
        ) where T : ButtonBase, IChildrenProvider {
            button.Children.Add(
                new Image {
                    Sprite = sprite,
                    Material = material.GetValueOrDefault(GameResources.UINoGlowMaterial),
                    Color = color ?? Color.white,
                    PixelsPerUnit = pixelsPerUnit ?? 0f,
                    ImageType = pixelsPerUnit == null ? type : UImage.Type.Sliced,
                    PreserveAspect = preserveAspect
                }.With(
                    x => {
                        if (button is not ISkewedComponent skewed) return;
                        x.Skew = skewed.Skew;
                    }
                ).AsFlexItem(grow: 1f).Export(out image)
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

        #region Label

        public static T SetLocalizedText<T>(this T comp, string token) where T : Label {
            comp.Text = BLLocalization.GetTranslation(token);
            return comp;
        }

        #endregion
        
        #region ReeWrapper

        public static ReeWrapperV2<TRee> BindRee<TRee>(this ReeWrapperV2<TRee> comp, ref TRee variable)
            where TRee : ReeUIComponentV2 {
            variable = comp.ReeComponent;
            return comp;
        }

        #endregion

        #region Image

        [Pure]
        public static Image InBackground(
            this ILayoutItem comp,
            Optional<Sprite> sprite = default,
            Optional<Material> material = default,
            Color? color = null,
            UImage.Type type = UImage.Type.Sliced,
            float pixelsPerUnit = 10f,
            float skew = 0f,
            ImageView.GradientDirection? gradientDirection = null,
            Color gradientColor0 = default,
            Color gradientColor1 = default
        ) {
            return comp.In<Image>().AsBackground(
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

        [Pure]
        public static Image InBlurBackground(
            this ILayoutItem comp,
            float pixelsPerUnit = 12f,
            Color? color = null
        ) {
            return comp.In<Image>().AsBlurBackground(
                pixelsPerUnit,
                color
            );
        }

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
            float skew = 0f,
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
            component.Skew = skew;
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

        #region NamedRail

        [Pure]
        public static NamedRail InNamedRail(this ILayoutItem comp, string text) {
            return new NamedRail {
                Label = {
                    Text = text
                },
                Component = comp
            }.With(
                x => {
                    if (comp is not ISkewedComponent skewed) return;
                    x.Label.Skew = skewed.Skew;
                }
            );
        }

        #endregion

        #region LoadingContainer

        public static LoadingContainer InLoadingContainer(this ILayoutItem comp) {
            return new LoadingContainer {
                LayoutModifier = comp.LayoutModifier?.CreateCopy(),
                Component = comp
            };
        }
        
        #endregion
        
        #region TextArea

        public static T WithItemsText<T>(this T comp, IEnumerable<string> items, bool silent = false) where T : TextArea {
            var text = string.Join(string.Empty, items.Select((x, idx) => $"{(idx > 0 ? ", " : "")}{x}"));
            if (silent) {
                comp.SetTextSilent(text);
            } else {
                comp.Text = text;
            }
            return comp;
        }

        #endregion

        #region Other

        [Pure]
        public static T In<T>(this ILayoutItem comp) where T : DrivingReactiveComponent, new() {
            return new T {
                Children = { comp.WithRectExpand() }
            };
        }

        public static T WithAnimation<T>(this T component, Action<float> animator) where T : IObservableHost, IAnimationProgressProvider {
            return component.WithListener(static x => x.AnimationProgress, animator);
        }

        public static T WithGraphicMask<T>(this T component) where T : ReactiveComponentBase {
            component.Content.AddComponent<Mask>();
            return component;
        }

        #endregion
    }
}