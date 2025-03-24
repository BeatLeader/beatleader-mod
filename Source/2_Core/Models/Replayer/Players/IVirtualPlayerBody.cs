using JetBrains.Annotations;

namespace BeatLeader.Models {
    /// <summary>
    /// Determines a player body instance.
    /// </summary>
    [PublicAPI]
    public interface IVirtualPlayerBody : IVirtualPlayerPoseReceiver {}
}