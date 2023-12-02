using System;
using System.Collections.Generic;
using BeatLeader.Models.AbstractReplay;

namespace BeatLeader.Models {
    public interface IReplayScoreEventsProcessor : IEventsProcessor {
        LinkedListNode<ScoreEvent> CurrentScoreEvent { get; }
        
        event Action<LinkedListNode<ScoreEvent>>? ScoreEventDequeuedEvent;
    }
}