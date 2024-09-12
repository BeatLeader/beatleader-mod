using System;
using Reactive;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal interface ISharedModal : IModal { }
    
    internal class SharedModal<T> : ISharedModal, IReactiveComponent, ILayoutItem where T : class, IModal, IReactiveComponent, new() {
        #region Pool

        public bool BuildImmediate {
            // ReSharper disable once ValueParameterNotUsed
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

        public event Action<IModal, bool>? ModalClosedEvent;
        public event Action<IModal, bool>? ModalOpenedEvent;
        public event Action<IModal, float>? OpenProgressChangedEvent;

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

        private void HandleModalClosed(IModal modal, bool finished) {
            OnCloseInternal(finished);
            if (finished) DespawnModal();
            ModalClosedEvent?.Invoke(this, finished);
        }

        private void HandleModalOpened(IModal modal, bool finished) {
            OnOpenInternal(finished);
            ModalOpenedEvent?.Invoke(this, finished);
        }

        private void HandleOpenProgressChanged(IModal modal, float progress) {
            OpenProgressChangedEvent?.Invoke(this, progress);
        }

        #endregion
    }
}