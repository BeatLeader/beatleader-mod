namespace BeatLeader.Models {
    public interface IVRControllersManager {
        IVRControllersProvider SpawnControllers(IVirtualPlayerBase player, bool primary);
        
        void DespawnControllers(IVRControllersProvider provider);
    }
}