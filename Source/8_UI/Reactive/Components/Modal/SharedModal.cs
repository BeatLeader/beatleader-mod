using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal interface ISharedModal : INewModal { }
    
    internal class SharedModal<T> : ISharedModal, IReactiveComponent, ILayoutItem where T : class, INewModal, IReactiveComponent, new() {
        #region Pool

        public bool BuildImmediate {
            set => modals.Preload(1);
        }
        
        public T Modal => _modal ?? throw new InvalidOperationException();

        private static readonly ReactivePool<T> modals = new();
        private T? _modal;

        private void SpawnModal() {
            if (_modal != null) return;
            _modal = modals.Spawn();
            _modal.ModalClosedEvent += HandleModalClosed;
            _modal.ModalOpenedEvent += HandleModalOpened;
            _modal.OpenProgressChangedEvent += HandleOpenProgressChanged;
            OnSpawn();
        }

        private void DespawnModal() {
            _modal!.ModalClosedEvent -= HandleModalClosed;
            _modal.ModalOpenedEvent -= HandleModalOpened;
            _modal.OpenProgressChangedEvent -= HandleOpenProgressChanged;
            OnDespawn();
            modals.Despawn(_modal);
            _modal = null;
        }

        protected virtual void OnSpawn() { }
        protected virtual void OnDespawn() { }

        #endregion

        #region Modal Adapter

        public float AnimationProgress => _modal?.AnimationProgress ?? 0f;

        public event Action<INewModal, bool>? ModalClosedEvent;
        public event Action<INewModal, bool>? ModalOpenedEvent;
        public event Action<INewModal, float>? OpenProgressChangedEvent;

        public void Pause() {
            Modal.Pause();
        }

        public void Resume() {
            Modal.Resume();
        }

        public virtual void Close(bool immediate) {
            Modal.Close(immediate);
        }

        public void Open(bool immediate) {
            SpawnModal();
            Modal.Open(immediate);
        }

        protected virtual void OnOpenInternal(bool finished) { }
        protected virtual void OnCloseInternal(bool finished) { }

        #endregion

        #region ReactiveComponent Adapter

        public GameObject Content => Modal.Content;

        public RectTransform ContentTransform => Modal.ContentTransform;

        public bool IsDestroyed => Modal.IsDestroyed;

        public bool IsInitialized => Modal.IsInitialized;

        public bool Enabled {
            get => Modal.Enabled;
            set => Modal.Enabled = value;
        }

        public GameObject Use(Transform? parent) {
            SpawnModal();
            return Modal.Use(parent);
        }

        #endregion

        #region LayoutItem

        public bool Equals(ILayoutItem other) {
            return other == this;
        }

        public ILayoutDriver? LayoutDriver { get; set; }
        public ILayoutModifier? LayoutModifier { get; set; }
        public float? DesiredHeight => null;
        public float? DesiredWidth => null;
        public bool WithinLayout { get; set; }
        public event Action<ILayoutItem>? ModifierUpdatedEvent;

        public void ApplyTransforms(Action<RectTransform> applicator) { }

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