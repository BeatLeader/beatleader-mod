using System;

namespace BeatLeader.Models {
    public interface IEventsProcessor {
        bool CurrentEventHasTimeMismatch { get; }
        bool QueueIsBeingAdjusted { get; }
        
        event Action? EventQueueAdjustStartedEvent;
        event Action? EventQueueAdjustFinishedEvent;
    }
}