namespace BeatLeader.Models {
    public interface IVRControllersManager {
        IVRControllersProvider SpawnControllersProvider(bool primary);
        void DespawnControllersProvider(IVRControllersProvider provider);
    }
}