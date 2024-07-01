using System;

namespace BeatLeader.Components {
    internal interface IModalContext {
        event Action? ContextUpdatedEvent;
    }

    internal interface IPersistentModal : IModal { }

    internal interface IPersistentModal<T, in TContext> : IPersistentModal where T : ReeUIComponentV3<T>, IPersistentModal<T, TContext> {
        TContext? Context { set; }
    }

    internal abstract class PersistentModalComponentBase<T, TContext> : ModalComponentBase<T>, IPersistentModal<T, TContext>
        where T : ReeUIComponentV3<T>, IPersistentModal<T, TContext>
        where TContext : IModalContext {
        #region Context

        TContext? IPersistentModal<T, TContext>.Context {
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