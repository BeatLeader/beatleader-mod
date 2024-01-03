namespace BeatLeader.Models {
    public interface IVirtualPlayerBodySpawner {
        IVirtualPlayerBodyModel BodyModel { get; }

        IVirtualPlayerBody SpawnBody(IVirtualPlayerBase player);
        void DespawnBody(IVirtualPlayerBody body);
    }
}