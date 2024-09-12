using System;
using System.Collections.Generic;
using BeatLeader.Installers;
using BeatLeader.Utils;
using HMUI;
using Reactive;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRUIControls;

namespace BeatLeader.UI.Reactive.Components {
    internal class ModalSystem : ReactiveComponent {
        #region OpenModal

        public static void PresentModal<T>(
            T modal,
            Transform screenChild,
            bool animated = true,
            bool interruptAll = false
        ) where T : IModal, IReactiveComponent {
            var screen = screenChild.GetComponentInParent<ViewController>();
            PresentModal(modal, screen, animated, interruptAll);
        }

        public static void PresentModal<T>(
            T modal,
            ViewController screen,
            bool animated = true,
            bool interruptAll = false
        ) where T : IModal, IReactiveComponent {
            var modalSystem = BorrowOrInstantiateModalSystem(screen);
            PresentModalInternal(modalSystem, modal, animated, interruptAll);
        }

        private static void PresentModalInternal<T>(
            ModalSystem system,
            T modal,
            bool animated,
            bool interruptAll
        ) where T : IModal, IReactiveComponent {
            if (interruptAll) {
                InterruptAllEvent?.Invoke();
            }
            system.PresentModal(modal, animated);
        }

        #endregion

        #region ModalSystem Pool

        private static readonly ReactivePool<ViewController, ModalSystem> systemsPool = new() { DetachOnDespawn = false };

        private static ModalSystem BorrowOrInstantiateModalSystem(ViewController viewController) {
            var system = systemsPool.Get(viewController);
            system.Use(viewController.transform);
            return system;
        }

        private void ReleaseModalSystem() {
            systemsPool.Despawn(this);
        }

        #endregion

        #region Setup

        private static event Action? InterruptAllEvent;

        protected override void OnInitialize() {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            InterruptAllEvent += InterruptAll;
        }

        protected override void OnDestroy() {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            InterruptAllEvent -= InterruptAll;
            systemsPool.Despawn(this);
        }

        private void OnActiveSceneChanged(Scene from, Scene to) {
            InterruptAll();
        }

        private void InterruptAll() {
            if (!HasActiveModal) return;
            while (_modalStack.Count > 0) {
                CloseModal();
            }
        }

        #endregion

        #region Open & Close

        private IReactiveComponent? ReactiveActiveModal => _activeModal as IReactiveComponent;
        private bool HasActiveModal => _activeModal != null;

        private readonly Stack<IModal> _modalStack = new();
        private IModal? _activeModal;

        private void PresentModal<T>(T modal, bool animated) where T : IModal, IReactiveComponent {
            //showing modal system if needed
            if (HasActiveModal) {
                _activeModal!.Pause();
            } else {
                ShowModalSystem();
            }
            //adding modal to the stack
            _modalStack.Push(modal);
            _activeModal = modal;
            _activeModal.ModalClosedEvent += HandleModalClosed;
            //showing the modal
            ReactiveActiveModal!.Use(ContentTransform);
            ReactiveActiveModal.ContentTransform.SetAsLastSibling();
            _activeModal!.Open(!animated);
            RefreshBlocker();
        }

        private void CloseModal() {
            if (!HasActiveModal || _needToHideModalSystem) return;
            _activeModal!.Close(false);
        }

        private void CloseModalInternal() {
            if (!HasActiveModal) return;
            //removing current modal
            _modalStack.Pop();
            //setting new active modal
            if (_modalStack.Count > 0) {
                _activeModal!.ModalClosedEvent -= HandleModalClosed;
                _activeModal = _modalStack.Peek();
                _activeModal.Resume();
            } else {
                _needToHideModalSystem = true;
            }
            RefreshBlocker(0);
        }

        private void RefreshBlocker(int offset = -1) {
            _blockerGo.SetActive(HasActiveModal);
            if (!HasActiveModal) return;
            var modalIndex = ReactiveActiveModal!.ContentTransform.GetSiblingIndex();
            _blocker.SetSiblingIndex(modalIndex + offset);
        }

        private void HandleModalClosed(IModal modal, bool finished) {
            if (_needToHideModalSystem) {
                if (!finished) return;
                HideModalSystem();
            }
            if (_modalStack.Count == 0) return;
            var index = _modalStack.FindIndex(modal);
            if (index == -1) return;
            var count = index + 1;
            for (var i = 0; i < count; i++) {
                CloseModalInternal();
            }
        }

        #endregion

        #region ModalSystem Open & Close

        private bool _needToHideModalSystem;

        private void ShowModalSystem() {
            ContentTransform.WithRectExpand();
            Enabled = true;
        }

        private void HideModalSystem() {
            _needToHideModalSystem = false;
            _activeModal!.ModalClosedEvent -= HandleModalClosed;
            _activeModal = null;
            ReleaseModalSystem();
        }

        #endregion

        #region Construct

        private UnityEngine.UI.Button _blockerButton = null!;
        private GameObject _blockerGo = null!;
        private RectTransform _blocker = null!;

        protected override void Construct(RectTransform rectTransform) {
            var go = rectTransform.gameObject;
            //canvas
            var canvas = go.AddComponent<Canvas>();
            canvas.additionalShaderChannels |=
                AdditionalCanvasShaderChannels.Tangent |
                AdditionalCanvasShaderChannels.TexCoord2;
            go.AddComponent<CanvasGroup>();
            //blocker
            _blockerGo = new GameObject("Blocker");
            _blockerButton = _blockerGo.AddComponent<UnityEngine.UI.Button>();
            _blocker = _blockerGo.AddComponent<RectTransform>();
            _blockerGo.AddComponent<Touchable>();
            _blocker.SetParent(rectTransform, false);
            _blockerButton.onClick.AddListener(HandleBlockerClicked);
            _blocker.WithRectExpand();
            //raycaster
            var raycaster = go.AddComponent<VRGraphicRaycaster>();
            OnMenuInstaller.Container.Inject(raycaster);
            go.AddComponent<GraphicRaycaster>();
        }

        private void HandleBlockerClicked() {
            CloseModal();
        }

        #endregion
    }
}