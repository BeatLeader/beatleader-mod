using System;
using System.Collections.Generic;

namespace BeatLeader.Models {
    public interface IReplayTagManager {
        IReadOnlyCollection<IEditableReplayTag> Tags { get; }

        event Action<IEditableReplayTag>? TagCreatedEvent;
        event Action<IEditableReplayTag>? TagDeletedEvent;
        
        ReplayTagValidationResult ValidateTag(string name);
        IEditableReplayTag CreateTag(string name);
    }
}