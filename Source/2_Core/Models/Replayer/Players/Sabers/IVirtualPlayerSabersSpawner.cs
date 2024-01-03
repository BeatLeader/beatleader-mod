namespace BeatLeader.Models {
    public interface IVirtualPlayerSabersSpawner {
        IVirtualPlayerSabers SpawnSabers(IVirtualPlayerBase player);
        void DespawnSabers(IVirtualPlayerSabers sabers);
    }
}