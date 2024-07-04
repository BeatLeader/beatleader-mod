using System;

namespace BeatLeader.UI.Reactive.Components {
    internal interface IModalContext {
        event Action? ContextUpdatedEvent;
    }

    internal interface IContextModal : INewModal { }

    internal interface IContextModal<in TContext> : IContextModal where TContext : IModalContext {
        TContext? Context { set; }
    }
    
    internal class ContextModalComponentBase<TContext> : AnimatedModalComponentBase, IContextModal<TContext> where TContext : IModalContext {
        #region Context

        TContext? IContextModal<TContext>.Context {
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
        protected bool HasContext => _context != null;
        
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