using System;

namespace BeatLeader.UI.Reactive.Components {
    internal interface IModalContext {
        event Action? ContextUpdatedEvent;
    }

    internal interface IPersistentModal : IModal { }

    internal interface IPersistentModal<in TContext> : IPersistentModal {
        TContext? Context { set; }
    }

    internal abstract class PersistentModalComponentBase<TContext> : ModalComponentBase, IPersistentModal<TContext>
        where TContext : IModalContext {
        #region Context

        TContext? IPersistentModal<TContext>.Context {
            set {
                if (_context is not null) {
                    _context.ContextUpdatedEvent -= HandleContextUpdated;
                }
                _context = value;
                if (_context is null) return;
                _context.ContextUpdatedEvent += HandleContextUpdated;
                HandleContextUpdated();
            }
        }

        protected TContext Context => _context ?? throw new UninitializedComponentException();

        private bool _ignoreContextUpdates;
        private TContext? _context;

        private void HandleContextUpdated() {
            if (!_ignoreContextUpdates) OnContextUpdate();
        }

        protected void UpdateContext(Action action) {
            _ignoreContextUpdates = true;
            action();
            _ignoreContextUpdates = false;
        }

        protected virtual void OnContextUpdate() { }

        #endregion
    }
}