using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class KeyboardModal<T, TInput> : SharedModal<AnimatedModalWrapper<T>>, IKeyboardController<IInputFieldController>
        where T : IKeyboardController<IInputFieldController>, IReactiveComponent, new()
        where TInput : IInputFieldController, IReactiveComponent {

        #region Keyboard Component

        public event Action? KeyboardClosedEvent;

        private TInput? _inputField;

        void IKeyboardController<IInputFieldController>.Setup(IInputFieldController? input) {
            _inputField = input == null ? default : (TInput)input;
        }

        void IKeyboardController<IInputFieldController>.Refresh() {
            Keyboard.Refresh();
        }

        void IKeyboardController<IInputFieldController>.SetActive(bool active) {
            if (active) {
                this.Present(_inputField!.ContentTransform);
            } else {
                BorrowedModal.Close(false);
            }
        }

        #endregion

        #region UI Props

        public RelativePlacement Placement { get; set; } = RelativePlacement.BottomCenter;
        public Vector2? Offset { get; set; }
       
        #endregion

        #region Modal
        
        private T Keyboard => BorrowedModal.Component;

        public override void Close(bool immediate) {
            if (_inputField is { CanProceed: false }) return;
            base.Close(immediate);
        }

        protected override void OnCloseInternal(bool finished) {
            if (!finished) return;
            Keyboard.SetActive(false);
        }

        protected override void OnOpenInternal(bool finished) {
            if (finished) return;
            Keyboard.SetActive(true);
            Keyboard.Setup(_inputField);
            BorrowedModal.WithAnchorImmediate(_inputField!.ContentTransform, Placement, Offset);
        }

        protected override void OnSpawn() {
            Keyboard.KeyboardClosedEvent += HandleKeyboardClosed;
        }

        protected override void OnDespawn() {
            Keyboard.KeyboardClosedEvent -= HandleKeyboardClosed;
        }

        #endregion

        #region Callbacks

        private void HandleKeyboardClosed() {
            BorrowedModal.Close(false);
        }

        #endregion
    }
}