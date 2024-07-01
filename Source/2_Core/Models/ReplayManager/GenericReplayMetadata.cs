using System.Collections.Generic;

namespace BeatLeader.Models {
    internal class GenericReplayMetadata : IReplayMetadata {
        public ICollection<IReplayTag> Tags { get; } = new HashSet<IReplayTag>();
    }
}