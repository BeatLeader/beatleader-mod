using BeatLeader.Models;

namespace BeatLeader.UI.Replayer {
    internal record AvatarPartConfigWithModel(
        IVirtualPlayerBodyPartModel PartModel,
        IVirtualPlayerBodyPartConfig PartConfig
    );
}