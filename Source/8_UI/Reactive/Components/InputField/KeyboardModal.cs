using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class KeyboardModal<T, TInput> : IKeyboardController<IInputFieldController>
        where T : IKeyboardController<IInputFieldController>, IReactiveComponent, new()
        where TInput : IInputFieldController, IReactiveComponent {
        #region Constructor & Destructor

        public KeyboardModal() {
            KeyboardClosedEventInternal += HandleKeyboardClosedInternal;
        }

        ~KeyboardModal() {
            KeyboardClosedEventInternal -= HandleKeyboardClosedInternal;
        }

        #endregion

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
                Keyboard.SetActive(true);
                ActivateModal();
            } else {
                Modal.Close();
            }
        }

        #endregion

        #region UI Props

        public Action<T>? OpenCallback { get; set; }
        public Action<T>? CloseCallback { get; set; }

        public bool ClickOffCloses { get; set; } = true;
        public DynamicShadowSettings? ShadowSettings { get; set; } = new();
        public Vector2 PositionOffset { get; set; } = new(0f, 10f);
        public ModalSystemHelper.RelativePlacement Placement { get; set; } = ModalSystemHelper.RelativePlacement.BottomCenter;

        #endregion

        #region Modal Management

        private static Modal<T> Modal {
            get {
                if (_modal?.IsDestroyed ?? true) {
                    _modal = new();
                    _modal.ModalAskedToBeClosedEvent += HandleModalClosed;
                    _modal.Component.KeyboardClosedEvent += HandleKeyboardClosed;
                }
                return _modal;
            }
        }

        private static T Keyboard => Modal.Component;
        
        private static event Action? KeyboardClosedEventInternal;

        private static Modal<T>? _modal;

        private void ActivateModal() {
            if (_inputField == null) throw new UninitializedComponentException();
            Keyboard.Setup(_inputField);
            OpenCallback?.Invoke(Keyboard);
            RefreshModal();
            Modal.Open(_inputField!.ContentTransform);
        }

        private void RefreshModal() {
            Modal.ClickOffCloses = ClickOffCloses;
            Modal.ShadowSettings = ShadowSettings;
            Modal.PositionOffset = PositionOffset;
            Modal.Placement = Placement;
            Modal.AnchorTransform = _inputField!.ContentTransform;
        }

        #endregion

        #region Callbacks

        private void HandleKeyboardClosedInternal() {
            KeyboardClosedEvent?.Invoke();
            CloseCallback?.Invoke(Keyboard);
        }

        private static void HandleModalClosed() {
            Keyboard.SetActive(false);
            KeyboardClosedEventInternal?.Invoke();
        }

        private static void HandleKeyboardClosed() {
            Modal.Close();
            KeyboardClosedEventInternal?.Invoke();
        }

        #endregion
    }
}