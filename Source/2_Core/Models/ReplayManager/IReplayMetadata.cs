using System.Collections.Generic;

namespace BeatLeader.Models {
    public interface IReplayMetadata {
        ICollection<IReplayTag> Tags { get; }
    }
}