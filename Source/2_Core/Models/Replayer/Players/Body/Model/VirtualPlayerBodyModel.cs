using System.Collections.Generic;
using BeatLeader.Models;

namespace BeatLeader.Replayer.Emulation {
    public record VirtualPlayerBodyModel(
        string Name,
        IReadOnlyCollection<IVirtualPlayerBodyPartModel> Parts
    ) : IVirtualPlayerBodyModel;
}