namespace BeatLeader.Models {
    /// <summary>
    /// A sub abstraction for mods that want to change the avatar but dont wanna touch the sabers
    /// </summary>
    public interface IVirtualPlayerAvatarSpawner : IVirtualPlayerBodyComponentSpawner {
        IVirtualPlayerAvatar SpawnAvatar(IVirtualPlayersManager playersManager, IVirtualPlayerBase player);
        void DespawnAvatar(IVirtualPlayerAvatar avatar);
    }
}