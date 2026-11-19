using Reactive;

namespace BeatLeader.UI.Reactive.Components {
    internal static class ModalExtensions {
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