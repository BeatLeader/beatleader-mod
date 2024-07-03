namespace BeatLeader.UI.Reactive.Components {
    internal class ContextModal<T, TContext> : SharedModal<T>
        where T : class, IContextModal<TContext>, IReactiveComponent, new()
        where TContext : IModalContext {
        
        public TContext? Context { get; set; }

        protected override void OnOpenInternal(bool finished) {
            if (!finished) return;
            BorrowedModal.Context = Context;
        }

        protected override void OnCloseInternal(bool finished) {
            if (!finished) return;
            BorrowedModal.Context = default;
        }
    }
}