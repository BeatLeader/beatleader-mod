using System.Collections.Generic;

namespace BeatLeader.Models.AbstractReplay {
    public class GenericReplay : IReplay {
        public GenericReplay(
            IReplayData replayData,
            IReadOnlyList<PlayerMovementFrame> movementFrames,
            IReadOnlyList<NoteEvent> noteEvents,
            IReadOnlyList<WallEvent> wallEvents,
            IReadOnlyList<PauseEvent> pauseEvents) 
            { 
            ReplayData = replayData;
            PlayerMovementFrames = movementFrames;
            NoteEvents = noteEvents;
            WallEvents = wallEvents;
            PauseEvents = pauseEvents;
        }

        public IReplayData ReplayData { get; }
        public IReadOnlyList<PlayerMovementFrame> PlayerMovementFrames { get; }
        public IReadOnlyList<NoteEvent> NoteEvents { get; }
        public IReadOnlyList<WallEvent> WallEvents { get; }
        public IReadOnlyList<PauseEvent> PauseEvents { get; }
    }
}
