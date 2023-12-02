using Zenject;

namespace BeatLeader.Models {
    internal abstract class TickablePoolItem : TickablePoolItemBase, ITickable {
        protected sealed override void InitializeTickable(TickableManager tickableManager) => tickableManager.Add(this);

        protected sealed override void DisposeTickable(TickableManager tickableManager) => tickableManager.Remove(this);

        public abstract void Tick();
    }
}