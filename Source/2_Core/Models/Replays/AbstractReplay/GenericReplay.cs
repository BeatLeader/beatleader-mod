using System.Collections.Generic;

namespace BeatLeader.Models.AbstractReplay {
    public class GenericReplay : IReplay {
        public GenericReplay(
            IReplayData replayData,
            BattleRoyaleReplayData? optionalReplayData,
            IReadOnlyList<PlayerMovementFrame> movementFrames,
            IReadOnlyList<NoteEvent> noteEvents,
            IReadOnlyList<WallEvent> wallEvents,
            IReadOnlyList<PauseEvent> pauseEvents,
            IReadOnlyList<HeightEvent>? heightEvents,
            IReadOnlyDictionary<string, byte[]> customData
        ) {
            ReplayData = replayData;
            OptionalReplayData = optionalReplayData;
            PlayerMovementFrames = movementFrames;
            NoteEvents = noteEvents;
            WallEvents = wallEvents;
            PauseEvents = pauseEvents;
            HeightEvents = heightEvents;
            CustomData = customData;
        }

        public IReplayData ReplayData { get; }
        public BattleRoyaleReplayData? OptionalReplayData { get; }
        public IReadOnlyList<PlayerMovementFrame> PlayerMovementFrames { get; }
        public IReadOnlyList<NoteEvent> NoteEvents { get; }
        public IReadOnlyList<WallEvent> WallEvents { get; }
        public IReadOnlyList<PauseEvent> PauseEvents { get; }

        public IReadOnlyList<HeightEvent>? HeightEvents { get; }
        public IReadOnlyDictionary<string, byte[]> CustomData { get; }
    }
}