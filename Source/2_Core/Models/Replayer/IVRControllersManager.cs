namespace BeatLeader.Models {
    public interface IVRControllersManager {
        float ControllersIntensity { get; set; }
        
        IVRControllersProvider SpawnControllers(IVirtualPlayerBase player, bool primary);
        void DespawnControllers(IVRControllersProvider provider);
    }
}