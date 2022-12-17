using System;

namespace BeatLeader.Models {
    public interface IBeatmapTimeController {
        float SongTime { get; }
        float SongEndTime { get; }
        float SongStartTime { get; }
        float SongSpeedMultiplier { get; }

        event Action<float> SongSpeedWasChangedEvent;
        event Action<float> EarlySongWasRewoundEvent;
        event Action<float> SongWasRewoundEvent;

        void Rewind(float time, bool resumeAfterRewind = true);
        void SetSpeedMultiplier(float speedMultiplier, bool resumeAfterSpeedChange = true);
    }
}
