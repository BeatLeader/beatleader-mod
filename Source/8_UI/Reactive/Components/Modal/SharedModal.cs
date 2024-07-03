using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class SharedModal<T> : INewModal, IReactiveComponent where T : class, INewModal, IReactiveComponent, new() {
        #region Pool

        protected T BorrowedModal => _borrowedModal ?? throw new InvalidOperationException();

        private static readonly ReactivePool<T> modals = new();
        private T? _borrowedModal;

        private void SpawnModal() {
            if (_borrowedModal != null) return;
            _borrowedModal = modals.Spawn();
            _borrowedModal.ModalClosedEvent += HandleModalClosed;
            _borrowedModal.ModalOpenedEvent += HandleModalOpened;
            _borrowedModal.OpenProgressChangedEvent += HandleOpenProgressChanged;
            OnSpawn();
        }
        
        private void DespawnModal() {
            _borrowedModal!.ModalClosedEvent -= HandleModalClosed;
            _borrowedModal.ModalOpenedEvent -= HandleModalOpened;
            _borrowedModal.OpenProgressChangedEvent -= HandleOpenProgressChanged;
            OnDespawn();
            modals.Despawn(_borrowedModal);
            _borrowedModal = null;
        }
        
        protected virtual void OnSpawn() { }
        protected virtual void OnDespawn() { }
        
        #endregion

        #region Modal Adapter

        public float AnimationProgress => _borrowedModal?.AnimationProgress ?? 0f;

        public event Action<INewModal, bool>? ModalClosedEvent;
        public event Action<INewModal, bool>? ModalOpenedEvent;
        public event Action<INewModal, float>? OpenProgressChangedEvent;

        public void Pause() {
            BorrowedModal.Pause();
        }

        public void Resume() {
            BorrowedModal.Resume();
        }

        public virtual void Close(bool immediate) {
            BorrowedModal.Close(immediate);
        }

        public void Open(bool immediate) {
            SpawnModal();
            BorrowedModal.Open(immediate);
        }
        
        protected virtual void OnOpenInternal(bool finished) { }
        protected virtual void OnCloseInternal(bool finished) { }

        #endregion

        #region ReactiveComponent Adapter

        public GameObject Content => BorrowedModal.Content;

        public RectTransform ContentTransform => BorrowedModal.ContentTransform;

        public bool IsDestroyed => BorrowedModal.IsDestroyed;

        public bool IsInitialized => BorrowedModal.IsInitialized;

        public bool Enabled {
            get => BorrowedModal.Enabled;
            set => BorrowedModal.Enabled = value;
        }

        public GameObject Use(Transform? parent) {
            SpawnModal();
            return BorrowedModal.Use(parent);
        }

        #endregion

        #region Callbacks

        private void HandleModalClosed(INewModal modal, bool finished) {
            OnCloseInternal(finished);
            if (finished) DespawnModal();
            ModalClosedEvent?.Invoke(this, finished);
        }

        private void HandleModalOpened(INewModal modal, bool finished) {
            OnOpenInternal(finished);
            ModalOpenedEvent?.Invoke(this, finished);
        }

        private void HandleOpenProgressChanged(INewModal modal, float progress) {
            OpenProgressChangedEvent?.Invoke(this, progress);
        }

        #endregion
    }
}