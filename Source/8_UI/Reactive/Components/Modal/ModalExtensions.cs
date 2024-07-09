using System;
using BeatLeader.Utils;
using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal enum RelativePlacement {
        LeftTop,
        LeftCenter,
        LeftBottom,
        TopRight,
        TopCenter,
        TopLeft,
        RightTop,
        RightCenter,
        RightBottom,
        BottomRight,
        BottomCenter,
        BottomLeft,
        Center
    }

    internal static class ModalExtensions {
        #region Events

        public static T WithCloseListener<T>(this T comp, Action callback) where T : IModal {
            comp.ModalClosedEvent += (_, _) => callback();
            return comp;
        }

        public static T WithCloseListener<T>(this T comp, Action<IModal, bool> callback) where T : IModal {
            comp.ModalClosedEvent += callback;
            return comp;
        }

        public static T WithOpenListener<T>(this T comp, Action callback) where T : IModal {
            comp.ModalOpenedEvent += (_, _) => callback();
            return comp;
        }

        public static T WithOpenListener<T>(this T comp, Action<IModal, bool> callback) where T : IModal {
            comp.ModalOpenedEvent += callback;
            return comp;
        }

        #endregion

        #region ModalWrapper

        public static AnimatedModalWrapper<T> WithComponent<T>(this AnimatedModalWrapper<T> comp, Action<T> callback) where T : IReactiveComponent, new() {
            callback(comp.Component);
            return comp;
        }

        #endregion

        #region Present

        public static void Present<T>(this T comp, ViewController viewController, bool animated = true) where T : IModal, IReactiveComponent {
            ModalSystem.PresentModal(comp, viewController, animated);
        }

        public static void Present<T>(this T comp, Transform screenChild, bool animated = true) where T : IModal, IReactiveComponent {
            ModalSystem.PresentModal(comp, screenChild, animated);
        }

        #endregion

        #region WithModal

        public static T WithModal<T, TModal>(this T comp, TModal modal, bool animated = true)
            where T : IClickableComponent, IReactiveComponent
            where TModal : IModal, IReactiveComponent {
            comp.ClickEvent += () => modal.Present(comp.ContentTransform, animated);
            return comp;
        }

        #endregion

        #region WithAlphaOnModalOpen

        public static T WithAlphaOnModalOpen<T>(this T comp, IModal modal, float threshold = 0.2f) where T : IReactiveComponent {
            WithAlphaOnModalOpen(modal, () => comp.Content, threshold);
            return comp;
        }
        
        public static T WithAlphaOnModalOpen<T>(this T modal, Func<GameObject> objectFunc, float threshold = 0.2f) where T : IModal {
            CanvasGroup? group = null;
            modal.ModalOpenedEvent += HandleModalOpened;
            modal.ModalClosedEvent += HandleModalClosed;
            modal.OpenProgressChangedEvent += HandleProgressChanged;
            return modal;

            void HandleProgressChanged(IModal _, float progress) {
                if (group == null) return;
                group.alpha = Mathf.Lerp(1f, threshold, progress);
            }

            void HandleModalOpened(IModal _, bool finished) {
                if (finished) return;
                group ??= objectFunc().GetOrAddComponent<CanvasGroup>();
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

        #region WithAnchor

        public static T WithAnchor<T>(
            this T comp,
            IReactiveComponent anchor,
            RelativePlacement placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                () => anchor,
                () => placement,
                offset,
                unbindOnceOpened
            );
        }

        public static T WithAnchor<T>(
            this T comp,
            IReactiveComponent anchor,
            Func<RelativePlacement> placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                () => anchor,
                placement,
                offset,
                unbindOnceOpened
            );
        }

        public static T WithAnchor<T>(
            this T comp,
            Func<IReactiveComponent> anchor,
            RelativePlacement placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                () => anchor().ContentTransform,
                () => placement,
                offset,
                unbindOnceOpened
            );
        }

        public static T WithAnchor<T>(
            this T comp,
            Func<IReactiveComponent> anchor,
            Func<RelativePlacement> placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                () => anchor().ContentTransform,
                placement,
                offset,
                unbindOnceOpened
            );
        }

        public static T WithAnchor<T>(
            this T comp,
            RectTransform anchor,
            RelativePlacement placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                () => anchor,
                () => placement,
                offset,
                unbindOnceOpened
            );
        }

        public static T WithAnchor<T>(
            this T comp,
            Func<RectTransform> anchor,
            RelativePlacement placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                anchor,
                () => placement,
                offset,
                unbindOnceOpened
            );
        }

        public static T WithAnchorImmediate<T>(
            this T comp,
            RectTransform anchor,
            RelativePlacement placement,
            Vector2? offset = null
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                () => anchor,
                () => placement,
                offset,
                false,
                true
            );
        }

        public static T WithAnchor<T>(
            this T comp,
            Func<RectTransform> anchor,
            Func<RelativePlacement> placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true,
            bool immediate = false
        ) where T : IModal, IReactiveComponent {
            if (immediate) {
                HandleModalOpened(comp, false);
            } else {
                comp.ModalOpenedEvent += HandleModalOpened;
            }
            return comp;

            void HandleModalOpened(IModal modal, bool opened) {
                var root = comp.ContentTransform.parent;
                if (root == null) return;
                CalculateRelativePlacement(
                    root,
                    anchor(),
                    placement(),
                    offset.GetValueOrDefault(new(0f, 0.5f)),
                    out var position,
                    out var pivot
                );
                comp.ContentTransform.localPosition = position;
                comp.ContentTransform.pivot = pivot;
                if (!immediate && unbindOnceOpened && comp is not ISharedModal) {
                    modal.ModalOpenedEvent -= HandleModalOpened;
                }
            }
        }

        private static void CalculateRelativePlacement(
            Transform root,
            RectTransform anchor,
            RelativePlacement placement,
            Vector2 offset,
            out Vector2 position,
            out Vector2 pivot
        ) {
            position = root.InverseTransformPoint(anchor.position);
            var rect = anchor.rect;
            var anchorHeightDiv = new Vector2(0f, rect.height / 2);
            var anchorWidthDiv = new Vector2(rect.width / 2, 0f);
            position = placement switch {
                RelativePlacement.LeftTop => position - anchorWidthDiv + anchorHeightDiv - offset,
                RelativePlacement.LeftCenter => position - anchorWidthDiv + new Vector2(-offset.x, offset.y),
                RelativePlacement.LeftBottom => position - anchorWidthDiv - anchorHeightDiv + new Vector2(-offset.x, offset.y),
                RelativePlacement.TopLeft => position + anchorHeightDiv - anchorWidthDiv + offset,
                RelativePlacement.TopCenter => position + anchorHeightDiv + offset,
                RelativePlacement.TopRight => position + anchorHeightDiv + anchorWidthDiv + new Vector2(-offset.x, offset.y),
                RelativePlacement.RightTop => position + anchorWidthDiv + anchorHeightDiv + new Vector2(offset.x, -offset.y),
                RelativePlacement.RightCenter => position + anchorWidthDiv + offset,
                RelativePlacement.RightBottom => position + anchorWidthDiv - anchorHeightDiv + offset,
                RelativePlacement.BottomLeft => position - anchorHeightDiv - anchorWidthDiv + new Vector2(offset.x, -offset.y),
                RelativePlacement.BottomCenter => position - anchorHeightDiv + new Vector2(offset.x, -offset.y),
                RelativePlacement.BottomRight => position - anchorHeightDiv + anchorWidthDiv - offset,
                RelativePlacement.Center => position + offset + rect.size * Vector2.one * 0.5f - rect.size * anchor.pivot,
                _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
            };
            pivot = placement switch {
                RelativePlacement.LeftTop => new(1f, 1f),
                RelativePlacement.LeftCenter => new(1f, 0.5f),
                RelativePlacement.LeftBottom => new(1f, 0f),
                RelativePlacement.TopLeft => new(0f, 0f),
                RelativePlacement.TopCenter => new(0.5f, 0f),
                RelativePlacement.TopRight => new(1f, 0f),
                RelativePlacement.RightTop => new(0f, 1f),
                RelativePlacement.RightCenter => new(0f, 0.5f),
                RelativePlacement.RightBottom => new(0f, 0f),
                RelativePlacement.BottomLeft => new(0f, 1f),
                RelativePlacement.BottomCenter => new(0.5f, 1f),
                RelativePlacement.BottomRight => new(1f, 1f),
                RelativePlacement.Center => Vector2.one * 0.5f,
                _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
            };
        }

        #endregion
    }
}