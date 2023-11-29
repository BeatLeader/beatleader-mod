using System.Collections.Generic;

namespace BeatLeader.Models.AbstractReplay {
    public interface IReplay {
        IReplayData ReplayData { get; }

        IReadOnlyList<PlayerMovementFrame> PlayerMovementFrames { get; }
        IReadOnlyList<NoteEvent> NoteEvents { get; }
        IReadOnlyList<WallEvent> WallEvents { get; }
        IReadOnlyList<PauseEvent> PauseEvents { get; }
        IReadOnlyList<HeightEvent>? HeightEvents { get; }
    }
}
