using System;

namespace BeatLeader.UI.Hub.Models {
    internal interface IBeatmapReplayFilterData {
        IPreviewBeatmapLevel? BeatmapLevel { get; }
        BeatmapCharacteristicSO? BeatmapCharacteristic { get; }
        BeatmapDifficulty? BeatmapDifficulty { get; }

        event Action DataUpdatedEvent;
    }
}