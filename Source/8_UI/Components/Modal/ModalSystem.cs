using System;
using System.Collections.Generic;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Screen = HMUI.Screen;

namespace BeatLeader.Components {
    internal static class ModalSystemHelper {
        public enum RelativePlacement {
            LeftTop,
            LeftBottom,
            TopRight,
            TopLeft,
            RightTop,
            RightBottom,
            BottomRight,
            BottomLeft
        }

        public static void OpenPersistentModalRelatively<T, TContext>(
            TContext context,
            Transform screenChild,
            RectTransform anchor,
            RelativePlacement placement,
            bool animateBackground = false,
            bool interruptAll = false
        ) where T : ReeUIComponentV3<T>, IPersistentModal<T, TContext> {
            var screen = screenChild.GetComponentInParent<Screen>();
            OpenPersistentModalRelatively<T, TContext>(context, screen, anchor, placement, animateBackground, interruptAll);
        }
        
        public static void OpenPersistentModalRelatively<T, TContext>(
            TContext context,
            Screen screen,
            RectTransform anchor,
            RelativePlacement placement,
            bool animateBackground = false,
            bool interruptAll = false
        ) where T : ReeUIComponentV3<T>, IPersistentModal<T, TContext> {
            CalculateRelativePlacement(screen.transform, anchor, placement, out var pos, out var pivot);
            var settings = new ModalSystem.ModalScreenSettings(pos, pivot, animateBackground);
            ModalSystem.OpenPersistentModal<T, TContext>(context, screen, interruptAll, settings);
        }

        private static void CalculateRelativePlacement(
            Transform root,
            RectTransform anchor,
            RelativePlacement placement,
            out Vector2 position,
            out Vector2 pivot
        ) {
            position = root.InverseTransformPoint(anchor.position);
            var rect = anchor.rect;
            var anchorHeightDiv = new Vector2(0f, rect.height / 2);
            var anchorWidthDiv = new Vector2(rect.width / 2, 0f);
            position = placement switch {
                RelativePlacement.LeftTop => position - anchorWidthDiv + anchorHeightDiv,
                RelativePlacement.LeftBottom => position - anchorWidthDiv - anchorHeightDiv,
                RelativePlacement.TopLeft => position + anchorHeightDiv - anchorWidthDiv,
                RelativePlacement.TopRight => position + anchorHeightDiv + anchorWidthDiv,
                RelativePlacement.RightTop => position + anchorWidthDiv + anchorHeightDiv,
                RelativePlacement.RightBottom => position + anchorWidthDiv - anchorHeightDiv,
                RelativePlacement.BottomLeft => position - anchorHeightDiv - anchorWidthDiv,
                RelativePlacement.BottomRight => position - anchorHeightDiv + anchorWidthDiv,
                _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
            };
            pivot = placement switch {
                RelativePlacement.LeftTop => new(1f, 1f),
                RelativePlacement.LeftBottom => new(1f, 0f),
                RelativePlacement.TopLeft => new(0f, 0f),
                RelativePlacement.TopRight => new(1f, 0f),
                RelativePlacement.RightTop => new(0f, 1f),
                RelativePlacement.RightBottom => new(0f, 0f),
                RelativePlacement.BottomLeft => new(0f, 1f),
                RelativePlacement.BottomRight => new(1f, 1f),
                _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
            };
        }
    }

    internal class ModalSystem : ReeUIComponentV3<ModalSystem> {
        public record ModalScreenSettings(
            Vector2 Position,
            Vector2 Pivot,
            bool AnimateBackground
        );

        #region OpenModal

        public static void OpenPersistentModal<T, TContext>(
            TContext context,
            Transform screenChild,
            bool interruptAll = false,
            ModalScreenSettings? settings = null
        ) where T : ReeUIComponentV3<T>, IPersistentModal<T, TContext> {
            var screen = screenChild.GetComponentInParent<Screen>();
            OpenPersistentModal<T, TContext>(context, screen, interruptAll, settings);
        }

        public static void OpenPersistentModal<T, TContext>(
            TContext context,
            Screen screen,
            bool interruptAll = false,
            ModalScreenSettings? settings = null
        ) where T : ReeUIComponentV3<T>, IPersistentModal<T, TContext> {
            var modalSystem = GetOrInstantiateModalSystem(screen);
            var modal = modalSystem.BorrowOrInstantiatePersistentModal<T, TContext>(context);
            OpenModalInternal(modalSystem, modal, interruptAll, settings);
        }

        public static void OpenModal(IModal modal, Transform screenChild, bool interruptAll = false, ModalScreenSettings? settings = null) {
            var screen = screenChild.GetComponentInParent<Screen>();
            OpenModal(modal, screen, interruptAll, settings);
        }

        public static void OpenModal(IModal modal, Screen screen, bool interruptAll = false, ModalScreenSettings? settings = null) {
            var modalSystem = GetOrInstantiateModalSystem(screen);
            OpenModalInternal(modalSystem, modal, interruptAll, settings);
        }

        private static void OpenModalInternal(ModalSystem system, IModal modal, bool interruptAll, ModalScreenSettings? settings) {
            if (interruptAll) {
                InterruptAllEvent?.Invoke();
            }
            system.AppendOrOpenModal(modal, settings);
        }

        #endregion

        #region ModalSystem

        private static readonly Dictionary<int, ModalSystem> cachedModalSystems = new();

        private static ModalSystem GetOrInstantiateModalSystem(Screen screen) {
            var key = screen.GetHashCode();
            if (!cachedModalSystems.TryGetValue(key, out var system)) {
                system = Instantiate(screen.transform);
                system._screen = screen;
                cachedModalSystems[key] = system;
            }
            return system;
        }

        #endregion

        #region Setup

        private static event Action? InterruptAllEvent;

        private Screen _screen = null!;

        protected override void OnInitialize() {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            InterruptAllEvent += InterruptAll;
            InitializeModal();
        }

        protected override void OnDispose() {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            InterruptAllEvent -= InterruptAll;
            cachedModalSystems.Remove(_screen.GetHashCode());
        }

        private void OnActiveSceneChanged(Scene from, Scene to) {
            InterruptAll();
        }

        #endregion

        #region PersistentModal Pool

        private readonly Dictionary<Type, Stack<IModal>> _persistentModalsPool = new();

        private IPersistentModal<T, TContext> BorrowOrInstantiatePersistentModal<T, TContext>(TContext context) where T : ReeUIComponentV3<T>, IPersistentModal<T, TContext> {
            var key = typeof(T);
            _persistentModalsPool.TryGetValue(key, out var modalStack);

            var modal = modalStack?.Count switch {
                > 0 => modalStack.Pop(),
                _ => ReeUIComponentV3<T>.Instantiate(transform)
            } as IPersistentModal<T, TContext>;

            modal!.Context = context;
            return modal;
        }

        private void ReleasePersistentModal(IModal modal) {
            var key = modal.GetType();
            if (!_persistentModalsPool.TryGetValue(key, out var modalsStack)) {
                modalsStack = new();
                _persistentModalsPool[key] = modalsStack;
            }
            modalsStack.Push(modal);
        }

        private void TryReleasePersistentModal(IModal modal) {
            if (modal is not IPersistentModal) return;
            ReleasePersistentModal(modal);
        }

        #endregion

        #region Modal Stack

        private bool HasActiveModal => _activeModal is not null;

        private readonly Stack<IModal> _modalStack = new();
        private IModal? _activeModal;

        private void AppendOrOpenModal(IModal modal, ModalScreenSettings? settings = null) {
            if (HasActiveModal) {
                _activeModal!.Pause();
                _modalStack.Push(_activeModal);
            } else {
                ShowModalScreen(settings);
            }
            _activeModal = modal;
            _activeModal.Setup(_container);
        }

        private void PopOrCloseModal(bool closeAnimated = true) {
            if (!HasActiveModal) return;
            var goingToClose = _modalStack.Count is 0;
            if (_modalStack.TryPop(out var modal)) {
                modal!.Resume();
            }
            if (!goingToClose) {
                DismissActiveModal();
                _activeModal = modal;
            } else {
                HideModalScreen(closeAnimated, DismissActiveModal);
            }
        }

        private void DismissActiveModal() {
            _activeModal!.Setup(null);
            TryReleasePersistentModal(_activeModal);
            _activeModal = null;
        }

        private void InterruptAll() {
            if (!HasActiveModal) return;
            while (_modalStack.Count > 0) {
                PopOrCloseModal(false);
            }
        }

        #endregion

        #region ModalView

        [UIComponent("modal-view"), UsedImplicitly]
        private ModalView _modalView = null!;

        private Transform _container = null!;

        private void InitializeModal() {
            _container = _modalView.transform;
            _modalView.gameObject.AddComponent<VerticalLayoutGroup>();
            _modalView.SetField("_animateParentCanvas", false);

            var contentFitter = _modalView.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var background = _modalView.transform.Find("BG");
            Destroy(background.gameObject);

            var touchable = _modalView.GetComponent<Touchable>();
            DestroyImmediate(touchable);

            var img = _modalView.gameObject.AddComponent<AdvancedImageView>();
            img.sprite = BundleLoader.WhiteBG;
            img.material = GameResources.UIFogBackgroundMaterial;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 13f;

            _modalView.blockerClickedEvent += OnBlockerClicked;
        }

        private void OnBlockerClicked() {
            if (_activeModal is null || !_activeModal.OffClickCloses) return;
            PopOrCloseModal();
        }

        private void ApplyModalScreenSettings(ModalScreenSettings settings) {
            if (_activeModal is not null) return;
            var trans = _modalView.GetComponent<RectTransform>();
            trans.pivot = settings.Pivot;
            trans.localPosition = settings.Position;
            //TODO: asm pub
            _modalView.SetField("_animateParentCanvas", settings.AnimateBackground);
        }

        private void ShowModalScreen(ModalScreenSettings? settings = null) {
            var shouldApplySettings = settings is not null;
            if (shouldApplySettings) {
                ApplyModalScreenSettings(settings!);
            }
            _modalView.Show(true, !shouldApplySettings);
        }

        private void HideModalScreen(bool animated = true, Action? callback = null) {
            _modalView.Hide(animated, callback);
        }

        #endregion
    }
}