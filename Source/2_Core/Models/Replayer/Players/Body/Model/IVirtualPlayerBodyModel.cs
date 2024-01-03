using System.Collections.Generic;

namespace BeatLeader.Models {
    public interface IVirtualPlayerBodyModel {
        IReadOnlyCollection<IVirtualPlayerBodyPartModel> Parts { get; }
    }
}