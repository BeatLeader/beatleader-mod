using System.Collections.Generic;
using System.Linq;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UImage = UnityEngine.UI.Image;

namespace BeatLeader.UI.Reactive.Components {
    internal static class ComponentsExtensions {
        #region Button

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
                        ((ISkewedComponent)x).Skew = skewed.Skew;
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
    }
}