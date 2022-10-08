using System;

namespace BeatLeader.Models
{
    public interface IBeatmapTimeController
    {
        float SongTime { get; }
        float TotalSongTime { get; }
        float SongSpeedMultiplier { get; }

        void Rewind(float time, bool resumeAfterRewind = true);
        void SetSpeedMultiplier(float speedMultiplier, bool resumeAfterSpeedChange = true);
    }
}
