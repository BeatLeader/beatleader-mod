using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using HMUI;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRUIControls;

namespace BeatLeader.UI.Reactive.Components {
    internal record ModalSettings(
        Vector2 Position,
        Vector2 Pivot,
        bool AnimateParentCanvas = false,
        bool AnimateModal = true,
        DynamicShadowSettings? ShadowSettings = null
    );

    internal class ModalSystem : ReactiveComponent {
        #region OpenModal

        public static void OpenPersistentModal<T, TContext>(
            TContext context,
            Transform screenChild,
            bool interruptAll = false,
            ModalSettings? settings = null
        ) where T : IPersistentModal<TContext>, IReactiveComponent, new() {
            var screen = screenChild.GetComponentInParent<ViewController>();
            OpenPersistentModal<T, TContext>(context, screen, interruptAll, settings);
        }

        public static void OpenPersistentModal<T, TContext>(
            TContext context,
            ViewController screen,
            bool interruptAll = false,
            ModalSettings? settings = null
        ) where T : IPersistentModal<TContext>, IReactiveComponent, new() {
            var modalSystem = BorrowOrInstantiateModalSystem(screen);
            var modal = BorrowOrInstantiatePersistentModal<T, TContext>(context);
            OpenModalInternal(modalSystem, modal, interruptAll, settings);
        }

        public static void OpenModal<T>(
            T modal,
            Transform screenChild,
            bool interruptAll = false,
            ModalSettings? settings = null
        ) where T : IModal, IReactiveComponent {
            var screen = screenChild.GetComponentInParent<ViewController>();
            OpenModal(modal, screen, interruptAll, settings);
        }

        public static void OpenModal<T>(
            T modal,
            ViewController screen,
            bool interruptAll = false,
            ModalSettings? settings = null
        ) where T : IModal, IReactiveComponent {
            var modalSystem = BorrowOrInstantiateModalSystem(screen);
            OpenModalInternal(modalSystem, modal, interruptAll, settings);
        }

        private static void OpenModalInternal<T>(
            ModalSystem system,
            T modal,
            bool interruptAll,
            ModalSettings? settings
        ) where T : IModal, IReactiveComponent {
            if (interruptAll) {
                InterruptAllEvent?.Invoke();
            }
            system.AppendOrOpenModal(modal, settings);
        }

        #endregion

        #region ModalSystem Pool

        private static readonly ReactivePool<ViewController, ModalSystem> systemsPool = new();

        private static ModalSystem BorrowOrInstantiateModalSystem(ViewController viewController) {
            var system = systemsPool.Get(viewController);
            system._parentCanvasGroup = viewController.gameObject.GetOrAddComponent<CanvasGroup>();
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

        #endregion

        #region PersistentModal Pool

        private static readonly Dictionary<Type, Stack<IModal>> persistentModalsPool = new();

        private static T BorrowOrInstantiatePersistentModal<T, TContext>(TContext context) where T : IPersistentModal<TContext>, IReactiveComponent, new() {
            var key = typeof(T);
            persistentModalsPool.TryGetValue(key, out var modalStack);

            var modal = modalStack?.Count switch {
                > 0 => modalStack.Pop(),
                _ => new T()
            } as IPersistentModal<TContext>;

            if (modal is IReactiveComponent { IsDestroyed: true }) {
                modal = BorrowOrInstantiatePersistentModal<T, TContext>(context);
            } else {
                modal!.Context = context;
            }

            return (T)modal;
        }

        private static void ReleasePersistentModal(IModal modal) {
            var key = modal.GetType();
            if (!persistentModalsPool.TryGetValue(key, out var modalsStack)) {
                modalsStack = new();
                persistentModalsPool[key] = modalsStack;
            }
            modalsStack.Push(modal);
        }

        private static void TryReleasePersistentModal(IModal modal) {
            if (modal is not IPersistentModal) return;
            ReleasePersistentModal(modal);
        }

        #endregion

        #region Open & Close

        private ILayoutItem? LayoutActiveModal => _activeModal as ILayoutItem;
        private IReactiveComponent? ReactiveActiveModal => _activeModal as IReactiveComponent;
        private bool HasActiveModal => _activeModal != null;

        private readonly Stack<IModal> _modalStack = new();
        private IModal? _activeModal;

        private void AppendOrOpenModal<T>(T modal, ModalSettings? settings = null) where T : IModal, IReactiveComponent {
            //appending a modal
            var firstModal = !HasActiveModal;
            if (HasActiveModal) {
                _activeModal!.Pause();
                _modalStack.Push(_activeModal);
            } else {
                ShowModalSystem(settings);
            }
            //replacing events
            if (_activeModal != null) {
                _activeModal.ModalAskedToBeClosedEvent -= HandleModalWantsToBeClosed;
            }
            _activeModal = modal;
            if (_activeModal != null) {
                _activeModal.ModalAskedToBeClosedEvent += HandleModalWantsToBeClosed;
            }
            //showing the modal
            PresentActiveModal(
                firstModal,
                settings?.AnimateModal ?? true,
                settings?.AnimateParentCanvas ?? false
            );
            if (settings != null) {
                ApplyModalSettings(modal, settings);
            }
            //placing blocker under the modal
            RefreshBlocker();
        }

        private void PopOrCloseModal(bool closeAnimated = true) {
            if (!HasActiveModal) return;
            //truncating from the stack
            var goingToClose = _modalStack.Count is 0;
            if (_modalStack.TryPop(out var modal)) {
                modal!.Resume();
            }
            //unsubscribing events
            if (_activeModal != null) {
                _activeModal.ModalAskedToBeClosedEvent -= HandleModalWantsToBeClosed;
            }
            DismissActiveModal(goingToClose, closeAnimated);
            _activeModal = modal;
            //subscribing the previous modal back
            if (_activeModal != null) {
                _activeModal.ModalAskedToBeClosedEvent += HandleModalWantsToBeClosed;
            }
            //placing blocker under the previous modal
            RefreshBlocker(0);
        }

        private void InterruptAll() {
            if (!HasActiveModal) return;
            while (_modalStack.Count > 0) {
                PopOrCloseModal(false);
            }
        }

        private void RefreshBlocker(int offset = -1) {
            _blockerGo.SetActive(HasActiveModal);
            if (!HasActiveModal) return;
            var modalIndex = ReactiveActiveModal!.ContentTransform.GetSiblingIndex();
            _blocker.SetSiblingIndex(modalIndex + offset);
        }

        private void HandleModalWantsToBeClosed() {
            PopOrCloseModal();
        }

        #endregion

        #region Present & Dismiss Modal

        private static PanelAnimationSO PresentAnimation {
            get {
                _presentAnimation ??= Resources
                    .FindObjectsOfTypeAll<PanelAnimationSO>()
                    .First(x => x.name == "HMUI.DefaultPresentPanelAnimationsSO");
                return _presentAnimation;
            }
        }

        private static PanelAnimationSO DismissAnimation {
            get {
                _dismissAnimation ??= Resources
                    .FindObjectsOfTypeAll<PanelAnimationSO>()
                    .First(x => x.name == "HMUI.DefaultDismissPanelAnimationsSO");
                return _dismissAnimation;
            }
        }

        private static PanelAnimationSO? _presentAnimation;
        private static PanelAnimationSO? _dismissAnimation;

        private void DismissActiveModal(bool lastModal, bool animated, Action? callback = null) {
            var cachedActiveModal = ReactiveActiveModal!;
            DismissAnimation.ExecuteAnimation(
                ReactiveActiveModal!.Content,
                _systemOpenedWithAnimation ? _parentCanvasGroup : null,
                !animated,
                () => {
                    if (lastModal) HideModalSystem();
                    cachedActiveModal.Use(null);
                    cachedActiveModal.Enabled = false;
                    callback?.Invoke();
                }
            );
            TryUnbindShadow(cachedActiveModal);
            TryReleasePersistentModal(_activeModal!);
            _activeModal = null;
        }

        private void PresentActiveModal(bool firstModal, bool animated, bool canvasAnimated) {
            if (LayoutActiveModal != null) {
                LayoutActiveModal.LayoutDriver = null;
            }
            ReactiveActiveModal!.Use(ContentTransform);
            ReactiveActiveModal.Enabled = true;
            PresentAnimation.ExecuteAnimation(
                ReactiveActiveModal!.Content,
                canvasAnimated && firstModal ? _parentCanvasGroup : null,
                !animated,
                null
            );
            _activeModal!.OnOpen();
        }

        private static void ApplyModalSettings<T>(T modal, ModalSettings settings) where T : IModal, IReactiveComponent {
            var trans = modal.ContentTransform;
            trans.pivot = settings.Pivot;
            trans.localPosition = settings.Position;
            if (settings.ShadowSettings != null) {
                BindShadow(modal, settings.ShadowSettings);
            }
        }

        #endregion

        #region Shadow

        private static readonly ReactivePool<IReactiveComponent, DynamicShadow> shadowsPool = new();

        private static void BindShadow(IReactiveComponent modal, DynamicShadowSettings shadowSettings) {
            var shadow = shadowsPool.Spawn(modal);
            shadow.Setup(modal.ContentTransform, shadowSettings);
        }

        private static void TryUnbindShadow(IReactiveComponent modal) {
            shadowsPool.TryDespawn(modal);
        }

        #endregion

        #region Screen

        private CanvasGroup? _parentCanvasGroup;
        private bool _systemOpenedWithAnimation;

        private void ShowModalSystem(ModalSettings? settings) {
            ContentTransform.WithRectExpand();
            _systemOpenedWithAnimation = settings?.AnimateParentCanvas ?? false;
        }

        private void HideModalSystem() {
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
            BeatSaberUI.DiContainer.Inject(raycaster);
        }

        private void HandleBlockerClicked() {
            if (_activeModal is null || !_activeModal.OffClickCloses) return;
            _activeModal.Close();
        }

        #endregion
    }
}