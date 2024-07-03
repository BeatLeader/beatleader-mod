using System;
using HMUI;
using UnityEngine;
using Screen = HMUI.Screen;

namespace BeatLeader.UI.Reactive.Components {
    internal static class ModalSystemHelper {
        public enum RelativePlacement {
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

        public static void OpenModalRelatively<T>(
            T modal,
            Transform screenChild,
            RectTransform anchor,
            RelativePlacement placement,
            Vector2? offset = null,
            bool animateBackground = false,
            bool interruptAll = false,
            DynamicShadowSettings? shadowSettings = null
        ) where T : IModal, IReactiveComponent {
            var screen = screenChild.GetComponentInParent<ViewController>();
            OpenModalRelatively(
                modal,
                screen,
                anchor,
                placement,
                offset,
                animateBackground,
                interruptAll,
                shadowSettings
            );
        }

        public static void OpenModalRelatively<T>(
            T modal,
            ViewController screen,
            RectTransform anchor,
            RelativePlacement placement,
            Vector2? offset = null,
            bool animateBackground = false,
            bool interruptAll = false,
            DynamicShadowSettings? shadowSettings = null
        ) where T : IModal, IReactiveComponent {
            offset ??= new(0f, 0.5f);
            CalculateRelativePlacement(
                screen.transform,
                anchor,
                placement,
                offset.Value,
                out var pos,
                out var pivot
            );
            var settings = new ModalSettings(pos, pivot, animateBackground, ShadowSettings: shadowSettings);
            ModalSystem.OpenModal(modal, screen, interruptAll, settings);
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
                RelativePlacement.Center => position + offset,
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
                RelativePlacement.Center => anchor.pivot,
                _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
            };
        }
    }
}