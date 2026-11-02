using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal static class ModalExtensions {
        #region Placement

        public static T WithPlacement<T>(
            this T modal,
            RelativePlacement placement,
            RectTransform? anchor = null,
            Vector2? offset = null
        ) where T : ModalBase {
            modal.PlacementData = new() {
                Placement = placement,
                Offset = offset ?? new(0f, 0.5f),
                Clip = true
            };

            if (anchor != null) {
                modal.PlacementAnchor = anchor;
            }

            return modal;
        }

        #endregion

        #region Shadow

        public static Layout InShadow(this ILayoutItem content, DynamicShadowSettings? settings = null) {
            return new Layout {
                FlexController = {
                    ConstrainHorizontal = false,
                    ConstrainVertical = false
                },

                Children = {
                    new DynamicShadow {
                        Settings = settings ?? new()
                    },
                    content
                }
            };
        }

        #endregion
    }
}