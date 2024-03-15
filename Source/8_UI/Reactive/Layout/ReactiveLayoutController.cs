namespace BeatLeader.UI.Reactive {
    internal abstract class ReactiveLayoutController : IApplicableLayoutController {
        protected ILayoutItem? Item { get; private set; }

        public void Setup(ILayoutItem item) {
            Item = item;
        }

        public void RefreshChildren() {
            OnChildrenUpdated();
        }

        public abstract void Recalculate();

        protected virtual void OnChildrenUpdated() { }
    }
}