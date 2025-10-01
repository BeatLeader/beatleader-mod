using Reactive;
using Reactive.Components;

namespace BeatLeader.UI.Reactive.Components {
    internal static class ModalExtensions {
        #region WithShadow

        public static T WithShadow<T>(this T comp, DynamicShadowSettings? settings = null) where T : IReactiveComponent, IModal {
            settings ??= new();
            DynamicShadow? shadow = null;
            comp.ModalOpenedEvent += HandleModalOpened;
            comp.ModalClosedEvent += HandleModalClosed;
            return comp;

            void HandleModalOpened(IModal _, bool finished) {
                if (!finished) return;
                if (shadow != null) DynamicShadow.DespawnShadow(shadow);
                shadow = DynamicShadow.SpawnShadow(comp.ContentTransform, settings);
            }

            void HandleModalClosed(IModal _, bool finished) {
                if (finished || shadow == null) return;
                DynamicShadow.DespawnShadow(shadow);
                shadow = null;
            }
        }

        #endregion
    }
}