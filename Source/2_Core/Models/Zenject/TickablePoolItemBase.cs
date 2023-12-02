using Zenject;

namespace BeatLeader.Models {
    internal abstract class TickablePoolItemBase {
        [Inject] private readonly TickableManager _tickableManager = null!;

        protected void InitializeTickable() => InitializeTickable(_tickableManager);
        
        protected void DisposeTickable() => DisposeTickable(_tickableManager);

        protected abstract void InitializeTickable(TickableManager tickableManager);
        
        protected abstract void DisposeTickable(TickableManager tickableManager);
    }
}