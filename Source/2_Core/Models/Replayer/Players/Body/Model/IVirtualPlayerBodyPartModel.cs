using System.Collections.Generic;

namespace BeatLeader.Models {
    public interface IVirtualPlayerBodyPartModel {
        IReadOnlyCollection<IVirtualPlayerBodyPartSegmentModel> Segments { get; }
        string Name { get; }
        string Id { get; }
    }
}