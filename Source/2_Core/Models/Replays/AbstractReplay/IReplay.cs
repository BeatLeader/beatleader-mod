using System.Collections.Generic;
using JetBrains.Annotations;

namespace BeatLeader.Models.AbstractReplay {
    [PublicAPI]
    public interface IReplay {
        IReplayData ReplayData { get; }
        IReplayNoteComparator NoteComparator { get; }
        BattleRoyaleReplayData? OptionalReplayData { get; }

        IReadOnlyList<PlayerMovementFrame> PlayerMovementFrames { get; }
        IReadOnlyList<NoteEvent> NoteEvents { get; }
        IReadOnlyList<WallEvent> WallEvents { get; }
        IReadOnlyList<PauseEvent> PauseEvents { get; }
        IReadOnlyList<HeightEvent>? HeightEvents { get; }
        IReadOnlyDictionary<string, byte[]> CustomData { get; }
    }
}
