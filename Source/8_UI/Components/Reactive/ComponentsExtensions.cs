using System.Collections.Generic;
using System.Linq;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using TMPro;

namespace BeatLeader.UI.Reactive.Components {
    internal static class ComponentsExtensions {
        #region Button

        public static T WithLocalizedLabel<T>(
            this T button,
            string token,
            float fontSize = 4f,
            bool richText = true,
            TextOverflowModes overflow = TextOverflowModes.Overflow
        ) where T : ButtonBase, ILayoutDriver {
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
        ) where T : ButtonBase, ILayoutDriver {
            return button.WithLabel(
                out label,
                BLLocalization.GetTranslation(token),
                fontSize,
                richText,
                overflow
            );
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