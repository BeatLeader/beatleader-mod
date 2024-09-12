﻿using System;
using System.Collections.Generic;
using BeatLeader.Models.AbstractReplay;

namespace BeatLeader.Models {
    public interface IReplayBeatmapEventsProcessor : IEventsProcessor {
        event Action<LinkedListNode<NoteEvent>>? NoteEventDequeuedEvent;
        event Action<LinkedListNode<WallEvent>>? WallEventDequeuedEvent;
    }
}