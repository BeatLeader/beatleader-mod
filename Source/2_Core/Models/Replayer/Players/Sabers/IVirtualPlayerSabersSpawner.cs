namespace BeatLeader.Models {
    /// <summary>
    /// A sub abstraction for mods that want to change the sabers but dont wanna touch the avatar
    /// </summary>
    public interface IVirtualPlayerSabersSpawner : IVirtualPlayerBodyComponentSpawner {
        IVirtualPlayerSabers SpawnSabers(IVirtualPlayersManager playersManager, IVirtualPlayerBase player);
        void DespawnSabers(IVirtualPlayerSabers sabers);
    }
}