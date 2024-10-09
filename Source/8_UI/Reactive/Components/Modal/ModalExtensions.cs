using System;
using Reactive;
using Reactive.Components;
using UnityEngine;
using UnityExtensions = BeatLeader.Utils.UnityExtensions;

namespace BeatLeader.UI.Reactive.Components {
    internal static class ModalExtensions {
        #region WithAlphaOnModalOpen
        
        //TODO: rework and fix
        public static T WithAlphaOnModalOpen<T>(this T modal, Func<GameObject> objectFunc, float threshold = 0.2f) where T : IModal {
            CanvasGroup? group = null;
            modal.ModalOpenedEvent += HandleModalOpened;
            modal.ModalClosedEvent += HandleModalClosed;
            return modal;

            void HandleProgressChanged(IModal _, float progress) {
                if (group == null) return;
                group.alpha = Mathf.Lerp(1f, threshold, progress);
            }

            void HandleModalOpened(IModal _, bool finished) {
                if (finished) return;
                group ??= UnityExtensions.GetOrAddComponent<CanvasGroup>(objectFunc());
                group.ignoreParentGroups = true;
            }

            void HandleModalClosed(IModal _, bool finished) {
                if (!finished) return;
                group!.ignoreParentGroups = false;
            }
        }

        #endregion

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