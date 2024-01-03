namespace BeatLeader.Models {
    public interface IVirtualPlayer : IVirtualPlayerBase {
        IVirtualPlayerBody Body { get; }
        IVirtualPlayerSabers Sabers { get; }
    }
}