using System;
using System.Collections;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Screen = HMUI.Screen;

namespace BeatLeader {
    [Obsolete("Use ModalSystem instead")]
    internal class ReeModalSystem : ReeUIComponentV2 {
        #region ActiveModals

        private static readonly Dictionary<int, ReeModalSystem> ActiveModals = new Dictionary<int, ReeModalSystem>();

        private static event Action InterruptAllEvent;

        [PublicAPI]
        public static void OpenModal<T>(Transform screenChild, object state) where T : IReeModal {
            var screen = screenChild.GetComponentInParent<Screen>();
            OpenModal<T>(screen, state);
        }

        public static void OpenModal<T>(Screen screen, object state, bool interruptActiveModals = true) where T : IReeModal {
            ReeModalSystem controller;
            var key = screen.GetHashCode();

            if (ActiveModals.ContainsKey(key)) {
                controller = ActiveModals[key];
            } else {
                controller = Instantiate<ReeModalSystem>(screen.transform);
                controller.Construct(screen);
                controller.ManualInit(screen.transform);
                ActiveModals[key] = controller;
            }

            if (interruptActiveModals) {
                InterruptAllEvent?.Invoke();
            }

            controller.OpenModal<T>(state);
        }

        public static void ForceUpdateAll() {
            foreach (var activeModal in ActiveModals.Values) {
                activeModal.ForceUpdate();
            }
        }

        #endregion

        #region Construct / Initialize / Dispose

        private Screen _screen;

        private void Construct(Screen screen) {
            _screen = screen;
        }

        protected override void OnInitialize() {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            InterruptAllEvent += InterruptAll;
            InitializeModal();
        }

        protected override void OnDispose() {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            InterruptAllEvent -= InterruptAll;
            ActiveModals.Remove(_screen.GetHashCode());
        }

        private void OnActiveSceneChanged(Scene from, Scene to) {
            InterruptAll();
        }

        #endregion

        #region Pool

        [UIComponent("container"), UsedImplicitly]
        private Transform _container;

        private readonly Dictionary<Type, IReeModal> _pool = new Dictionary<Type, IReeModal>();

        private IReeModal GetOrInstantiateModal<T>() where T : IReeModal {
            var key = typeof(T);
            if (_pool.ContainsKey(key)) return _pool[key];

            var component = Instantiate(typeof(T), _container);
            component.ManualInit(_container);

            var modal = (IReeModal)component;
            _pool[key] = modal;
            return modal;
        }

        #endregion

        #region Stack

        private readonly Stack<(IReeModal, object)> _stack = new Stack<(IReeModal, object)>();
        private IReeModal _activeModal;
        private object _activeModalState;
        private bool _hasActiveModal;

        private void OpenModal<T>(object context) where T : IReeModal {
            var editor = GetOrInstantiateModal<T>();
            PopOpen(editor, context);
        }

        private void PopOpen(IReeModal modal, object state) {
            if (!_hasActiveModal) {
                _stack.Push((modal, state));
                OpenImmediately();
            } else {
                _stack.Push((_activeModal, _activeModalState));
                _stack.Push((modal, state));
                CloseOrPop();
            }
        }

        private void OpenImmediately() {
            if (_stack.Count == 0) return;
            var (modal, state) = _stack.Pop();
            _activeModal = modal;
            _activeModalState = state;
            _hasActiveModal = true;
            _activeModal.Resume(state, CloseOrPop);
            ShowModal();
        }

        private void CloseOrPop() {
            _activeModal.Pause();

            if (_stack.Count != 0) {
                OpenImmediately();
                return;
            }

            _activeModal = null;
            _activeModalState = null;
            _hasActiveModal = false;
            HideModal();
        }

        #endregion

        #region ForceUpdate / InterruptAll

        private void InterruptAll() {
            if (!_hasActiveModal) return;

            _activeModal.Interrupt();

            while (_stack.Count > 0) {
                var (modal, state) = _stack.Pop();
                modal.Interrupt();
            }

            _activeModal = null;
            _activeModalState = null;
            _hasActiveModal = false;
            HideModal(false);
        }

        private void ForceUpdate() {
            if (!_hasActiveModal) return;
            StartCoroutine(ForceUpdateCoroutine());
        }

        private IEnumerator ForceUpdateCoroutine() {
            yield return new WaitForEndOfFrame();
            HideModal(false);
            ShowModal(false);
        }

        #endregion

        #region ModalView

        [UIComponent("modal-view"), UsedImplicitly]
        private ModalView _modalView;

        private void InitializeModal() {
            var background = _modalView.GetComponentInChildren<ImageView>();
            if (background != null) background.enabled = false;
            var touchable = _modalView.GetComponentInChildren<Touchable>();
            if (touchable != null) touchable.enabled = false;
            var area = _container.GetComponent<ImageView>();
            if (area != null) area.raycastTarget = true;
            _modalView.blockerClickedEvent += OnBlockerClicked;
        }

        private void OnBlockerClicked() {
            if (!_hasActiveModal) return;
            _activeModal.HandleOffClick();
        }

        private void ShowModal(bool animated = true) {
            if (_modalView == null) return;
            _modalView.Show(animated, true);
        }

        private void HideModal(bool animated = true) {
            if (_modalView == null) return;
            _modalView.Hide(animated);
        }

        #endregion
    }
}