using System;
using System.Collections.Generic;

namespace BeatLeader.Models {
    public interface IReplayMetadata {
        ICollection<IReplayTag> Tags { get; }

        event Action<IReplayTag>? TagAddedEvent;
        event Action<IReplayTag>? TagRemovedEvent;
    }
}