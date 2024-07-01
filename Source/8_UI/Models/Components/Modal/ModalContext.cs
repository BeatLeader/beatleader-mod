using System;
using BeatLeader.Components;

namespace BeatLeader.UI.Components.Models {
    internal abstract class ModalContext : IModalContext {
        public event Action? ContextUpdatedEvent;
        
        protected void NotifyContextUpdated() {
            ContextUpdatedEvent?.Invoke();
        }
    }
}