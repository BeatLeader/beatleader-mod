using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class Modal<T> : ModalComponentBase where T : IReactiveComponent, new() {
        #region UI Props

        public T Component { get; } = new();
        
        public Action<T>? OpenCallback { get; set; }
        public Action<T>? CloseCallback { get; set; }

        public bool ClickOffCloses { get; set; } = true;
        public DynamicShadowSettings? ShadowSettings { get; set; } = new();
        public Vector2 PositionOffset { get; set; } = new(0f, 2f);
        public ModalSystemHelper.RelativePlacement Placement { get; set; } = ModalSystemHelper.RelativePlacement.BottomCenter;

        public RectTransform AnchorTransform {
            get => _anchorTransform ?? throw new UninitializedComponentException("AnchorTransform was not specified");
            set => _anchorTransform = value;
        }

        private RectTransform? _anchorTransform;

        #endregion

        #region Modal

        public override bool OffClickCloses => ClickOffCloses;

        public void Open(Transform screenChild) {
            OpenCallback?.Invoke(Component);
            ModalSystemHelper.OpenModalRelatively(
                this,
                screenChild,
                AnchorTransform,
                Placement,
                PositionOffset,
                false,
                false,
                ShadowSettings
            );
        }

        protected override void OnClose() {
            CloseCallback?.Invoke(Component);
        }

        protected override GameObject Construct() {
            return Component.Use(null);
        }

        #endregion
    }
}