using Zenject;

namespace BeatLeader.Models {
    internal abstract class LateTickablePoolItem : TickablePoolItemBase, ILateTickable {
        protected sealed override void InitializeTickable(TickableManager tickableManager) => tickableManager.AddLate(this);

        protected sealed override void DisposeTickable(TickableManager tickableManager) => tickableManager.RemoveLate(this);

        public abstract void LateTick();
    }
}