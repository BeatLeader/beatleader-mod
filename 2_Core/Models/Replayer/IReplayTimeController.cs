using System;

namespace BeatLeader.Models {
    public interface IReplayTimeController : IBeatmapTimeController {
        float ReplayEndTime { get; }

        event Action SongReachedReplayEndEvent;
    }
}
